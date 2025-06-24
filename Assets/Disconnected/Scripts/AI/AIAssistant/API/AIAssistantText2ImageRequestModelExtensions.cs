using System;

namespace Assets.Disconnected.Scripts.AI.AIAssistant.API
{
    /// <summary>
    /// Extension methods for AIAssistantText2ImageResponseModel.
    /// </summary>
    public static class AIAssistantText2ImageRequestModelExtensions
    {
        /// <summary>
        /// Converts an AIAssistantText2ImageResponseModel to a PromptCompiler instance, merging with a previous PromptCompiler if provided.
        /// </summary>
        /// <param name="response">The response model to convert.</param>
        /// <param name="previous">The previous PromptCompiler to merge from (optional).</param>
        /// <returns>A PromptCompiler instance with merged and updated values.</returns>
        /// <exception cref="ArgumentNullException">Thrown if response is null.</exception>
        public static AIAssistantChatTextToImageRequestModel CreateTextToImageRequestModel(
            this ChatInternalMemory chatData, 
            string newUserIntent,
            string userRequestIntro
        )
        {
            if (chatData == null)
            {
                throw new ArgumentNullException(nameof(chatData));
            }

            // Start with previous PromptCompiler's state if provided
            var request = new AIAssistantChatTextToImageRequestModel();

            // Overwrite with new response values where appropriate
            request.UserIntent = newUserIntent;
            request.UserRequestIntro = userRequestIntro;
            request.Text2ImageModel = chatData.promptCompiler.model.ToString();
            request.PreviousAssistantResponse = chatData.assistantResponse;
            request.PreviousPositivePrompt = chatData.promptCompiler.positivePrompt;
            request.PreviousNegativePrompt = chatData.promptCompiler.negativePrompt;

            // Precedence rule: (2) chat overrides, (3) session preferences

            // CustomBackground
            string customBackground = null;
            /*
            if (!string.IsNullOrEmpty(chatData.promptCompiler.customBackground))
            {
                customBackground = chatData.promptCompiler.customBackground;
            }
            else 
            */
            if (!string.IsNullOrEmpty(chatData.chatOverrides.customBackground))
            {
                customBackground = chatData.chatOverrides.customBackground;
            }
            if (customBackground != null)
                request.CustomBackground = customBackground;


            // Image Art Style
            string imageArtStyle = null;
            /*
            if (!string.IsNullOrEmpty(chatData.promptCompiler.imageArtStyle))
            {
                imageArtStyle = chatData.promptCompiler.imageArtStyle;
            }
            else
            */ 
            if (!string.IsNullOrEmpty(chatData.chatOverrides.imageArtStyle))
            {
                imageArtStyle = chatData.chatOverrides.imageArtStyle;
            }
            else if (!string.IsNullOrEmpty(chatData.sessionPreferences.imageArtStyle))
            {
                imageArtStyle = chatData.sessionPreferences.imageArtStyle;
            }
            if (imageArtStyle != null)
                request.ArtStyle = imageArtStyle;

            // Image Shape
            ImageShape? imageShape = null;
            /*
            if (chatData.promptCompiler.imageShape != null)
            {
                imageShape = chatData.promptCompiler.imageShape;
            }
            else 
            */
            if (chatData.chatOverrides.imageShape != null)
            {
                imageShape = chatData.chatOverrides.imageShape;
            }
            else if (chatData.sessionPreferences.imageShape != null)
            {
                imageShape = chatData.sessionPreferences.imageShape;
            }
            if (imageShape != null)
                request.ImageShape = imageShape ?? ImageShape.Square;

            // AllowNSFW
            bool? allowNsfw = null;
            /*
            if (chatData.promptCompiler.allowNsfw != null)
            {
                allowNsfw = chatData.promptCompiler.allowNsfw;
            }
            else 
            */
            if (chatData.chatOverrides.allowNsfw != null)
            {
                allowNsfw = chatData.chatOverrides.allowNsfw;
            }
            else if (chatData.sessionPreferences.allowNsfw != null)
            {
                allowNsfw = chatData.sessionPreferences.allowNsfw;
            }
            if (allowNsfw != null)
                request.AllowNSFW = allowNsfw ?? false;

            // ImageTargetGoal
            ImageTarget? target = null;
            /*
            if (chatData.promptCompiler.target != null)
            {
                target = chatData.promptCompiler.target;
            }
            else
            */
            if (chatData.chatOverrides.target != null)
            {
                target = chatData.chatOverrides.target;
            }
            else if (chatData.sessionPreferences.target != null)
            {
                target = chatData.sessionPreferences.target;
            }
            if (target != null)
                request.ImageTargetGoal = target ?? ImageTarget.ImageTo3D;

            return request;
        }
    }
} 