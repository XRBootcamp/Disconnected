// Root myDeserializedClass = JsonConvert.DeserializeObject<List<Root>>(myJsonResponse);

using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UIElements;

// NOTE: JsonSerializer.Serialize either requires: all values {get ; set; }, or the attribute in every field that is to be included in the Serializ {get; set;}
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
    public class GenerateTextToImageOutputModel
    {
        public string imagePath {get; set;}
        public Texture2D texture {get; set;}

        public GenerateTextToImageOutputModel(string imagePath, Texture2D texture)
        {
            this.imagePath = imagePath;
            this.texture = texture;
        }
    }

    [Serializable]
    // NOTE: according to documentation: https://runware.ai/docs/en/getting-started/how-to-connect
    public class TextToImageRequestModel
    {
        public string taskType {get; set;} = "imageInference";
        public string taskUUID {get; set;}
        public string positivePrompt {get; set;}
        public string model {get; set;}
        public string outputType {get; set;}
        public string outputFormat {get; set;}
        public int height {get; set;}
        public int width {get; set;}
        public int numberResults {get; set;} // default: 1
        public TextToImageRequestAdvancedFeatures advancedFeatures {get; set;} = null;
        public bool? includeCost {get; set;} = true; // set as default so we always know

        public bool? checkNSFW {get; set;}

        [CanBeNull]public string negativePrompt {get; set;}

        public int? steps {get; set;} // default: recommended by model, if none - whatever runware defines as default
        public double? CFGScale {get; set;} // default: recommended by model, if none - whatever runware defines as default

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
        public bool layerDiffuse {get; set;}
    }


    [Serializable]
    public class TextToImageResponseDataArrayModel
    {
        public List<TextToImageResponseModel> data {get; set;}
    }

    [Serializable]
    public class TextToImageResponseModel
    {
        public string taskType {get; set;}
        public string imageUUID {get; set;}
        public string taskUUID {get; set;}
        public double cost {get; set;}
        public long seed {get; set;}
        public string imageURL {get; set;}
        public string imageBase64Data {get; set;}
        public string positivePrompt {get; set;}
        public bool? NSFWContent {get; set;}
    }


    [Serializable]
    public class ErrorResponseArrayModel
    {
        public List<ErrorResponseModel> errors {get; set;}
    }

    [Serializable]
    public class ErrorResponseModel
    {
        public string code {get; set;}
        public string message {get; set;}
        public string parameter {get; set;}
        public string type {get; set;}
        public string taskType {get; set;}
    }

}