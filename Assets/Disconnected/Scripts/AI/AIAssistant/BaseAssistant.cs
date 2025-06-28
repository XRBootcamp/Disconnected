using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using Sirenix.OdinInspector;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Overlays;
#endif

[RequireComponent(typeof(MicRecorder))]
[RequireComponent(typeof(WhisperTranscriber))]
[RequireComponent(typeof(GroqTTS))]
[RequireComponent(typeof(AudioSource))]
public abstract class BaseAssistant : MonoBehaviour
{
    public enum State
    {
        None = 0, // when nothing (default)
        Selected = 1, // when selected by user
        OnHold = 2 // when calling the API
    }
    [SerializeField] protected MicRecorder micRecorder;
    [SerializeField] protected WhisperTranscriber speech2TextAI;
    [SerializeField] protected GroqTTS assistantTextToSpeechAI;
    [SerializeField] protected AudioSource assistantAudioSource;
    protected AssistantReasoningController _reasoningAIService;

    [Space]
    [Header("Debug")]
    [SerializeField] protected AIClientToggle aiClientToggle;
    [Space]
    [SerializeField] protected State state;

    public abstract BaseConfig Config {get; set; }
    protected bool _isRecordingUser;

    [SerializeField] protected AudioClip micRecording;
    [SerializeField, TextArea(5, 20)] protected string userIntent;
    [SerializeField, TextArea(5, 20)] protected string assistantResponse;
    [SerializeField, TextArea(2, 20)] protected string toolOutput;
    [SerializeField, TextArea(2, 20)] protected string errorOutput;

    // TODO: these unity events we will see what makes sense
    public UnityEvent<string> onApiResponse;
    public UnityEvent<string> onApiRequest;
    public UnityEvent<string> onSelected;
    public UnityEvent<string> onUnselected;
    public UnityEvent<string> onClosing;

    public State CurrentState { get => state; set => state = value; }


    public string Id { get; private set; }

    public abstract string DisplayName {get;}

    /// <summary>
    /// To populate everything I can right away
    /// </summary>
    protected virtual void OnValidate()
    {
        micRecorder = GetComponent<MicRecorder>();
        speech2TextAI = GetComponent<WhisperTranscriber>();
        // NOTE: to avoid on validate bug if moving the components around - we do have base class GroqTTS (for the assistant)
        // and in voice characters we have a GroqFilteredTTS
        var components = GetComponents<GroqTTS>();
        assistantTextToSpeechAI = components.FirstOrDefault(c => c.GetType() == typeof(GroqTTS));
        assistantAudioSource = GetComponent<AudioSource>();
    }

    protected virtual void Start()
    {
        if (APIKeyLoader.Instance == null)
        {
            Debug.LogError("APIKeyLoader instance not found!");
            return;
        }
        // TODO: load files - create my assistant 
        // NOTE: events  
        micRecorder.onRecordedAudio.AddListener(UserRecordedIntent);
        micRecorder.onDurationPassed.AddListener(DiscardMicRecording);
    }

    // TODO: if needed
    public abstract void Dispose(); // cleanup if needed

    // NOTE: to simplify the process
    protected virtual void OnDestroy()
    {
        onApiResponse.RemoveAllListeners();
        onApiRequest.RemoveAllListeners();
        onSelected.RemoveAllListeners();
        onUnselected.RemoveAllListeners();
        onClosing.RemoveAllListeners();
    }

    public virtual void Initialize(string id, BaseConfig config, AIGameSettings gameSettings)
    {
        Id = id;
        Config = config;

        MicRecorderManager.Instance.RegisterRecorder(micRecorder);
        assistantTextToSpeechAI.SetVoice(gameSettings.aiAssistantVoice);

        _reasoningAIService = new AssistantReasoningController(
            model: gameSettings.aiReasoningModel,
            currentSystemMessage: null,
            image2TextModelName: null,
            buildTextToImagePromptDescription: null,
            userRequestIntro: null,
            explainRuleSystemDescription: null,
            tools: null
        );
    }

    #region Handle User Recordings
    public virtual void ToggleRecording()
    {
        if (!_isRecordingUser)
        {
            StartRecordingUser();
        }
        else
        {
            StopRecording();
        }
    }

    protected virtual void StartRecordingUser()
    {
        // NOTE: only record when it is over
        if (state == State.OnHold) return;

        state = State.OnHold;
        micRecorder.StartRecording();
        _isRecordingUser = true;
    }

