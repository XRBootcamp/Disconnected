using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Runware;
using UnityEngine;
using UnityEngine.Events;
using NaughtyAttributes;
using UnityEditor;
using TMPro;
using System.Linq;
using UnityEditor.Overlays;
using System.Text.Json;
using Assets.Disconnected.Scripts.AI.AIAssistant.API;
using System.Linq.Expressions;

[RequireComponent(typeof(MicRecorder))]
[RequireComponent(typeof(WhisperTranscriber))]
[RequireComponent(typeof(GroqTTS))]
[RequireComponent(typeof(RunwareTTI))]
[RequireComponent(typeof(AudioSource))]
public class AIAssistant : MonoBehaviour
{
    public enum State
    {
        None = 0, // when nothing (default)
        Selected = 1, // when selected by user
        OnHold = 2 // when calling the API
    }

    [SerializeField] private MicRecorder micRecorder;

    [Header("AIs")]
    [SerializeField] private WhisperTranscriber speech2TextAI;

    [SerializeField] private GroqTTS textToSpeechAI;
    [SerializeField] private RunwareTTI textToImageAI;

    [Space]
    [Header("AI Reasoning")]
    private AIAssistantReasoningController _reasoningAI;
    [SerializeField] private TextAsset reasoningUserPromptIntro;
    [SerializeField] private List<TextAsset> reasoningStaticSystemMessageDocuments;
    [SerializeField] private TextAsset buildingTextToImagePromptSystemDocument;
    [SerializeField] private TextAsset explainRuleSystemDocument;
    [SerializeField] private ChatInternalMemory chatData;

    [Space]
    [Header("Debug")]
    [SerializeField] private AIClientToggle aiClientToggle;

    [Space]
    [SerializeField] private AudioClip micRecording;
    [SerializeField, TextArea(5, 20)] private string userIntent;
    [SerializeField, TextArea(5, 20)] private string assistantResponse;
    [SerializeField, TextArea(2, 20)] private string toolOutput;
    [SerializeField, TextArea(2, 20)] private string errorOutput;

    [SerializeField] private Texture2D lastGeneratedImage;

    private bool _isAssistantVoiceReady;


    [SerializeField] private State state;

    // TODO: these unity events we will see what makes sense
    public UnityEvent<AIAssistant> onApiResponse;
    public UnityEvent<AIAssistant> onApiRequest;
    public UnityEvent<AIAssistant> onSelected;
    public UnityEvent<AIAssistant> onUnselected;
    public UnityEvent<AIAssistant> onClosing;

    public State CurrentState { get => state; set => state = value; }

    private void OnValidate() {
        micRecorder = GetComponent<MicRecorder>();
        speech2TextAI = GetComponent<WhisperTranscriber>();
        textToSpeechAI = GetComponent<GroqTTS>();
        textToImageAI = GetComponent<RunwareTTI>();
        textToSpeechAI.audioSource = GetComponent<AudioSource>();

    }

    void Start()
    {
        // TODO: load files - create my assistant 
        // NOTE: events  
        micRecorder.onRecordedAudio.AddListener(UserRecordedIntent);
        micRecorder.onDurationPassed.AddListener(DiscardMicRecording);
    }

    public void Initialize(AIGameSettings gameSettings)
    {
        MicRecorderManager.Instance.RegisterRecorder(micRecorder);
        textToSpeechAI.SetVoice(gameSettings.aiAssistantVoice);
        textToImageAI.model = gameSettings.textToImageAIModelName;

        _reasoningAI = new AIAssistantReasoningController(
            model: gameSettings.aiReasoningModel,
            currentSystemMessage: null,
            image2TextModelName: gameSettings.textToImageAIModelName.ToString(),
            buildTextToImagePromptDescription: buildingTextToImagePromptSystemDocument.text,
            userRequestIntro: reasoningUserPromptIntro.text,
            explainRuleSystemDescription: explainRuleSystemDocument.text,
            tools: null
        );

        _reasoningAI.CreateSystemMessage(reasoningStaticSystemMessageDocuments.Select(doc => doc.text).ToList());
        _reasoningAI.AddTool(_reasoningAI.BuildTextToImagePrompt(ProcessTextToImagePrompt));
        _reasoningAI.AddTool(_reasoningAI.ExplainRuleSystem(ProcessExplainRuleSystem));
    }

    private async Task<string> ProcessTextToImagePrompt(string arg)
    {
        try
        {
            Debug.Log($"[{nameof(AIAssistant)} - {gameObject.name}] - {nameof(ProcessTextToImagePrompt)} input:\n{arg}");

            var jsonArgs = JsonDocument.Parse(arg);

            var text2ImageResponse = JsonSerializer.Deserialize<AIAssistantText2ImageResponseModel>(jsonArgs.RootElement);

            string assistantResponse = text2ImageResponse.assistantResponse;
            // transform response into new prompt compiler
            var newPromptCompiler = text2ImageResponse.ToPromptCompiler(chatData.promptCompiler);

            Debug.Log($"[{nameof(AIAssistant)} - {gameObject.name}] - {nameof(ProcessTextToImagePrompt)} PromptCompiler:\n{JsonSerializer.Serialize(newPromptCompiler)}");

            var request = newPromptCompiler.ToTextToImageRequestModel();

            var returnString = JsonSerializer.Serialize(new { assistantResponse = $"{assistantResponse}", result = $"{request.ToString()}" });

            Debug.Log($"[{nameof(AIAssistant)} - {gameObject.name}] - {nameof(ProcessTextToImagePrompt)} TextToImageRequestModel:\n{returnString}");
            await textToImageAI.GenerateTextToImage(
                request: request,
                onStartAction: null, // TODO: add voice input
                onCompleteAction: SetLastGeneratedImage,
                onErrorAction: null // TODO: add voice input as well
            );

            // TODO: this part should only happen if it successfully returns stuff
            UnityMainThreadDispatcher.Instance().Enqueue(async () =>
            {
                await textToSpeechAI.GenerateAndPlaySpeech(assistantResponse);

                chatData.promptCompiler = newPromptCompiler;
                chatData.assistantResponse = assistantResponse;

                Debug.Log($"[{nameof(AIAssistant)} - {gameObject.name}] - {nameof(ProcessTextToImagePrompt)} Completed!!!");

                // TODO: UI Display stuff - event invoke

            });
            return returnString;
        }
        catch (Exception e)
        {
            Debug.LogError($"[{nameof(AIAssistant)} - {gameObject.name}] - {nameof(ProcessTextToImagePrompt)} ERROR:\n{e}");
            return null;
        }
    }

