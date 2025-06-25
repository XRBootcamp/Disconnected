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


[RequireComponent(typeof(RunwareTTI))]
public class AISpeechToImage3dAssistant : BaseAIAssistant
{

    [Header("Image AIs")]

    [SerializeField] private RunwareTTI textToImageAI;

    [SerializeField] private SF3DAPIClient imageTo3DAI;

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
        textToImageAI.model = gameSettings.textToImageAIModelName;

        base.Initialize(gameSettings);

        /*
        _reasoningAIService = new AIAssistantReasoningController(
            model: gameSettings.aiReasoningModel,
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
    /// After recording user audio, we transcribe it in our speech to text AI, then run the function calling LLm AI
    /// for it to generate image/3d model or answer user questions.
    /// </summary>
    /// <returns></returns>
    protected override async Task MakeRequestToAssistant()
    {
        
        // NOTE: this method does all the Heavy Lifting
        toolOutput = await _reasoningAIService.RunReasoningAsync(chatData, userIntent);

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
            tasks.Add(assistantTextToSpeechAI.GenerateAndPlaySpeech(assistantResponse, FileEnumPath.None));

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

            Debug.Log($"[{nameof(AISpeechToImage3dAssistant)} - {gameObject.name}] - {nameof(ProcessTextToImagePrompt)}: Generated {generatedModels.Count} 3D models");

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
                Debug.Log($"[{nameof(AISpeechToImage3dAssistant)} - {gameObject.name}] - {nameof(ProcessTextToImagePrompt)} Completed!!!");

                // TODO: UI Display stuff - event invoke
            });
            errorOutput = null;
            return returnString;
        }
        catch (Exception e)
        {
            string errorMessage = $"[{nameof(AISpeechToImage3dAssistant)} - {gameObject.name}] - {nameof(ProcessTextToImagePrompt)} ERROR:\n{e}";
            Debug.LogError(errorMessage);
            errorOutput = errorMessage;

            //state = AIAssistantManager.Instance.SetStateAfterOnHold(this);

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
            Debug.Log($"[{nameof(AISpeechToImage3dAssistant)} - {gameObject.name}] - {nameof(ProcessExplainRuleSystem)} input:\n{arg}");
            var jsonArgs = JsonDocument.Parse(arg);
            var jsonModel = JsonSerializer.Deserialize<AssistantResponseOnlyModel>(jsonArgs.RootElement);

            // just text to speech the assistant response
            string assistantResponse = jsonModel.assistantResponse;

            var returnString = JsonSerializer.Serialize(jsonModel);
            await assistantTextToSpeechAI.GenerateAndPlaySpeech(assistantResponse, FileEnumPath.None);

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
            //state = AIAssistantManager.Instance.SetStateAfterOnHold(this);
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
    [Button]
    protected override void FirstStartRecording()
    {
        base.FirstStartRecording();
    }

    [Button]
    protected override void SecondStopRecordingAndSave()
    {
        base.SecondStopRecordingAndSave();
    }

    #endregion


}
