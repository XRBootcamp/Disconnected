using System;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using GLTFast;
using UnityEditor;

public class SF3DAPIClient : IDisposable
{
    private const string apiUrl = "https://api.stability.ai/v2beta/3d/stable-fast-3d";
    private string apiKey = "YOUR_API_KEY";
    private string unityEditorPrefabDirectory = "Assets/GeneratedPrefabs/";

    public SF3DAPIClient(string newApiKey, string prefabDirectory = null)
    {
        apiKey = newApiKey;
        this.unityEditorPrefabDirectory = prefabDirectory ?? this.unityEditorPrefabDirectory;
    }

    /// <summary>
    /// Generates a 3D model from an image using the Stable Fast 3D API.
    /// </summary>
    /// <param name="imagePath">Path to the input image file.</param>
    /// <param name="outputPath">Path to save the generated GLB model file.</param>
    /// <param name="parent">Parent transform for the loaded model.</param>
    public async Task<GameObject> Generate3DModelAsync(
        string imagePath,
        string filename,
        FileEnumPath fileEnumPath,
        System.Action<GameObject> onModelLoaded,
        Transform parent = null)
    {
        if (!File.Exists(imagePath))
        {
            Debug.LogError($"Image file not found at: {imagePath}");
            return null;
        }

        byte[] imageBytes = File.ReadAllBytes(imagePath);
        WWWForm form = new WWWForm();
        form.AddBinaryData("image", imageBytes, Path.GetFileName(imagePath), $"image/{Path.GetExtension(imagePath).ToLower()}");//"image/png");

        using (UnityWebRequest request = UnityWebRequest.Post(apiUrl, form))
        {
            request.SetRequestHeader("Authorization", $"Bearer {apiKey}");
            request.downloadHandler = new DownloadHandlerBuffer();

            var operation = request.SendWebRequest();
            while (!operation.isDone)
            {
                await Task.Yield();
            }

            if (request.result == UnityWebRequest.Result.Success)
            {
                byte[] responseData = request.downloadHandler.data;
                Debug.Log("3D model generated successfully!");

                // Moved filePath and saving to the extension functions to always save in quest
                string filePath = FileManagementExtensions.GenerateFilePath(
                    appPath: fileEnumPath,
                    fileName: filename,
                    relativePath: FilePaths.IMAGE_TO_3D,
                    extension: FileExtensions.GLB,
                    appendDateTime: false
                );

                FileManagementExtensions.SaveInRootedDataPath(
                    rootPath: filePath,
                    relativePath: null,
                    data: responseData
                );

                // Load Model from responseData
                GameObject loadedModel = await LoadModel(
                    modelPath: filePath,
                    parentTransform: parent,
                    onModelLoaded: onModelLoaded
                );

                return loadedModel;
            }
            else
            {
                Debug.LogError($"Error {request.responseCode}: {request.error}");
                return null;
            }
        }
    }

    // Load the generated model using GLTFUtility
    async Task<GameObject> LoadModel(string modelPath, Transform parentTransform, System.Action<GameObject> onModelLoaded)
    {
        var gltf = new GltfImport();

        var success = await gltf.Load("file://" + modelPath);

        if (success)
        {
            GameObject loadedModel = new GameObject(Path.GetFileNameWithoutExtension(modelPath));
            // NOTE: unsure if I should do this here
            //loadedModel.transform.SetParent(parentTransform);

            success = await gltf.InstantiateMainSceneAsync(loadedModel.transform);

            if (success)
            {
                Debug.Log("Model loaded into Unity.");

                if (onModelLoaded != null)
                {
                    onModelLoaded?.Invoke(loadedModel);
                }
                
            }
            else
            {
                Debug.LogError("glTFast instantiation failed.");
                if (onModelLoaded != null)
                {
                    onModelLoaded?.Invoke(null);
                }
                
            }
            // Moved to AI Assistant
/*
#if UNITY_EDITOR
            SavePrefab(loadedModel, loadedModel.name);
#endif
*/
            return loadedModel;
        }
        else
        {
            Debug.LogError("glTFast load failed.");
            if (onModelLoaded != null)
            {
                onModelLoaded?.Invoke(null);
            }
            
        }
        return null;
    }

    // TODO: review how things work - benjamin
    // Save the model as a prefab at the given path
    public void SavePrefab(GameObject model, string filename)
    {
#if UNITY_EDITOR
        if (model != null)
        {
            // Ensure the prefab directory exists
            if (!Directory.Exists(unityEditorPrefabDirectory))
            {
                Directory.CreateDirectory(unityEditorPrefabDirectory);
            }
            string filePath = Path.Combine(unityEditorPrefabDirectory, $"{filename}.prefab");
            Debug.Log($"Prefab to be saved at: {filePath}");
            PrefabUtility.SaveAsPrefabAsset(model, filePath);
            Debug.Log($"Prefab saved to: {filePath}");
        }
        else
        {
            Debug.LogError("Cannot save prefab. Model is null.");
        }
#else
            //Debug.Log($"[{nameof(SF3DAPIClient)}] - {nameof(SavePrefab)} - does not work in outside UnityEditor");
#endif

    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }
}