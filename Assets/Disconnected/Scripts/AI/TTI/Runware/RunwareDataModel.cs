// Root myDeserializedClass = JsonConvert.DeserializeObject<List<Root>>(myJsonResponse);

using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UIElements;

// NOTE: JsonSerializer.Serialize either requires: all values {get ; set; }, or the attribute [JsonInclude] in every field that is to be included in the Serialize
namespace Runware
{

    public enum OutputType
    {
        URL = 0,
        dataURI = 1,
        base64Data = 2
    }

    // NOTE: Add models - go to https://my.runware.ai/models/all and copy their ids 
    // Text-to-Image models
    public enum TextToImageAIModel
    {
        Flux1Schnell = 0, // runware:100@1
        Flux1Dev = 1      // runware:101@1
    }

    // Image-to-Image models
    public enum ImageToImageAIModel
    {
        Flux1KontextPro = 0, // bfl:3@1
        FluxDevRedux = 1     // runware:105@1
    }

    [Serializable]
    // NOTE: according to documentation: https://runware.ai/docs/en/getting-started/how-to-connect
    public class TextToImageRequestModel
    {
        [JsonInclude] public string taskType = "imageInference";
        [JsonInclude] public string taskUUID;
        [JsonInclude] public string positivePrompt;
        [JsonInclude] public string model;
        [JsonInclude] public string outputType;
        [JsonInclude] public string outputFormat;
        [JsonInclude] public int height;
        [JsonInclude] public int width;
        [JsonInclude] public int numberResults; // default: 1
        [JsonInclude] public TextToImageRequestAdvancedFeatures advancedFeatures;
        [JsonInclude] public bool? includeCost = true; // set as default so we always know

        [JsonInclude] public bool? checkNSFW;

        [CanBeNull][JsonInclude] public string negativePrompt;

        [JsonInclude] public int? steps; // default: recommended by model, if none - whatever runware defines as default
        [JsonInclude] public double? CFGScale; // default: recommended by model, if none - whatever runware defines as default

        public TextToImageRequestModel(
            string prompt,
            TextToImageAIModel model,
            OutputType type,
            ImageExtensions format,
            bool alphaIsTransparency,
            int height = 1024,
            int width = 1024,
            int numberResults = 1,
            // optionals
            string negativePrompt = null,
            bool? isNSFW = null,
            int? overwriteDefaultSteps = null,
            double? overwriteDefaultCFGScale = null
        )
        {
            this.taskUUID = RunwareExtensions.GenerateUUIDv4String();
            this.positivePrompt = prompt;
            this.model = RunwareExtensions.ToAirModelId(model);
            this.outputType = type.ToString();
            this.outputFormat = format.ToString();
            this.height = RunwareExtensions.ValidateDivisibleBy64(height);
            this.width = RunwareExtensions.ValidateDivisibleBy64(width);
            this.numberResults = numberResults;

            // We want transparency supported only if we set transparency, and model supports it
            if (alphaIsTransparency && model.SupportsTransparency())
            {
                this.advancedFeatures = new TextToImageRequestAdvancedFeatures();
                this.advancedFeatures.layerDiffuse = true;
            }

            if (model.SupportsNegativePrompt())
            {
                this.negativePrompt = negativePrompt;
            }
            else
            {
                this.negativePrompt = null;
            }

            if (isNSFW != null)
            {
                this.checkNSFW = isNSFW;
            }

            int? defaultModelSteps = model.GetDefaultSteps();
            if (overwriteDefaultSteps != null)
            {
                this.steps = overwriteDefaultSteps;
            }
            else if (defaultModelSteps != null)
            {
                this.steps = defaultModelSteps;
            }

            double? defaultModelCFGScale = model.GetDefaultCFGScale();
            if (overwriteDefaultCFGScale != null)
            {
                this.CFGScale = overwriteDefaultCFGScale;
            }
            else if (defaultModelCFGScale != null)
            {
                this.CFGScale = defaultModelCFGScale;
            }


        }

        /// <summary>
        /// Serializes the current object to a JSON string, removing empty or null fields,
        /// specifically excluding the "negativePrompt" field if it is empty or null.
        /// </summary>
        /// <returns>A JSON string representation of the object with specified fields removed if empty or null.</returns>
        public string ToBody()
        {
            // Serialize this object to JSON
            string jsonString = JsonUtility.ToJson(this);

            // Prepare a list of fields to remove if they are empty or null
            List<string> fieldsToRemove = new List<string> { "negativePrompt" };

            // Remove empty or null fields from the JSON string
            jsonString = RunwareExtensions.RemoveEmptyOrNullFields(jsonString, fieldsToRemove);

            // Log the resulting JSON string for debugging purposes
            Debug.Log($"[{nameof(TextToImageRequestModel)}] >> {jsonString}");

            // Return the cleaned JSON string
            return jsonString;
        }
    }


    // TODO: if alpha is enabled, add this to TextToImageRequestModel
    // https://runware.ai/docs/en/image-inference/api-reference#request-advancedfeatures
    /**
     *  "advancedFeatures": {
            "layerDiffuse": true
        },
     * 
     */

    [Serializable]
    public class TextToImageRequestAdvancedFeatures
    {
        [JsonInclude] public bool layerDiffuse;
    }


    [Serializable]
    public class TextToImageResponseDataArrayModel
    {
        [JsonInclude] public List<TextToImageResponseModel> data;
    }

    [Serializable]
    public class TextToImageResponseModel
    {
        [JsonInclude] public string taskType;
        [JsonInclude] public string imageUUID;
        [JsonInclude] public string taskUUID;
        [JsonInclude] public double cost;
        [JsonInclude] public long seed;
        [JsonInclude] public string imageURL;
        [JsonInclude] public string imageBase64Data;
        [JsonInclude] public string positivePrompt;
        [JsonInclude] public bool? NSFWContent;
    }


    [Serializable]
    public class ErrorResponseArrayModel
    {
        [JsonInclude] public List<ErrorResponseModel> errors;
    }

    [Serializable]
    public class ErrorResponseModel
    {
        [JsonInclude] public string code;
        [JsonInclude] public string message;
        [JsonInclude] public string parameter;
        [JsonInclude] public string type;
        [JsonInclude] public string taskType;
    }

}