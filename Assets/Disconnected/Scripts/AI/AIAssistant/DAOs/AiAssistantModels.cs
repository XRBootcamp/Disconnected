using System;
using System.Text.Json.Serialization;
using Runware;
using UnityEngine;

[Serializable]
public class ChatInternalMemory
{
    public string assistantResponse;
    public PromptCompiler promptCompiler;
    public ImageChatOverrides chatOverrides;
    public ImageSessionPreferences sessionPreferences;

}

/// <summary>
/// Unity model for session preferences - used in actual Unity code
/// </summary>
[Serializable]
public class ImageSessionPreferences
{
    /// <summary>
    /// Can be overridden per chat
    /// </summary>
    public string imageArtStyle { get; set; } = null;

    /// <summary>
    /// Square | Horizontal | Vertical
    /// </summary>
    public ImageShape? imageShape { get; set; } = null;

    /// <summary>
    /// Global default unless explicitly toggled
    /// </summary>
    public bool? allowNsfw { get; set; } = null;

    /// <summary>
    /// Session goal: ImageTo3D or WallArt
    /// </summary>
    public ImageTarget? target { get; set; } = null;

    public ImageSessionPreferences() {}
    /// <summary>
    /// Convert from API response model to Unity model
    /// </summary>
    public static ImageSessionPreferences FromResponseModel(ImageSessionPreferencesResponseModel responseModel)
    {
        return new ImageSessionPreferences
        {
            imageArtStyle = responseModel.style,
            allowNsfw = responseModel.allowNsfw,
            imageShape = TTIEnumExtensions.ParseImageShape(responseModel.imageShape),
            target = TTIEnumExtensions.ParseImageTarget(responseModel.target)
        };
    }

    /// <summary>
    /// Convert Unity model to API response model
    /// </summary>
    public ImageSessionPreferencesResponseModel ToResponseModel()
    {
        return new ImageSessionPreferencesResponseModel
        {
            style = this.imageArtStyle,
            allowNsfw = this.allowNsfw,
            imageShape = this.imageShape.ToString(),
            target = this.target.ToString()
        };
    }
}

/// <summary>
/// Unity model for chat overrides - used in actual Unity code
/// </summary>
[Serializable]
public class ImageChatOverrides
{
    /// <summary>
    /// Per-chat visual styling
    /// </summary>
    public string imageArtStyle { get; set; } = null;

    /// <summary>
    /// Square | Horizontal | Vertical
    /// </summary>
    public ImageShape? imageShape { get; set; } = null;

    /// <summary>
    /// Overrides session default
    /// </summary>
    public bool? allowNsfw { get; set; } = null;

    /// <summary>
    /// Detail level for image generation
    /// </summary>
    public ImageDetailLevel editDetail { get; set; } = ImageDetailLevel.Default;

    /// <summary>
    /// Creativity level for image generation
    /// </summary>
    public ImageCreativityLevel t2iCreativity { get; set; } = ImageCreativityLevel.Default;

    /// <summary>
    /// null or empty means fallback to transparent/white; non-empty means custom background prompt
    /// </summary>
    public string customBackground { get; set; } = null;

    /// <summary>
    /// Session goal: ImageTo3D or WallArt
    /// </summary>
    public ImageTarget? target { get; set; } = null;

    public ImageChatOverrides() {}

    /// <summary>
    /// Convert from API response model to Unity model
    /// </summary>
    public static ImageChatOverrides FromResponseModel(ImageChatOverridesResponseModel responseModel)
    {
        return new ImageChatOverrides
        {
            imageArtStyle = responseModel.style,
            imageShape = TTIEnumExtensions.ParseImageShape(responseModel.imageShape),
            allowNsfw = responseModel.allowNsfw,
            editDetail = TTIEnumExtensions.ParseImageDetailLevel(responseModel.editDetail),
            t2iCreativity = TTIEnumExtensions.ParseImageCreativityLevel(responseModel.t2iCreativity),
            customBackground = responseModel.customBackground,
            target = TTIEnumExtensions.ParseImageTarget(responseModel.target)
        };
    }

    /// <summary>
    /// Convert Unity model to API response model
    /// </summary>
    public ImageChatOverridesResponseModel ToResponseModel()
    {
        return new ImageChatOverridesResponseModel
        {
            style = this.imageArtStyle,
            imageShape = this.imageShape.ToString(),
            allowNsfw = this.allowNsfw,
            editDetail = this.editDetail.ToString(),
            t2iCreativity = this.t2iCreativity.ToString(),
            customBackground = this.customBackground,
            target = this.target.ToString()
        };
    }

}

