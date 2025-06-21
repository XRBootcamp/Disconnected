using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using REST_API_HANDLER;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;
using System;
using System.IO;
using Microsoft.Unity.VisualStudio.Editor;
using System.Net.Http;
using NUnit.Framework;
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

        public async Task GenerateTextToImage(string description, Action onStartAction, Action<List<Texture2D>> onCompleteAction, Action<ErrorResponseArrayModel> onErrorAction, bool alphaIsTransparency,
            OutputType outputType = OutputType.base64Data, ImageExtensions outputFormat = ImageExtensions.PNG,
            int width = 1024, int height = 1024, int numberResults = 1,
            string negativePrompt = null, bool? isNSFW = null, int? overwriteDefaultSteps = null,
            double? overwriteDefaultCFGScale = null)
        {
            var apiKeyLoader = APIKeyLoader.Instance;
            if (apiKeyLoader == null)
            {
                Debug.LogError("APIKeyLoader instance not found!");
                return;
            }

            try
            {
                TextToImageRequestModel reqModel = new(description, model, outputType, outputFormat, height, width, numberResults, negativePrompt, isNSFW, overwriteDefaultSteps, overwriteDefaultCFGScale);

                // NOTE: Connect UI events here - like loading screen
                if (onStartAction != null)
                {
                    onStartAction.Invoke();
                }

                var result = await apiKeyLoader.RunwareApi.CreateTextToImageAsync(new List<TextToImageRequestModel> {reqModel});

                var textures = await LoadTextures(result.data, alphaIsTransparency);
                
                // NOTE: Connect UI events after completion
                if (onCompleteAction != null)
                {
                    onCompleteAction.Invoke(textures);
                }

                Debug.Log($"[{nameof(RunwareTTI)}] - GenerateTextToImage SUCCESS\n{result.data}");
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
            }
        }

        async Task<List<Texture2D>> LoadTextures(List<TextToImageResponseModel> urls, bool alphaIsTransparency)
        {
            List<Texture2D> listOfTextures = new();
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
                _texture.alphaIsTransparency = alphaIsTransparency;

                ImageManagementExtensions.WriteImageOnDisk(
                    _texture,
                    filename,
                    ImageExtensions.PNG, FileEnumPath.Temporary,
                    FilePaths.TEXT_TO_IMAGE, true
                );
                listOfTextures.Add(_texture);
            }
            return listOfTextures;
        }

        /// <summary>
        /// NOTE: the reason this is deprecated it is because a httpclient should be created per baseUrl
        /// and not a global one, other than this - it was working fine
        /// It could raise some issues when having multiple RunwareTTI at the same time.
        /// </summary>

        #region DEPRECATED
    
        public void GenerateTextToImage(string description, Action<Texture2D> onCompleteAction, Action<ErrorResponseArrayModel> onErrorAction, bool alphaIsTransparency,
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

            TextToImageRequestModel reqModel = new(description, model, outputType, outputFormat, height, width, numberResults, negativePrompt, isNSFW, overwriteDefaultSteps, overwriteDefaultCFGScale);
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
                    onCompleteAction.Invoke(textures.FirstOrDefault<Texture2D>());
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