using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Runware;
using UnityEngine;
using System.Linq;
using System.Text.Json;
using Assets.Disconnected.Scripts.AI.AIAssistant.API;
using System.IO;
using Disconnected.Scripts.Utils;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Overlays;
#endif

[RequireComponent(typeof(RunwareTTI))]
[RequireComponent(typeof(InteractionComponentManager))]
public class AISpeechToImage3dAssistant : BaseAIAssistant
{

    [Header("Image AIs")]

    [SerializeField] private RunwareTTI textToImageAI;

    [SerializeField] private SF3DAPIClient imageTo3DAI;

    [SerializeField] private InteractionComponentManager xrInteractionManager;

    [Space]
    [Header("AI Reasoning Data")]
    [SerializeField] private TextAsset reasoningUserPromptIntro;
    [SerializeField] private List<TextAsset> reasoningStaticSystemMessageDocuments;
    [SerializeField] private TextAsset buildingTextToImagePromptSystemDocument;
    [SerializeField] private TextAsset explainRuleSystemDocument;
    [SerializeField] private ChatInternalMemory chatData;

    [Space]
    [Header("Debug - Text to Image")]
    [SerializeField] private Texture2D lastGeneratedImage;

    /// <summary>
    /// To populate everything I can right away
    /// </summary>
    protected override void OnValidate()
    {
        base.OnValidate();
        textToImageAI = GetComponent<RunwareTTI>();
        xrInteractionManager = GetComponent<InteractionComponentManager>();
    }

    protected override void Start()
    {
        base.Start();
        if (APIKeyLoader.Instance == null)
        {
            Debug.LogError("APIKeyLoader instance not found!");
            return;
        }

        // TODO: Improve implementation of SF3DApi
        imageTo3DAI = APIKeyLoader.Instance.SF3DApi;
    }

    public override void Initialize(AIGameSettings gameSettings)
    {
        base.Initialize(gameSettings);
        textToImageAI.model = gameSettings.textToImageAIModelName;

        /*
        _reasoningAIService = new AIAssistantReasoningController(
            model: gameSettingFs.aiReasoningModel,
            currentSystemMessage: null,
            image2TextModelName: gameSettings.textToImageAIModelName.ToString(),
            buildTextToImagePromptDescription: buildingTextToImagePromptSystemDocument.text,
            userRequestIntro: reasoningUserPromptIntro.text,
            explainRuleSystemDescription: explainRuleSystemDocument.text,
            tools: null
        );
        */

        // Initialize reasoning service for text to image assistance
        _reasoningAIService.Image2TextModelName = gameSettings.textToImageAIModelName.ToString();
        _reasoningAIService.BuildTextToImagePromptDescription = buildingTextToImagePromptSystemDocument.text;
        _reasoningAIService.UserRequestIntro = reasoningUserPromptIntro.text;
        _reasoningAIService.ExplainRuleSystemDescription = explainRuleSystemDocument.text;

        // TODO: add more tools
        _reasoningAIService.CreateSystemMessage(reasoningStaticSystemMessageDocuments.Select(doc => doc.text).ToList());
        _reasoningAIService.AddTool(_reasoningAIService.BuildTextToImagePrompt(ProcessTextToImagePrompt));
        _reasoningAIService.AddTool(_reasoningAIService.ExplainRuleSystem(ProcessExplainRuleSystem));
    }

    #region AI-Assistant-Main-Methods

    /// <summary>
    /// Override the async processing to add the interjection before calling base processing
    /// </summary>
    protected override async Task ProcessUserRecordedIntentAsync()
    {
        try
        {
            // just an audio snippet of assistant thinking to provide a sense of processing not in vacuum
            var interjectionTask = AssistantAnswer(AssistantSpeechSnippets.EffortBasedInterjections.GetRandomEntry());

            // Call the base processing (transcription and reasoning)
            await base.ProcessUserRecordedIntentAsync();

            // Wait for the interjection to complete
            if (interjectionTask != null)
            {
                await interjectionTask;
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"[{nameof(AISpeechToImage3dAssistant)}] Error in ProcessUserRecordedIntentAsync: {e}");
            throw; // Re-throw to let base class handle state reset
        }
    }

