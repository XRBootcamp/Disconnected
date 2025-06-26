using System.Collections.Generic;
using System.Threading.Tasks;
using REST_API_HANDLER;
using UnityEngine;
using System;
using System.Net.Http;
using System.Linq;

namespace Runware
{
    public class RunwareTTI : MonoBehaviour
    {
        public TextToImageAIModel model;
        public OutputType outputType = OutputType.base64Data;
        public ImageExtensions outputFormat = ImageExtensions.PNG;
        public int height = 1024;
        public int width = 1024;
        public int numberResults = 1;

        public async Task<List<GenerateTextToImageOutputModel>> GenerateTextToImage(TextToImageRequestModel request, Action onStartAction, Action<List<GenerateTextToImageOutputModel>> onCompleteAction, Action<ErrorResponseArrayModel> onErrorAction)
        {
            var apiKeyLoader = APIKeyLoader.Instance;
            if (apiKeyLoader == null)
            {
                Debug.LogError("APIKeyLoader instance not found!");
                return null;
            }
            try
            {

                // NOTE: Connect UI events here - like loading screen
                if (onStartAction != null)
                {
                    onStartAction.Invoke();
                }

                var result = await apiKeyLoader.RunwareApi.CreateTextToImageAsync(new List<TextToImageRequestModel> { request });

                bool alphaIsTransparency = request.advancedFeatures?.layerDiffuse ?? false;
                var outputResult = await LoadTextures(result.data, alphaIsTransparency);

                // NOTE: Connect UI events after completion
                if (onCompleteAction != null)
                {
                    onCompleteAction.Invoke(outputResult);
                }

                Debug.Log($"[{nameof(RunwareTTI)}] - GenerateTextToImage SUCCESS\n{result.data}");

                return outputResult;
            }
            catch (HttpRequestException e)
            {
                ErrorResponseArrayModel entity = JsonUtility.FromJson<ErrorResponseArrayModel>(e.Message);

                // NOTE: Connect UI - error action events
                if (onErrorAction != null)
                {
                    onErrorAction.Invoke(entity);
                }
                Debug.LogError($"[{nameof(RunwareTTI)}] - GenerateTextToImage ERROR\n{e.Message}");

                return null;
            }
    
            
        }
        public async Task<List<GenerateTextToImageOutputModel>> GenerateTextToImage(string description, Action onStartAction, Action<List<GenerateTextToImageOutputModel>> onCompleteAction, Action<ErrorResponseArrayModel> onErrorAction, bool alphaIsTransparency,
            OutputType outputType = OutputType.base64Data, ImageExtensions outputFormat = ImageExtensions.PNG,
            ImageShape imageShape = ImageShape.Square, int numberResults = 1,
            string negativePrompt = null, bool? isNSFW = null, int? overwriteDefaultSteps = null,
            double? overwriteDefaultCFGScale = null)
        {
            // default values - ignore
            if (overwriteDefaultSteps == 20)
            {
                overwriteDefaultSteps = null;
            }
            if (overwriteDefaultCFGScale == 7)
            {
                overwriteDefaultCFGScale = null;
            }
            
            var imageDimensions = imageShape.GetDimensions();
            
            TextToImageRequestModel request = new(
                prompt: description,
                model: model,
                type: outputType,
                format: outputFormat,
                alphaIsTransparency: alphaIsTransparency,
                height: imageDimensions.height,
                width: imageDimensions.width,
                numberResults: numberResults,
                negativePrompt: negativePrompt,
                isNSFW: isNSFW,
                overwriteDefaultSteps: overwriteDefaultSteps,
                overwriteDefaultCFGScale: overwriteDefaultCFGScale
            );

            return await GenerateTextToImage(
                request: request,
                onStartAction: onStartAction,
                onCompleteAction: onCompleteAction,
                onErrorAction: onErrorAction
            );

        }

