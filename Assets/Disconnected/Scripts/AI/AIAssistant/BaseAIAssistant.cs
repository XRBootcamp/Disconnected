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
public abstract class BaseAIAssistant : MonoBehaviour
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
    protected AIAssistantReasoningController _reasoningAIService;

    [Space]
    [Header("Debug")]
    [SerializeField] protected AIClientToggle aiClientToggle;
    [Space]
    [SerializeField] protected State state;

    [SerializeField] protected AudioClip micRecording;
    [SerializeField, TextArea(5, 20)] protected string userIntent;
    [SerializeField, TextArea(5, 20)] protected string assistantResponse;
    [SerializeField, TextArea(2, 20)] protected string toolOutput;
    [SerializeField, TextArea(2, 20)] protected string errorOutput;

    // TODO: these unity events we will see what makes sense
    public UnityEvent<BaseAIAssistant> onApiResponse;
    public UnityEvent<BaseAIAssistant> onApiRequest;
    public UnityEvent<BaseAIAssistant> onSelected;
    public UnityEvent<BaseAIAssistant> onUnselected;
    public UnityEvent<BaseAIAssistant> onClosing;

    public State CurrentState { get => state; set => state = value; }


    /// <summary>
    /// To populate everything I can right away
    /// </summary>
    protected virtual void OnValidate()
    {
        micRecorder = GetComponent<MicRecorder>();
        speech2TextAI = GetComponent<WhisperTranscriber>();
        assistantTextToSpeechAI = GetComponent<GroqTTS>();
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

    // NOTE: to simplify the process
    protected virtual void OnDestroy()
    {
        onApiResponse.RemoveAllListeners();
        onApiRequest.RemoveAllListeners();
        onSelected.RemoveAllListeners();
        onUnselected.RemoveAllListeners();
        onClosing.RemoveAllListeners();
    }

    public virtual void Initialize(AIGameSettings gameSettings)
    {
        MicRecorderManager.Instance.RegisterRecorder(micRecorder);
        assistantTextToSpeechAI.SetVoice(gameSettings.aiAssistantVoice);

        _reasoningAIService = new AIAssistantReasoningController(
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
    public virtual void StartRecordingUser()
    {
        // NOTE: only record when it is over
        if (state == State.OnHold) return;

        state = State.OnHold;
        micRecorder.StartRecording();
    }

    public virtual void StopRecording()
    {
        // this triggers the onRecordedAudio once it is saved and we can proceed with everything
        // from UserRecordedIntent
        micRecorder.StopAndSave();
    }

    #endregion

    #region AI-Assistant-Main-Methods

    // TODO: unsure if this is the intended behaviour when mic audio duration surpassed the full possible duration
    protected virtual void DiscardMicRecording(AudioClip clip = null)
    {
        // debug
        micRecording = null;

        // reset state once it has been handled
        state = AIAssistantManager.Instance.SetStateAfterOnHold(this);
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
            Debug.LogError($"[{nameof(BaseAIAssistant)}] Error processing user recorded intent: {e}");
        }
        finally
        {
            // reset state once it has been handled
            state = AIAssistantManager.Instance.SetStateAfterOnHold(this);
        }
    }

    protected virtual async Task MakeRequestToAssistant()
    {
        // NOTE: this method does all the Heavy Lifting
        //toolOutput = await _reasoningAIService.RunReasoningAsync(chatData, userIntent);

        Debug.Log($"{nameof(MakeRequestToAssistant)} - to be implemented soon");
        await Task.Yield();
    }

    protected Task<GroqTTS> AssistantAnswer(string assistantResponse)
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
            await AssistantAnswer(AssistantSpeechSnippets.ErrorInterjections.GetRandomEntry());
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
        AIAssistantManager.Instance.SelectAssistant(this);
        onSelected.Invoke(this);
    }

    public void OnUnselect()
    {
        // Use the singleton instance
        AIAssistantManager.Instance.TryUnselectAssistant(this);
        onUnselected.Invoke(this);
    }

    public void ClosingChat()
    {
        if (state == State.OnHold)
        {
            // TODO: Implement UI Feature of can't close while running
            return;
        }

        // Use the singleton instance
        AIAssistantManager.Instance.RemoveAssistant(this);
        MicRecorderManager.Instance.UnregisterRecorder(micRecorder);
        onClosing.Invoke(this);

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