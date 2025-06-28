using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Assets.Disconnected.Scripts.AI.AIAssistant.API;
using GroqApiLibrary;
using UnityEngine;

/// <summary>
/// Project-specific controller for AI assistant reasoning, extending the core service.
/// Handles system message assignment, document merging, and tool configuration.
/// </summary>
public class AssistantReasoningController : GroqReasoningService
{
    private readonly string _delimiter_ = "\n\n------------------------------------------------------------------------------------\n\n";
    
    /// <summary>
    /// The image-to-text model name.
    /// </summary>
    public string Image2TextModelName { get; set; }
    public string BuildTextToImagePromptDescription { get; set; }
    public string UserRequestIntro { get; set; }
    public string ExplainRuleSystemDescription { get; set; }

    /// <param name="image2TextModelName">The image-to-text model name.</param>

    public AssistantReasoningController(
        string model,
        string currentSystemMessage,
        string image2TextModelName,
        string userRequestIntro,
        string buildTextToImagePromptDescription,
        string explainRuleSystemDescription,
        List<ExtendedTool> tools = null
    ) : base(model, currentSystemMessage, tools)
    {
        
        Image2TextModelName = image2TextModelName;
        BuildTextToImagePromptDescription = buildTextToImagePromptDescription;
        UserRequestIntro = userRequestIntro;
        ExplainRuleSystemDescription = explainRuleSystemDescription;
        // TODO : add tools as I go
        //Tools = new List<ExtendedTool> { BuildTextToImagePrompt() };//, OverrideSessionPreferences(), OverrideChatPreferences() };
    }

    /// <summary>
    /// Creates or updates the system message, merging documents as needed.
    /// Override this method to customize system message logic.
    /// </summary>
    /// <param name="systemMessageDocuments">A list of system message documents.</param>
    public virtual void CreateSystemMessage(List<string> systemMessageDocuments)
    {
        // Example: merge documents into a single system message
        string delimiter = "\n\n------------------------------------------------------------------------------------\n\n";
        string newSystemMessage = "You are a helpful assistant that can execute Unity commands. Please read the systemMessage carefully and each tool description first before executing commands." + delimiter;
        if (systemMessageDocuments != null)
        {
            foreach (var doc in systemMessageDocuments)
            {
                newSystemMessage += doc + delimiter;
            }
        }
        CurrentSystemMessage = newSystemMessage;
    }

    public async Task<string> RunReasoningAsync(
        Image3dConfig chatInternalMemory,
        string userIntent)
    {
        if (APIKeyLoader.Instance == null)
        {
            Debug.LogError("APIKeyLoader instance not found!");
            return null;
        }

        var request = chatInternalMemory.CreateTextToImageRequestModel(userIntent, UserRequestIntro);

        return await base.RunReasoningAsync(
            request.ToString(),
            APIKeyLoader.Instance.GroqApi.RunConversationWithToolsAsync
        );
    }

    /// <summary>
    /// Builds the Text-to-Image tool, requiring an external ExecuteAsync delegate for execution logic.
    /// </summary>
    /// <param name="executeAsync">The async execution delegate.</param>
    /// <returns>The configured ExtendedTool.</returns>
    public ExtendedTool BuildTextToImagePrompt(Func<string, Task<string>> executeAsync)
    {
        return new ExtendedTool
        {
            Type = "function",
            Function = new ExtendedFunction
            {
                Name = "build_text_2_image_prompt",
                Description = BuildTextToImagePromptDescription,
                Parameters = ParametersGenerator.GenerateParameters(typeof(APIText2ImageResponseModel)),
                /*
                Parameters = new Parameters
                {
                    Type = "object",
                    Properties = new Dictionary<string, Property>()
                    {
                        ["assistantResponse"] = new Property { Type = "string", Description = $"You as my lovely assistant clearly narrates what was updated, confirmed or answer a user's question." },
                        ["model"] = new Property { Type = "string", Description = $"{Image2TextModelName} Text to image model being used by the user. Important because some model type (e.g. FLUX) do not allow `negativePrompt` and if that is the user Intent, it needs to inserted in our `positivePrompt` accordingly" },
                        ["positivePrompt"] = new Property { Type = "string", Description = $"Text to image prompt output. Restructure prompt to include negative inputs if model does not allow negative prompts. The prompt should be written in a language that the {Image2TextModelName} will deliver the best outcome" },
                        ["negativePrompt"] = new Property { Type = "string", Description = "Things to exclude from the image output. Result null if model does not allow negative prompts." },
                        ["imageShape"] = new Property { Type = "string", Description = "Output image shape. 3 possible values: `Square`, `Horizontal`, `Vertical`. Depends on user intent. Default value: `Square`" },
                        ["isNSFW"] = new Property { Type = "boolean", Description = "If that is the user intent - set to true, otherwise the default is false" },
                        ["customBackground"] = new Property { Type = "string", Description = "Set to null or empty if the goal is to fallback to transparent/white; Non-empty describes what properties the scene/environment should have as background." },
                        ["imageArtStyle"] = new Property { Type = "string", Description = "If requested by user to use a specific image art style, it should be included both here and in the `positivePrompt`. Default: ''" },
                        ["target"] = new Property { Type = "string", Description = "User's ultimate goal when getting images. 2 possible values: `ImageTo3D`, `WallArt`. Default value: `ImageTo3D`" },
                        ["editDetail"] = new Property { Type = "string", Description = "Soft enum: `MoreDetailed`, `LessDetailed`, `AsIs`, `Default`. Based on user intent for detail level. Default: `Default`" },
                        ["t2iCreativity"] = new Property { Type = "string", Description = "Soft enum: `MoreCreative`, `LessCreative`, `AsIs`, `Default`. Based on user intent for creativity level. Default: `Default`" },
                    },
                    Required = new string[] {
                        "assistantResponse", "model",
                        "positivePrompt", "negativePrompt",
                        "imageShape", "isNSFW",
                        "customBackground", "target"
                    }
                },
                */
                ExecuteAsync = executeAsync
            }
        };
    }

    public ExtendedTool ExplainRuleSystem(Func<string, Task<string>> executeAsync)
    {
        return new ExtendedTool
        {
            Type = "function",
            Function = new ExtendedFunction
            {
                Name = "explain_rule_system",
                Description = ExplainRuleSystemDescription,
                Parameters = ParametersGenerator.GenerateParameters(typeof(AssistantResponseOnlyModel)),
                ExecuteAsync = executeAsync
            }
        };
    }
}