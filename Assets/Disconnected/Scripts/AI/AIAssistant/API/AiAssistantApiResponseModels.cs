using System;
using System.Text.Json.Serialization;
using UnityEngine;
using System.Collections.Generic;


[Serializable]
public class AIAssistantText2ImageResponseModel
{
    /// <summary>
    /// You as my lovely assistant clearly narrates what was updated, confirmed or answered in a user's intent.
    /// </summary>
    [JsonPropertyName("assistantResponse")]
    public string assistantResponse { get; set; }

    /// <summary>
    /// Text to image model being used by the user.
    /// Important for the reasoning model to create the best prompt possible for this model 
    /// </summary>
    [JsonPropertyName("model")]
    public string model { get; set; }

    /// <summary>
    /// Text to image positive prompt output.
    /// </summary>
    [JsonPropertyName("positivePrompt")]
    public string positivePrompt { get; set; }

    /// <summary>
    /// Things to exclude from the image output.
    /// </summary>
    [JsonPropertyName("negativePrompt")]
    public string negativePrompt { get; set; }

    /// <summary>
    /// Output image shape. Possible values: `Square`, `Horizontal`, `Vertical`, null. Default is null.
    /// </summary>
    [JsonPropertyName("imageShape")]
    public string imageShape { get; set; }

    /// <summary>
    /// Set to true if that is the user intent, otherwise default is false.
    /// </summary>
    [JsonPropertyName("isNSFW")]
    public bool isNSFW { get; set; }

    /// <summary>
    /// Set to null or empty if the goal is to fallback to transparent/white; Non-empty describes what properties the scene/environment should have as background.
    /// </summary>
    [JsonPropertyName("customBackground")]
    public string customBackground { get; set; }

    /// <summary>
    /// If requested by user to use a specific image art style, it should be included both here and in the `positivePrompt`. Default: null.
    /// </summary>
    [JsonPropertyName("imageArtStyle")]
    public string imageArtStyle { get; set; }

    /// <summary>
    /// User's ultimate goal for the image. Possible values: `ImageTo3D`, `WallArt`, null. Default: null.
    /// </summary>
    [JsonPropertyName("target")]
    public string target { get; set; }

    /// <summary>
    /// Soft enum: MoreDetailed | LessDetailed | AsIs | Default
    /// Based on user intent for detail level. Default: Default
    /// </summary>
    [JsonPropertyName("editDetail")]
    public string editDetail { get; set; } = "Default";

    /// <summary>
    /// Soft enum: MoreCreative | LessCreative | AsIs | Default
    /// Based on user intent for creativity level. Default: Default
    /// </summary>
    [JsonPropertyName("t2iCreativity")]
    public string t2iCreativity { get; set; } = "Default";