    private async Task<string> ProcessExplainRuleSystem(string arg)
    {
        try
        {
            Debug.Log($"[{nameof(AIAssistant)} - {gameObject.name}] - {nameof(ProcessExplainRuleSystem)} input:\n{arg}");

            var jsonArgs = JsonDocument.Parse(arg);
            var jsonModel = JsonSerializer.Deserialize<AssistantResponseOnlyModel>(jsonArgs.RootElement);

            string assistantResponse = jsonModel.assistantResponse;

            var returnString = JsonSerializer.Serialize(jsonModel);
            await textToSpeechAI.GenerateAndPlaySpeech(assistantResponse);

            Debug.Log($"[{nameof(AIAssistant)} - {gameObject.name}] - {nameof(ProcessExplainRuleSystem)} Completed!!!");

            /*
            UnityMainThreadDispatcher.Instance().Enqueue(async () =>
            {
                // TODO: UI Display stuff - event invoke
            });
            */
            return returnString;
        }
        catch (Exception e)
        {
            Debug.LogError($"[{nameof(AIAssistant)} - {gameObject.name}] - {nameof(ProcessExplainRuleSystem)} ERROR:\n{e}");
            return null;
        }
    }

    private void SetLastGeneratedImage(List<Texture2D> newImages)
    {
        lastGeneratedImage = newImages.FirstOrDefault();
    }

    private async void UserRecordedIntent(AudioClip arg0)
    {
        micRecording = micRecorder.GetLastAudioClip();
        await MakeRequestToAssistant();
    }

    // TODO: unsure if this is the intended behaviour when mic audio duration surpassed the full possible duration
    private void DiscardMicRecording(AudioClip clip = null)
    {
        micRecording = null;
        state = AIAssistantManager.Instance.SetStateAfterOnHold(this);
    }


    public async Task MakeRequestToAssistant()
    {
        // TODO: Test these state transitions - recording multiple windows. 
        state = State.OnHold;

        // TODO: 2 things: 
        // TODO:   (1) i need the last filePath of this recorder not other, the issue I can't be recording if other already is
        // TODO:   (2) I want async await, not a coroutine, because I need to do the full pipeline
        // TODO: test
        string newUserIntent = await speech2TextAI.TranscribeAsync(micRecorder.GetLastFilePath());
        SetUserIntent(newUserIntent);

        // TODO: Unity Events!
        string result = await _reasoningAI.RunReasoningAsync(chatData, newUserIntent);

        // TODO : make request - merge documents into a single thing (with the userIntent + chatOverrides)


        // TODO: receive request
        state = AIAssistantManager.Instance.SetStateAfterOnHold(this);
    }

    // TODO: Talk on hold if already talking (important)
    public async void Talk(ChatInternalMemory response)
    {
        if (string.IsNullOrEmpty(response.assistantResponse)) return;

        try
        {
            await textToSpeechAI.GenerateAndPlaySpeech(response.assistantResponse);
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[{nameof(AIAssistant)}] Error generating speech: {ex.Message}");
        }
    }

    #region SpeechToText

    [Button]
    private void FirstStartRecording()
    {
        // NOTE: Fake Mic Recording
        if (AIClientFakes.TryHandleFakeRecorder(aiClientToggle, SetFakeMicRecording))
        {
            return;
        }

        micRecorder.StartRecording();
    }

    [Button]
    private void SecondStopRecordingAndSave()
    {
        // NOTE: Fake Mic Recording
        if (AIClientFakes.TryHandleFakeRecorder(aiClientToggle, SetFakeMicRecording))
        {
            return;
        }

        micRecorder.StopAndSave();
    }

    private void SetFakeMicRecording(AudioClip newClip)
    {
#if UNITY_EDITOR
        micRecorder.OverrideAudioClipAndPath(newClip);
#endif
        micRecorder.onRecordedAudio.Invoke(newClip);
    }

    private void SetUserIntent(string text)
    {
        userIntent = text;
    }

    #endregion



    #region State-Changes
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
        AIAssistantManager.Instance.RemoveChat(this);
        MicRecorderManager.Instance.UnregisterRecorder(micRecorder);
        onClosing.Invoke(this);

        // TODO: UI/Animation - destroy the object clearly

        Destroy(gameObject);
    }

    #endregion


    // NOTE: to simplify the process
    private void OnDestroy()
    {
        onApiResponse.RemoveAllListeners();
        onApiRequest.RemoveAllListeners();
        onSelected.RemoveAllListeners();
        onUnselected.RemoveAllListeners();
        onClosing.RemoveAllListeners();
    }
}
