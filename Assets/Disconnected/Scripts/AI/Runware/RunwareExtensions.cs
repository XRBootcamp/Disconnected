using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Runware
{
    // TODO: Image to Image for refinement 

    public static class RunwareExtensions
    {
        // Overloads for RunwareTextToImageModel
        public static string ToAirModelId(this TextToImageAIModel model)
        {
            return model switch
            {
                TextToImageAIModel.Flux1Schnell => "runware:100@1",
                TextToImageAIModel.Flux1Dev => "runware:101@1",
                _ => throw new ArgumentOutOfRangeException(nameof(model), model, null)
            };
        }

        // Overloads for RunwareImageToImageModel
        public static string ToAirModelId(this ImageToImageAIModel model)
        {
            return model switch
            {
                ImageToImageAIModel.Flux1KontextPro => "bfl:3@1",
                ImageToImageAIModel.FluxDevRedux => "runware:105@1",
                _ => throw new ArgumentOutOfRangeException(nameof(model), model, null)
            };
        }

        // Overloads for RunwareTextToImageModel
        public static int? GetDefaultSteps(this TextToImageAIModel model)
        {
            return model switch
            {
                TextToImageAIModel.Flux1Schnell => null,
                TextToImageAIModel.Flux1Dev => null,
                _ => null
            };
        }

        // Overloads for RunwareImageToImageModel
        public static int? GetDefaultSteps(this ImageToImageAIModel model)
        {
            return model switch
            {
                ImageToImageAIModel.Flux1KontextPro => null,
                ImageToImageAIModel.FluxDevRedux => null,
                _ => null
            };
        }

        // Overloads for RunwareTextToImageModel
        public static double? GetDefaultCFGScale(this TextToImageAIModel model)
        {
            return model switch
            {
                TextToImageAIModel.Flux1Schnell => null,
                TextToImageAIModel.Flux1Dev => null,
                _ => null
            };
        }

        // Overloads for RunwareImageToImageModel
        public static double? GetDefaultCFGScale(this ImageToImageAIModel model)
        {
            return model switch
            {
                ImageToImageAIModel.Flux1KontextPro => null,
                ImageToImageAIModel.FluxDevRedux => null,
                _ => null
            };
        }

        // Overloads for RunwareTextToImageModel
        public static bool SupportsNegativePrompt(this TextToImageAIModel model)
        {
            return model switch
            {
                TextToImageAIModel.Flux1Schnell => false,
                TextToImageAIModel.Flux1Dev => false,
                _ => false
            };
        }

        // Overloads for RunwareImageToImageModel
        public static bool SupportsNegativePrompt(this ImageToImageAIModel model)
        {
            return model switch
            {
                ImageToImageAIModel.Flux1KontextPro => false,
                ImageToImageAIModel.FluxDevRedux => false,
                _ => false
            };
        }

        // Overloads for RunwareTextToImageModel
        public static bool SupportsTransparency(this TextToImageAIModel model)
        {
            // NOTE: https://runware.ai/docs/en/image-inference/api-reference#request-advancedfeatures-layerdiffuse 
            // FLUX models support transparency right from image generation
            return model switch
            {
                TextToImageAIModel.Flux1Schnell => true,
                TextToImageAIModel.Flux1Dev => true,
                _ => false
            };
        }

        // Overloads for RunwareImageToImageModel
        public static bool SupportsTransparency(this ImageToImageAIModel model)
        {
            return model switch
            {
                // FLUX models support transparency right from image generation
                ImageToImageAIModel.Flux1KontextPro => true,
                ImageToImageAIModel.FluxDevRedux => true,
                _ => false
            };
        }

        public static int ValidateDivisibleBy64(int value)
        {
            if (value % 64 != 0)
                throw new ArgumentException($"The value {value} is not divisible by 64.");

            return value;
        }

        /// <summary>
        /// Generates a valid UUIDv4 string.
        /// </summary>
        /// <returns>A string representation of a UUIDv4.</returns>
        public static string GenerateUUIDv4String()
        {
            // System.Guid.NewGuid() generates a UUIDv4-compliant GUID
            return Guid.NewGuid().ToString();
        }


        public static string RemoveEmptyOrNullFields(string json, List<string> fieldNames)
        {
            foreach (var field in fieldNames)
            {
                // Remove empty string fields
                json = System.Text.RegularExpressions.Regex.Replace(
                    json, $"\"{field}\":\"\"", "", System.Text.RegularExpressions.RegexOptions.None);

                // Remove null fields (JsonUtility usually omits nulls, but just in case)
                json = System.Text.RegularExpressions.Regex.Replace(
                    json, $"\"{field}\":null", "", System.Text.RegularExpressions.RegexOptions.None);

                // Remove trailing commas left by removal
                json = System.Text.RegularExpressions.Regex.Replace(
                    json, ",\\s*}", "}", System.Text.RegularExpressions.RegexOptions.None);
                json = System.Text.RegularExpressions.Regex.Replace(
                    json, "{\\s*,", "{", System.Text.RegularExpressions.RegexOptions.None);
            }
            return json;
        }

        /// <summary>
        /// Returns the width and height dimensions for a given ImageShape.
        /// </summary>
        /// <param name="shape">The ImageShape enum value.</param>
        /// <returns>A tuple containing (width, height) dimensions.</returns>
        public static (int width, int height) GetDimensions(this ImageShape shape)
        {
            return shape switch
            {
                ImageShape.Square => (1024, 1024),
                ImageShape.Horizontal => (1024, 768),
                ImageShape.Vertical => (768, 1024),
                _ => throw new ArgumentOutOfRangeException(nameof(shape), shape, null)
            };
        }
    }

}