    // Static property descriptions for ParametersGenerator
    [NonSerialized]
    public static readonly Dictionary<string, string> PropertyDescriptions = new Dictionary<string, string>
    {
        { "assistantResponse", "Human-readable summary that communicates what the assistant did in response to the user's input. This message should reflect the goal of the user — which is to generate an image — and summarize what kind of image will be created based on the current prompt. This process is immediate: the LLM creates the prompt, and the system feeds it into a text-to-image AI model. Always speak as if the image is about to be generated. Example: 'Here is your image: a silver robotic tiger with glowing blue eyes, created based on your intent and preferences.'" },
        { "model", "The name of the text-to-image model that should be used for generation. Determines how the assistant shapes the prompt and handles features like negative prompts." },
        { "positivePrompt", "The complete positive prompt string sent to the text-to-image model. This must fully describe the image, including art style, background, goal, NSFW intent, creativity, and detail—regardless of whether these are also present in their respective fields." },
        { "negativePrompt", "Comma-separated keywords or short phrases representing elements that must be excluded from the image. Used for filtering unwanted features in the generation." },
        { "imageShape", "Target image format. Expected values: 'Square', 'Horizontal', 'Vertical', or null. It does not influence prompt wording but helps adjust canvas orientation." },
        { "isNSFW", "Boolean flag. True only if the user explicitly requests NSFW content (e.g. gore, nudity, horror). Otherwise, always false." },
        { "customBackground", "String description of a requested background scene. If null or empty, defaults to transparent or white. Must also appear in `positivePrompt`." },
        { "imageArtStyle", "If the user requests a specific art style, it is recorded here (e.g. watercolor, pixel art). It must also be represented in the `positivePrompt`. Default is null." },
        { "target", "Indicates the user's ultimate purpose for the image. Common values: 'ImageTo3D', 'WallArt'. Should influence the phrasing in `positivePrompt` to better match the goal." },
        { "editDetail", "Soft enum guiding detail control. Options: 'MoreDetailed', 'LessDetailed', 'AsIs', 'Default'. Impacts prompt generation depth only if user asks." },
        { "t2iCreativity", "Soft enum guiding how rigidly to follow the prompt. Options: 'MoreCreative', 'LessCreative', 'AsIs', 'Default'. Adjusts prompt looseness if requested." }
    };
    /*
    public static readonly Dictionary<string, string> PropertyDescriptions = new Dictionary<string, string>
    {
        { "assistantResponse", "You as my lovely assistant clearly narrates what was updated, confirmed or answered in a user's intent." },
        { "model", "Text to image model being used by the user. Important for the reasoning model to create the best prompt possible for this model" },
        { "positivePrompt", "Text to image positive prompt output. Important note: this field must include the entire positive description of what is intended to be represented in our image prompt. It includes information from all other fields: art style, is NSFW, background, the goal of this generated image, how much detail and creativity." },
        { "negativePrompt", "Things to exclude from the image output. Write single words/expressions, separated by commas. (e.g. Flying cars, bananas, shooting rays)" },
        { "imageShape", "Output image shape. Possible values: `Square`, `Horizontal`, `Vertical`, null. Default is null." },
        { "isNSFW", "Set to true if that is the user intent, otherwise default is false." },
        { "customBackground", "Set to null or empty if the goal is to fallback to transparent/white; Non-empty describes what properties the scene/environment should have as background." },
        { "imageArtStyle", "If requested by user to use a specific image art style, it should be included both here and in the `positivePrompt`. Default: null." },
        { "target", "User's ultimate goal for the image. Possible values: `ImageTo3D`, `WallArt`, null. Default: null." },
        { "editDetail", "Soft enum: MoreDetailed | LessDetailed | AsIs | Default. Based on user intent for detail level. Default: Default" },
        { "t2iCreativity", "Soft enum: MoreCreative | LessCreative | AsIs | Default. Based on user intent for creativity level. Default: Default" }
    };
    */
    [NonSerialized]
    public static readonly string[] RequiredFields = new[]
    {
        "assistantResponse", "model", "positivePrompt", "negativePrompt", "imageShape", "isNSFW", "customBackground", "target"
    };
}

[Serializable]
public class ImageSessionPreferencesResponseModel
{
    /// <summary>
    /// You as my lovely assistant clearly narrates what was updated, confirmed or answered in a user's intent.
    /// </summary>
    [JsonPropertyName("assistantResponse")]
    public string assistantResponse { get; set; }
    /// <summary>
    /// Can be overridden per chat
    /// </summary>
    public string style { get; set; } = null;

    /// <summary>
    /// Output image shape. Possible values: `Square`, `Horizontal`, `Vertical`, null. Default is null
    /// </summary>
    [JsonPropertyName("imageShape")]
    public string imageShape { get; set; } = null;

    /// <summary>
    /// Global default unless explicitly toggled | null
    /// </summary>
    [JsonPropertyName("allowNsfw")]
    public bool? allowNsfw { get; set; } = null;

    /// <summary>
    /// Session goal: ImageTo3D or WallArt. Default null
    /// </summary>
    public string target { get; set; } = null;

    [NonSerialized]
    public static readonly Dictionary<string, string> PropertyDescriptions = new Dictionary<string, string>
    {
        { "assistantResponse", "You as my lovely assistant clearly narrates what was updated, confirmed or answered in a user's intent." },
        { "style", "Can be overridden per chat" },
        { "imageShape", "Output image shape. Possible values: `Square`, `Horizontal`, `Vertical`, null. Default is null" },
        { "allowNsfw", "Global default unless explicitly toggled | null" },
        { "target", "Session goal: ImageTo3D or WallArt. Default null" }
    };
    [NonSerialized]
    public static readonly string[] RequiredFields = new[]
    {
        "assistantResponse"
    };
}

