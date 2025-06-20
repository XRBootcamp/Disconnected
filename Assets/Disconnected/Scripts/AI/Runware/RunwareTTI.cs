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

        public void GenerateTextToImage(string description, Action<Texture2D> onCompleteAction, Action<ErrorResponseArrayModel> onErrorAction, bool alphaIsTransparency,
            OutputType outputType = OutputType.base64Data, ImageExtensions outputFormat = ImageExtensions.PNG,
            int width = 1024, int height = 1024, int numberResults = 1,
            string negativePrompt = null, bool? isNSFW = null, int? overwriteDefaultSteps = null,
            double? overwriteDefaultCFGScale = null)
        {
            if (APIKeyLoader.Instance == null)
            {
                Debug.LogError("APIKeyLoader instance not found!");
                return;
            }

            TextToImageRequestModel reqModel = new(description, model, outputType, outputFormat, height, width, numberResults, negativePrompt, isNSFW, overwriteDefaultSteps, overwriteDefaultCFGScale);
            var json = $"[{reqModel.ToBody()}]";

            ApiCall.instance.PostRequest<TextToImageResponseDataArrayModel>(
                APIKeyLoader.Instance.RunwareApi.BaseUrl,
                APIKeyLoader.Instance.RunwareApi.ToCustomHeader(),
                null, json,
            (result =>
            {
                LoadTextures(result.data, alphaIsTransparency, onCompleteAction);
                Debug.Log($"[{nameof(RunwareTTI)}] - GenerateTextToImage SUCCESS\n{result.data}");

            }), (error =>
            {
                ErrorResponseArrayModel entity = JsonUtility.FromJson<ErrorResponseArrayModel>(error);
                if (onErrorAction != null)
                {
                    onErrorAction.Invoke(entity);
                }
                Debug.LogError($"[{nameof(RunwareTTI)}] - GenerateTextToImage ERROR\n{error}");
            }));
        }

        // TODO: after implement this - this way - it is clearer
        /*

        public async Task<JsonObject?> CreateChatCompletionAsync(JsonObject request)
        {
            var response = await _httpClient.PostAsJsonAsync(BaseUrl + ChatCompletionsEndpoint, request);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"API request failed with status code {response.StatusCode}. Response content: {errorContent}");
            }

            return await response.Content.ReadFromJsonAsync<JsonObject>();
        }
        */


        async void LoadTextures(List<TextToImageResponseModel> urls, bool alphaIsTransparency, Action<Texture2D> completationAction)
        {
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

                if (completationAction != null)
                {
                    completationAction.Invoke(_texture);
                }
            }
        }
    }
}