    /// <summary>
    /// After recording user audio, we transcribe it in our speech to text AI, then run the function calling LLm AI
    /// for it to generate image/3d model or answer user questions.
    /// </summary>
    /// <returns></returns>
    protected override async Task MakeRequestToAssistant()
    {
        try
        {
            // NOTE: this method does all the Heavy Lifting
            toolOutput = await _reasoningAIService.RunReasoningAsync(chatData, userIntent);
        }
        catch (Exception e)
        {
            Debug.LogError($"[{nameof(AISpeechToImage3dAssistant)}] Error in MakeRequestToAssistant: {e}");
            throw; // Re-throw to let base class handle the error
        }
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
            Debug.Log($"[{nameof(AISpeechToImage3dAssistant)} - {gameObject.name}] - {nameof(ProcessTextToImagePrompt)} input:\n{arg}");

            // get json parse it - and transform it into our response data model
            var jsonArgs = JsonDocument.Parse(arg);
            var text2ImageResponse = JsonSerializer.Deserialize<AIAssistantText2ImageResponseModel>(jsonArgs.RootElement);

            // collect data
            string assistantResponse = text2ImageResponse.assistantResponse;
            // transform response into new prompt compiler that will be available for the user later on at the end
            var newPromptCompiler = text2ImageResponse.ToPromptCompiler(chatData.promptCompiler);

            Debug.Log($"[{nameof(AISpeechToImage3dAssistant)} - {gameObject.name}] - {nameof(ProcessTextToImagePrompt)} PromptCompiler:\n{JsonSerializer.Serialize(newPromptCompiler)}");

            // convert the promptCompiler into the API request model for the text to image prompt
            var request = newPromptCompiler.ToTextToImageRequestModel();

            // this return string is useless but it is what I am returning because GroqTTS requires me to return a string at the end 
            var returnString = JsonSerializer.Serialize(new { assistantResponse = $"{assistantResponse}", result = $"{request.ToString()}" });
            Debug.Log($"[{nameof(AISpeechToImage3dAssistant)} - {gameObject.name}] - {nameof(ProcessTextToImagePrompt)} TextToImageRequestModel:\n{returnString}");

            // Generate text to image results based on prompt
            var imageResultsTask = textToImageAI.GenerateTextToImage(
                request: request,
                onStartAction: null,
                onCompleteAction: SetLastGeneratedImage,
                onErrorAction: null // TODO: add voice input as well
            );

            // this text to image some time - so it best to run this assistant model asking for a bit more time
            await Task.WhenAll(
                //AssistantAnswer(AssistantSpeechSnippets.TimeBasedInterjections.GetRandomEntry()),
                imageResultsTask
            );
            var imageResults = await imageResultsTask;

            // TODO: Add an if based on function - to run 3d model generation or not 
            // Once image has been generated - I can run all generate 3d model
            var modelTasks = new List<Task<GameObject>>();

            // Add all 3D model generation tasks and collect their results
            foreach (var image in imageResults)
            {
                var modelTask = imageTo3DAI.Generate3DModelAsync(
                    imagePath: image.imagePath,
                    filename: $"{Path.GetFileNameWithoutExtension(image.imagePath)}-model",
                    fileEnumPath: FileEnumPath.Temporary, // ???: or Persistent - since we are storing in cloud, I think Persistent storage in headset does not make much sense 
                    onModelLoaded: AddInteractionComponents, // TODO: stuff when model is loaded - place method here
                    parent: null
                );
                modelTasks.Add(modelTask);
            }


            // Wait for all model generation tasks to complete
            var generatedModels = await Task.WhenAll(modelTasks);

            Debug.Log($"[{nameof(AISpeechToImage3dAssistant)} - {gameObject.name}] - {nameof(ProcessTextToImagePrompt)}: Generated {generatedModels.Length} 3D models");

            Debug.Log($"[{nameof(AISpeechToImage3dAssistant)} - {gameObject.name}] - {nameof(ProcessTextToImagePrompt)} Completed!!!");

            // TODO: part to invoke UI / UX changes in the main thread
            UnityMainThreadDispatcher.Instance().Enqueue(async () =>
            {
                chatData.promptCompiler = newPromptCompiler;
                chatData.assistantResponse = assistantResponse;

                // TODO: UI Display stuff - event invoke
            });
            errorOutput = null;

            // Now, after all model generation and main thread work, run AssistantAnswer
            // TODO: prompt better to have it running
            //_ = AssistantAnswer(assistantResponse);
            _ = AssistantAnswer(AssistantSpeechSnippets.CreativeOutputReadyInterjections.GetRandomEntry()); // fire-and-forget, or await if you want to wait

            return returnString;
        }
        catch (Exception e)
        {
            string errorMessage = $"[{nameof(AISpeechToImage3dAssistant)} - {gameObject.name}] - {nameof(ProcessTextToImagePrompt)} ERROR:\n{e}";
            Debug.LogError(errorMessage);
            errorOutput = errorMessage;
            AssistantOnErrorOccurred(errorMessage);

            state = AIAssistantManager.Instance.SetStateAfterOnHold(this);

            return null;
        }
    }

    private void AddInteractionComponents(GameObject @object)
    {
        xrInteractionManager.AddInteractionComponents(@object);
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
            Debug.Log($"[{nameof(AISpeechToImage3dAssistant)} - {gameObject.name}] - {nameof(ProcessExplainRuleSystem)} input:\n{arg}");
            var jsonArgs = JsonDocument.Parse(arg);
            var jsonModel = JsonSerializer.Deserialize<AssistantResponseOnlyModel>(jsonArgs.RootElement);

            // just text to speech the assistant response
            string assistantResponse = jsonModel.assistantResponse;

            var returnString = JsonSerializer.Serialize(jsonModel);

            await AssistantAnswer(assistantResponse);

            Debug.Log($"[{nameof(AISpeechToImage3dAssistant)} - {gameObject.name}] - {nameof(ProcessExplainRuleSystem)} Completed!!!");

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
            Debug.LogError($"[{nameof(AISpeechToImage3dAssistant)} - {gameObject.name}] - {nameof(ProcessExplainRuleSystem)} ERROR:\n{e}");
            AssistantOnErrorOccurred(e.ToString());
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

    #endregion



    #region Buttons
    // NOTE: buttons do not propagate in extended classes
    /*
    [Button]
    protected override void FirstStartRecording()
    {
        base.FirstStartRecording();
    }

    [Button]
    protected override void SecondStopRecordingAndProcessIntent()
    {
        base.SecondStopRecordingAndProcessIntent();
    }
    */

    #endregion


}