[Serializable]
public class PromptCompiler
{
    public TextToImageAIModel model { get; set; }
    public string positivePrompt { get; set; }
    public string negativePrompt { get; set; }
    public ImageShape? imageShape { get; set; } = null;
    public bool? allowNsfw { get; set; } = null;
    public string customBackground { get; set; }
    public string imageArtStyle { get; set; }
    public ImageTarget? target { get; set; } = null;
    public int? steps { get; set; } = 20;
    public double? cfgscale { get; set; } = 7;

    [NonSerialized] private int currentSteps = 20;
    [NonSerialized] private double currentCfgscale = 7;
    [NonSerialized] private int defaultSteps = 20; // according to Runware documentation
    [NonSerialized] private double defaultCfgscale = 7; // according to Runware documentation

    public PromptCompiler() {}
    // New constructor for merging
    public PromptCompiler(PromptCompiler previous = null)
    {
        if (previous != null)
        {
            model = previous.model;
            positivePrompt = previous.positivePrompt;
            negativePrompt = previous.negativePrompt;
            imageShape = previous.imageShape;
            allowNsfw = previous.allowNsfw;
            customBackground = previous.customBackground;
            imageArtStyle = previous.imageArtStyle;
            target = previous.target;
            steps = previous.steps;
            cfgscale = previous.cfgscale;
            currentSteps = previous.currentSteps;
            currentCfgscale = previous.currentCfgscale;
            defaultSteps = previous.defaultSteps;
            defaultCfgscale = previous.defaultCfgscale;
        }
    }

    public TextToImageRequestModel ToTextToImageRequestModel()
    {
        string newPositivePrompt = this.WritePositivePromptAccordingToModel();
        string newNegativePrompt = this.WriteNegativePromptAccordingToModel();
        bool alphaIsTransparency = string.IsNullOrEmpty(customBackground);
        int? overwriteDefaultSteps = steps;
        double? overwriteDefaultCFGScale = cfgscale;
        ImageShape shape = this.imageShape ?? ImageShape.Square;
        var imageDimensions = shape.GetDimensions();

        // default values - ignore
        if (overwriteDefaultSteps == 20)
        {
            overwriteDefaultSteps = null;
        }
        if (overwriteDefaultCFGScale == 7)
        {
            overwriteDefaultCFGScale = null;
        }

        return new(
            prompt: newPositivePrompt,
            model: model,
            type: OutputType.base64Data,
            format: ImageExtensions.PNG,
            alphaIsTransparency: alphaIsTransparency,
            height: imageDimensions.height,
            width: imageDimensions.width,
            numberResults: 1,
            negativePrompt: newNegativePrompt,
            isNSFW: this.allowNsfw,
            overwriteDefaultSteps: overwriteDefaultSteps,
            overwriteDefaultCFGScale: overwriteDefaultCFGScale
        );
    }

    public void UpdatePrompt(string newDescription)
    {
        positivePrompt = newDescription;
    }

    public void AdjustCreativity(string t2iCreativity)
    {
        ImageCreativityLevel creativityLevel = TTIEnumExtensions.ParseImageCreativityLevel(t2iCreativity);

        currentCfgscale = creativityLevel switch
        {
            ImageCreativityLevel.MoreCreative => Mathf.Min(50f, (float)currentCfgscale + 2),
            ImageCreativityLevel.LessCreative => Mathf.Max(0f, (float)currentCfgscale - 2),
            ImageCreativityLevel.AsIs => currentCfgscale,
            ImageCreativityLevel.Default => defaultCfgscale,
            _ => throw new NotImplementedException()
        };

        cfgscale = (currentCfgscale == defaultCfgscale ? null : currentCfgscale);
    }

    public void AdjustDetail(string editDetail)
    {
        ImageDetailLevel detailLevel = TTIEnumExtensions.ParseImageDetailLevel(editDetail);

        currentSteps = detailLevel switch
        {
            ImageDetailLevel.MoreDetailed => Mathf.Min(100, currentSteps + 5),
            ImageDetailLevel.LessDetailed => Mathf.Max(0, currentSteps - 5),
            ImageDetailLevel.AsIs => currentSteps,
            ImageDetailLevel.Default => defaultSteps,
            _ => throw new NotImplementedException()
        };

        steps = (currentSteps == defaultSteps ? null : currentSteps);
    }

    internal string WritePositivePromptAccordingToModel()
    {
        return !model.SupportsNegativePrompt() && !string.IsNullOrEmpty(negativePrompt) ?
            $"{positivePrompt}\nDo not Include: {negativePrompt}" :
            positivePrompt;
    }

    internal string WriteNegativePromptAccordingToModel()
    {
        return model.SupportsNegativePrompt() && !string.IsNullOrEmpty(negativePrompt) ? negativePrompt : null;
    }
}
