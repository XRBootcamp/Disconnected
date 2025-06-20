// Root myDeserializedClass = JsonConvert.DeserializeObject<List<Root>>(myJsonResponse);

using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UIElements;

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
        public string taskType = "imageInference";
        public string taskUUID;
        public string positivePrompt;
        public string model;
        public string outputType;
        public string outputFormat;
        public int height;
        public int width;
        public int numberResults; // default: 1
        public bool? includeCost = true; // set as default so we always know

        public bool? checkNSFW;
        
        [CanBeNull] public string negativePrompt;

        public int? steps; // default: recommended by model, if none - whatever runware defines as default
        public double? CFGScale; // default: recommended by model, if none - whatever runware defines as default

        public TextToImageRequestModel(
            string prompt,
            TextToImageAIModel model,
            OutputType type,
            ImageExtensions format,
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



    [Serializable]
    public class TextToImageResponseDataArrayModel
    {
        public List<TextToImageResponseModel> data;
    }

    [Serializable]
    public class TextToImageResponseModel
    {
        public string taskType;
        public string imageUUID;
        public string taskUUID;
        public double cost;
        public long seed;
        public string imageURL;
        public string imageBase64Data;
        public string positivePrompt;
        public bool? NSFWContent;
    }


    [Serializable]
    public class ErrorResponseArrayModel
    {
        public List<ErrorResponseModel> errors;
    }

    [Serializable]
    public class ErrorResponseModel
    {
        public string code;
        public string message;
        public string parameter;
        public string type;
        public string taskType;
    }

}