    protected virtual void StopRecording()
    {
        // this triggers the onRecordedAudio once it is saved and we can proceed with everything
        // from UserRecordedIntent
        micRecorder.StopAndSave();
        _isRecordingUser = false;
    }

    #endregion

    #region AI-Assistant-Main-Methods

    public void SetUserIntent(string newIntent)
    {
        Config.UserIntent = newIntent;
    }

    // TODO: unsure if this is the intended behaviour when mic audio duration surpassed the full possible duration
    protected virtual void DiscardMicRecording(AudioClip clip = null)
    {
        // debug
        micRecording = null;

        // reset state once it has been handled
        state = AssistantManager.Instance.SetStateAfterOnHold(this);
    }

    /// <summary>
    /// Method that will start the process to make request to assistant 
    /// </summary>
    /// <param name="arg0">ignored</param>
    protected virtual void UserRecordedIntent(AudioClip arg0)
    {
        // debug
        micRecording = micRecorder.GetLastAudioClip();

        // Start the async processing
        _ = ProcessUserRecordedIntentAsync();
    }

    /// <summary>
    /// Async method that handles the actual processing of user recorded intent
    /// </summary>
    protected virtual async Task ProcessUserRecordedIntentAsync()
    {
        try
        {
            // transcribe user intent from mic recording
            userIntent = await speech2TextAI.TranscribeAsync(micRecorder.GetLastFilePath());

            // NOTE: this method does all the Heavy Lifting
            await MakeRequestToAssistant();
        }
        catch (Exception e)
        {
            Debug.LogError($"[{nameof(BaseAssistant)}] Error processing user recorded intent: {e}");
        }
        finally
        {
            // reset state once it has been handled
            state = AssistantManager.Instance.SetStateAfterOnHold(this);
        }
    }

    protected virtual async Task MakeRequestToAssistant()
    {
        // NOTE: this method does all the Heavy Lifting
        //toolOutput = await _reasoningAIService.RunReasoningAsync(chatData, userIntent);

        Debug.Log($"{nameof(MakeRequestToAssistant)} - to be implemented soon");
        await Task.Yield();
    }

    protected Task<GroqTTS> SetAssistantResponse(string assistantResponse)
    {
        try
        {
            assistantAudioSource.Stop();
            return assistantTextToSpeechAI.GenerateAndPlaySpeech(assistantResponse, assistantAudioSource, FileEnumPath.None);
        }
        catch (Exception e)
        {
            return null;
        }
    }

    // TODO: in the future add context to error
    protected async void AssistantOnErrorOccurred(string context = null)
    {
        try
        {
            await SetAssistantResponse(AssistantSpeechSnippets.ErrorInterjections.GetRandomEntry());
        }
        catch (Exception e)
        {

            return;
        }

    }

    #endregion

    #region State-Changes - TODO if needed
    public void OnSelect()
    {
        // Use the singleton instance
        AssistantManager.Instance.SelectAssistant(Id);
        onSelected.Invoke(Id);
    }

    public void OnUnselect()
    {
        // Use the singleton instance
        AssistantManager.Instance.TryUnselectAssistant(Id);
        onUnselected.Invoke(Id);
    }

    public void ClosingChat()
    {
        if (state == State.OnHold)
        {
            // TODO: Implement UI Feature of can't close while running
            return;
        }

        // Use the singleton instance
        AssistantManager.Instance.RemoveAssistant(Id);
        MicRecorderManager.Instance.UnregisterRecorder(micRecorder);
        onClosing.Invoke(Id);

        // TODO: UI/Animation - destroy the object clearly

        Destroy(gameObject);
    }

    #endregion


    #region Buttons

    [Button]
    protected virtual void FirstStartRecording()
    {
        // NOTE: Fake Mic Recording
        if (AIClientFakes.TryHandleFakeRecorder(aiClientToggle, SetFakeMicRecording))
        {
            return;
        }

        StartRecordingUser();
    }

    [Button]
    protected virtual void SecondStopRecordingAndProcessIntent()
    {
        // NOTE: Fake Mic Recording
        if (AIClientFakes.TryHandleFakeRecorder(aiClientToggle, SetFakeMicRecording))
        {
            return;
        }

        StopRecording();
    }

    protected virtual void SetFakeMicRecording(AudioClip newClip)
    {
#if UNITY_EDITOR
        micRecorder.OverrideAudioClipAndPath(newClip);
#endif
        micRecorder.onRecordedAudio.Invoke(newClip);
    }

    #endregion


}