using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Runware;
using UnityEngine;
using UnityEngine.Events;
using Sirenix.OdinInspector;
using UnityEditor;
using TMPro;
using System.Linq;
using UnityEditor.Overlays;
using System.Text.Json;
using Assets.Disconnected.Scripts.AI.AIAssistant.API;
using System.Linq.Expressions;
using System.IO;

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

    [SerializeField] private SF3DAPIClient imageTo3DAI;

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
    [SerializeField] private State state;

    // TODO: these unity events we will see what makes sense
    public UnityEvent<AIAssistant> onApiResponse;
    public UnityEvent<AIAssistant> onApiRequest;
    public UnityEvent<AIAssistant> onSelected;
    public UnityEvent<AIAssistant> onUnselected;
    public UnityEvent<AIAssistant> onClosing;

    public State CurrentState { get => state; set => state = value; }

    /// <summary>
    /// To populate everything I can right away
    /// </summary>
    private void OnValidate()
    {
        micRecorder = GetComponent<MicRecorder>();
        speech2TextAI = GetComponent<WhisperTranscriber>();
        textToSpeechAI = GetComponent<GroqTTS>();
        textToImageAI = GetComponent<RunwareTTI>();
        textToSpeechAI.audioSource = GetComponent<AudioSource>();
    }

    void Start()
    {
        if (APIKeyLoader.Instance == null)
        {
            Debug.LogError("APIKeyLoader instance not found!");
            return;
        }
        imageTo3DAI = APIKeyLoader.Instance.SF3DApi;
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

        // TODO: add more tools
        _reasoningAI.CreateSystemMessage(reasoningStaticSystemMessageDocuments.Select(doc => doc.text).ToList());
        _reasoningAI.AddTool(_reasoningAI.BuildTextToImagePrompt(ProcessTextToImagePrompt));
        _reasoningAI.AddTool(_reasoningAI.ExplainRuleSystem(ProcessExplainRuleSystem));
    }


    #region Handle User Recordings
    public void StartRecordingUser()
    {
        // NOTE: only record when it is over
        if (state == State.OnHold) return;

        state = State.OnHold;
        micRecorder.StartRecording();
    }

    public void StopRecording()
    {
        // this triggers the onRecordedAudio once it is saved and we can proceed with everything
        // from UserRecordedIntent
        micRecorder.StopAndSave();
    }

    /// <summary>
    /// Method that will start the process to make request to assistant 
    /// </summary>
    /// <param name="arg0">ignored</param>
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
    #endregion

    #region AI-Assistant-Main-Methods

    /// <summary>
    /// After recording user audio, we transcribe it in our speech to text AI, then run the function calling LLm AI
    /// for it to generate image/3d model or answer user questions.
    /// </summary>
    /// <returns></returns>
    public async Task MakeRequestToAssistant()
    {
        string newUserIntent = await speech2TextAI.TranscribeAsync(micRecorder.GetLastFilePath());
        SetUserIntent(newUserIntent);

        // NOTE: this method does all the Heavy Lifting
        string result = await _reasoningAI.RunReasoningAsync(chatData, newUserIntent);

        // reset state once it has been handled
        state = AIAssistantManager.Instance.SetStateAfterOnHold(this);
    }

    /// <summary>
    /// Create text to image prompt - Function calling execute logic. 
    /// In it we generate image, then run image to 3D, while returning a AI speech.
    /// </summary>
    /// <param name="arg">JSON that we get from {nameof(BuildTextToImagePrompt)}</param>
    /// <returns></returns>
    private async Task<string> ProcessTextToImagePrompt(string arg)
    {
        try
        {
            Debug.Log($"[{nameof(AIAssistant)} - {gameObject.name}] - {nameof(ProcessTextToImagePrompt)} input:\n{arg}");

            // get json parse it - and transform it into our response data model
            var jsonArgs = JsonDocument.Parse(arg);
            var text2ImageResponse = JsonSerializer.Deserialize<AIAssistantText2ImageResponseModel>(jsonArgs.RootElement);

            // collect data
            string assistantResponse = text2ImageResponse.assistantResponse;
            // transform response into new prompt compiler that will be available for the user later on at the end
            var newPromptCompiler = text2ImageResponse.ToPromptCompiler(chatData.promptCompiler);

            Debug.Log($"[{nameof(AIAssistant)} - {gameObject.name}] - {nameof(ProcessTextToImagePrompt)} PromptCompiler:\n{JsonSerializer.Serialize(newPromptCompiler)}");

            // convert the promptCompiler into the API request model for the text to image prompt
            var request = newPromptCompiler.ToTextToImageRequestModel();

            // this return string is useless but it is what I am returning because GroqTTS requires me to return a string at the end 
            var returnString = JsonSerializer.Serialize(new { assistantResponse = $"{assistantResponse}", result = $"{request.ToString()}" });
            Debug.Log($"[{nameof(AIAssistant)} - {gameObject.name}] - {nameof(ProcessTextToImagePrompt)} TextToImageRequestModel:\n{returnString}");

            // Generate text to image results based on prompt
            var imageResults = await textToImageAI.GenerateTextToImage(
                request: request,
                onStartAction: null, // TODO: add voice input
                onCompleteAction: SetLastGeneratedImage,
                onErrorAction: null // TODO: add voice input as well
            );

            // Once image has been generated - I can run all async operations concurrently: AI speech and text-to-3d results
            var tasks = new List<Task>();

            // this is just a shitty way to get the list of GameObjects created
            var modelTasks = new List<Task<GameObject>>();

            // Add text-to-speech task - not storing since it is the assistant talking
            tasks.Add(textToSpeechAI.GenerateAndPlaySpeech(assistantResponse, FileEnumPath.None));

            // Add all 3D model generation tasks and collect their results
            foreach (var image in imageResults)
            {
                var modelTask = imageTo3DAI.Generate3DModelAsync(
                    imagePath: image.imagePath,
                    filename: $"{Path.GetFileNameWithoutExtension(image.imagePath)}-model",
                    fileEnumPath: FileEnumPath.Temporary, // ???: or Persistent - since we are storing in cloud, I think Persistent storage in headset does not make much sense 
                    onModelLoaded: null, // TODO: stuff when model is loaded - place method here
                    parent: null
                );
                modelTasks.Add(modelTask);
                tasks.Add(modelTask);
            }

            // Wait for all tasks to complete
            await Task.WhenAll(tasks);

            // Collect all the generated GameObjects
            var generatedModels = new List<GameObject>();
            foreach (var modelTask in modelTasks)
            {
                var model = await modelTask; // This will be immediate since we already awaited all tasks
                if (model != null)
                {
                    generatedModels.Add(model);
                }
            }

            Debug.Log($"[{nameof(AIAssistant)} - {gameObject.name}] - {nameof(ProcessTextToImagePrompt)}: Generated {generatedModels.Count} 3D models");

            // TODO: part to invoke UI / UX changes in the main thread
            UnityMainThreadDispatcher.Instance().Enqueue(async () =>
            {
                chatData.promptCompiler = newPromptCompiler;
                chatData.assistantResponse = assistantResponse;

                // Example: Save prefabs in editor and then do more stuff with the gameObjects
#if UNITY_EDITOR
                foreach (var model in generatedModels)
                {
                    imageTo3DAI.SavePrefab(model, model.name);
                }
#endif
                Debug.Log($"[{nameof(AIAssistant)} - {gameObject.name}] - {nameof(ProcessTextToImagePrompt)} Completed!!!");

                // TODO: UI Display stuff - event invoke
            });
            return returnString;
        }
        catch (Exception e)
        {
            Debug.LogError($"[{nameof(AIAssistant)} - {gameObject.name}] - {nameof(ProcessTextToImagePrompt)} ERROR:\n{e}");
            state = AIAssistantManager.Instance.SetStateAfterOnHold(this);

            return null;
        }
    }

    /// <summary>
    /// Explain Rule System, requests clarifications - Function calling execute logic. 
    /// In it we answer questions on how the pipeline works and how to make better prompts, and clarify when the user intent is unclear.
    /// Returns: an assistant response that is fed to textToSpeech AI
    /// </summary>
    /// <param name="arg">JSON that we get from {nameof(ExplainRuleSystem)}</param>
    /// <returns></returns>
    private async Task<string> ProcessExplainRuleSystem(string arg)
    {
        try
        {
            // get json parse it - and transform it into our response data model
            Debug.Log($"[{nameof(AIAssistant)} - {gameObject.name}] - {nameof(ProcessExplainRuleSystem)} input:\n{arg}");
            var jsonArgs = JsonDocument.Parse(arg);
            var jsonModel = JsonSerializer.Deserialize<AssistantResponseOnlyModel>(jsonArgs.RootElement);

            // just text to speech the assistant response
            string assistantResponse = jsonModel.assistantResponse;

            var returnString = JsonSerializer.Serialize(jsonModel);
            await textToSpeechAI.GenerateAndPlaySpeech(assistantResponse, FileEnumPath.None);

            Debug.Log($"[{nameof(AIAssistant)} - {gameObject.name}] - {nameof(ProcessExplainRuleSystem)} Completed!!!");

            // TODO: if there is anything that needs to be done in the main thread
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
            state = AIAssistantManager.Instance.SetStateAfterOnHold(this);
            return null;
        }
    }

    #endregion

    #region Debug-Methods
    private void SetLastGeneratedImage(List<GenerateTextToImageOutputModel> newImages)
    {
        lastGeneratedImage = newImages.FirstOrDefault().texture;
    }

    private void SetUserIntent(string text)
    {
        userIntent = text;
    }
    #endregion



    #region Buttons

    [Button]
    private void FirstStartRecording()
    {
        // NOTE: Fake Mic Recording
        if (AIClientFakes.TryHandleFakeRecorder(aiClientToggle, SetFakeMicRecording))
        {
            return;
        }

        StartRecordingUser();
    }

    [Button]
    private void SecondStopRecordingAndSave()
    {
        // NOTE: Fake Mic Recording
        if (AIClientFakes.TryHandleFakeRecorder(aiClientToggle, SetFakeMicRecording))
        {
            return;
        }

        StopRecording();
    }

    private void SetFakeMicRecording(AudioClip newClip)
    {
#if UNITY_EDITOR
        micRecorder.OverrideAudioClipAndPath(newClip);
#endif
        micRecorder.onRecordedAudio.Invoke(newClip);
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