[Serializable]
public class ImageChatOverridesResponseModel
{
    /// <summary>
    /// You as my lovely assistant clearly narrates what was updated, confirmed or answered in a user's intent.
    /// </summary>
    [JsonPropertyName("assistantResponse")]
    public string assistantResponse { get; set; }
    /// <summary>
    /// Per-chat visual styling
    /// </summary>
    public string style { get; set; } = null;

    /// <summary>
    /// square | horizontal | vertical | null
    /// </summary>
    [JsonPropertyName("imageShape")]
    public string imageShape { get; set; } = null;

    /// <summary>
    /// Overrides session default | null
    /// </summary>
    [JsonPropertyName("allowNsfw")]
    public bool? allowNsfw { get; set; } = null;

    /// <summary>
    /// Soft enum: MoreDetailed | LessDetailed | AsIs | Default
    /// Based on user intent for detail level. Default: Default
    /// </summary>
    [JsonPropertyName("editDetail")]
    public string editDetail { get; set; } = "Default";

    /// <summary>
    /// Soft enum: MoreCreative | LessCreative | AsIs | Default
    /// Based on user intent for creativity level. Default: Default
    /// </summary>
    [JsonPropertyName("t2iCreativity")]
    public string t2iCreativity { get; set; } = "Default";

    /// <summary>
    /// null or empty means fallback to transparent/white; non-empty means custom background prompt
    /// </summary>
    [JsonPropertyName("customBackground")]
    public string customBackground { get; set; } = null;

    /// <summary>
    /// Session goal: ImageTo3D or WallArt. Default null
    /// </summary>
    [JsonPropertyName("target")]
    public string target { get; set; } = null;

    [NonSerialized]
    public static readonly Dictionary<string, string> PropertyDescriptions = new Dictionary<string, string>
    {
        { "assistantResponse", "You as my lovely assistant clearly narrates what was updated, confirmed or answered in a user's intent." },
        { "style", "Per-chat visual styling" },
        { "imageShape", "square | horizontal | vertical | null" },
        { "allowNsfw", "Overrides session default | null" },
        { "editDetail", "Soft enum: MoreDetailed | LessDetailed | AsIs | Default. Based on user intent for detail level. Default: Default" },
        { "t2iCreativity", "Soft enum: MoreCreative | LessCreative | AsIs | Default. Based on user intent for creativity level. Default: Default" },
        { "customBackground", "null or empty means fallback to transparent/white; non-empty means custom background prompt" },
        { "target", "Session goal: ImageTo3D or WallArt. Default null" }
    };
    [NonSerialized]
    public static readonly string[] RequiredFields = new[]
    {
        "assistantResponse"
    };
}

/// <summary>
/// Used for:
/// - explain_rule_system: "You as my lovely assistant clearly narrates what the user asked regarding how this assistant works, its strengths and limitations."
/// - reset_chat_preferences: "You as my lovely assistant clearly indicates to the user that this specific chat preferences have been reset to their original values."
/// - reset_session_preferences: "You as my lovely assistant clearly indicates to the user that session preferences have been reset to their original values."
/// </summary>
[Serializable]
public class AssistantResponseOnlyModel
{
    /// <summary>
    /// You as my lovely assistant clearly narrates what was updated, confirmed or answered in a user's intent.
    /// </summary>
    [JsonPropertyName("assistantResponse")]
    public string assistantResponse { get; set; }

    [NonSerialized]
    public static readonly Dictionary<string, string> PropertyDescriptions = new Dictionary<string, string>
    {
        { "assistantResponse", "You as my lovely assistant clearly narrates what was updated, confirmed or answered in a user's intent." }
    };
    [NonSerialized]
    public static readonly string[] RequiredFields = new[]
    {
        "assistantResponse"
    };
}