        async Task<List<GenerateTextToImageOutputModel>> LoadTextures(List<TextToImageResponseModel> urls, bool alphaIsTransparency)
        {
            List<GenerateTextToImageOutputModel> listOfTextures = new();
            for (int i = 0; i < urls.Count; i++)
            {
                string imageUrl = urls[i].imageURL;
                string imageBase64Data = urls[i].imageBase64Data;
                Texture2D _texture;
                if (imageUrl != null && imageUrl != "")
                {
                    _texture = await ImageManagementExtensions.GetRemoteTexture(imageUrl);
                }
                else if (imageBase64Data != null && imageBase64Data != "")
                {
                    _texture = ImageManagementExtensions.ConvertBase64ToTexture(imageBase64Data);
                }
                else
                {
                    Debug.LogError($"[{nameof(RunwareTTI)}] - LoadTextures Error for task {i} of {urls[i].taskUUID}");
                    break;
                }

                string filename = $"RunwareT2I_{i.ToString("0.##")}";

                _texture.name = filename;
                // TODO: handle Texture importer to make alpha transparency
#if UNITY_EDITOR
                _texture.alphaIsTransparency = alphaIsTransparency;
#endif

                string imagePath = ImageManagementExtensions.WriteImageOnDisk(
                    _texture,
                    filename,
                    ImageExtensions.PNG, 
                    FileEnumPath.Temporary,
                    FilePaths.TEXT_TO_IMAGE, true
                );
                listOfTextures.Add( new (imagePath: imagePath, texture: _texture) );
            }
            return listOfTextures;
        }

        /// <summary>
        /// NOTE: the reason this is deprecated it is because a httpclient should be created per baseUrl
        /// and not a global one, other than this - it was working fine
        /// It could raise some issues when having multiple RunwareTTI at the same time.
        /// </summary>

        #region DEPRECATED

        public void GenerateTextToImage(string description, Action<GenerateTextToImageOutputModel> onCompleteAction, Action<ErrorResponseArrayModel> onErrorAction, bool alphaIsTransparency,
            OutputType outputType = OutputType.base64Data, ImageExtensions outputFormat = ImageExtensions.PNG,
            int width = 1024, int height = 1024, int numberResults = 1,
            string negativePrompt = null, bool? isNSFW = null, int? overwriteDefaultSteps = null,
            double? overwriteDefaultCFGScale = null)
        {
            var apiKeyLoader = APIKeyLoader.Instance;
            if (apiKeyLoader == null)
            {
                UnityEngine.Debug.LogError("APIKeyLoader instance not found!");
                return;
            }

            TextToImageRequestModel reqModel = new(description, model, outputType, outputFormat, alphaIsTransparency, height, width, numberResults, negativePrompt, isNSFW, overwriteDefaultSteps, overwriteDefaultCFGScale);
            var json = $"[{reqModel.ToBody()}]";

            ApiCall.instance.PostRequest<TextToImageResponseDataArrayModel>(
                apiKeyLoader.RunwareApi.BaseUrl,
                apiKeyLoader.RunwareApi.ToCustomHeader(),
                null, json,
            (async result =>
            {
                var textures = await LoadTextures(result.data, alphaIsTransparency);

                if (onCompleteAction != null)
                {
                    onCompleteAction.Invoke(textures.FirstOrDefault<GenerateTextToImageOutputModel>());
                }
                UnityEngine.Debug.Log($"[{nameof(RunwareTTI)}] - GenerateTextToImage SUCCESS\n{result.data}");

            }), (error =>
            {
                ErrorResponseArrayModel entity = JsonUtility.FromJson<ErrorResponseArrayModel>(error);
                if (onErrorAction != null)
                {
                    onErrorAction.Invoke(entity);
                }
                UnityEngine.Debug.LogError($"[{nameof(RunwareTTI)}] - GenerateTextToImage ERROR\n{error}");
            }));
        }

        #endregion
    }
}