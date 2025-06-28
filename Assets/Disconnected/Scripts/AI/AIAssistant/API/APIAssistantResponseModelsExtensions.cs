using System;
using Runware;
using UnityEngine;

namespace Assets.Disconnected.Scripts.AI.AIAssistant.API
{
    /// <summary>
    /// Extension methods for AIAssistantText2ImageResponseModel.
    /// </summary>
    public static class APIAssistantResponseModelExtensions
    {
        /// <summary>
        /// Converts an AIAssistantText2ImageResponseModel to a PromptCompiler instance, merging with a previous PromptCompiler if provided.
        /// </summary>
        /// <param name="response">The response model to convert.</param>
        /// <param name="previous">The previous PromptCompiler to merge from (optional).</param>
        /// <returns>A PromptCompiler instance with merged and updated values.</returns>
        /// <exception cref="ArgumentNullException">Thrown if response is null.</exception>
        public static Image3dPromptCompiler ToPromptCompiler(this APIText2ImageResponseModel response, Image3dPromptCompiler previous = null)
        {
            if (response == null)
            {
                throw new ArgumentNullException(nameof(response));
            }

            // Start with previous PromptCompiler's state if provided
            var compiler = new Image3dPromptCompiler(previous);

            // Overwrite with new response values where appropriate
            compiler.model = RunwareExtensions.ToTextToImageAIModel(response.model) ?? TextToImageAIModel.Flux1Schnell;
            compiler.positivePrompt = response.positivePrompt;
            compiler.negativePrompt = response.negativePrompt;
            compiler.allowNsfw = response.isNSFW;
            compiler.customBackground = response.customBackground;
            compiler.imageArtStyle = response.imageArtStyle;
            
            compiler.imageShape = TTIEnumExtensions.ParseImageShape(response.imageShape);
            compiler.target = TTIEnumExtensions.ParseImageTarget(response.target);
            // Debug log all fields
            Debug.Log($"[ToPromptCompiler] model: {compiler.model}, positivePrompt: {compiler.positivePrompt}, negativePrompt: {compiler.negativePrompt}, allowNsfw: {compiler.allowNsfw}, customBackground: {compiler.customBackground}, imageArtStyle: {compiler.imageArtStyle}, imageShape: {compiler.imageShape}, target: {compiler.target}");

            return compiler;
        }
    }
} 