using System;
using System.IO;
using System.Threading.Tasks;
using Disconnected.Scripts.Utils;
using GLTFast;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

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
        form.AddBinaryData("image", imageBytes, Path.GetFileName(imagePath),
            $"image/{Path.GetExtension(imagePath).ToLower()}");

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

    // Load the generated model using GLTFast
    async Task<GameObject> LoadModel(string modelPath, Transform parentTransform,
        System.Action<GameObject> onModelLoaded)
    {
        var gltf = new GltfImport();
        var success = await gltf.Load("file://" + modelPath);

        if (success)
        {
            GameObject loadedModel = new GameObject(Path.GetFileNameWithoutExtension(modelPath));
            success = await gltf.InstantiateMainSceneAsync(loadedModel.transform);

            if (success)
            {
                Debug.Log("Model loaded into Unity.");

                // After loading, add necessary components for VR interactions
                InteractionComponentManager.AddInteractionComponents(loadedModel);

                // Save the loaded model as a prefab
                SavePrefab(loadedModel,
                    Path.GetFileNameWithoutExtension(modelPath)); // Save the prefab using the model's filename

                // Notify that the model is loaded and ready
                onModelLoaded?.Invoke(loadedModel);
            }
            else
            {
                Debug.LogError("GLTFast instantiation failed.");
                onModelLoaded?.Invoke(null);
            }
        }
        else
        {
            Debug.LogError("GLTFast load failed.");
            onModelLoaded?.Invoke(null);
        }

        return null;
    }

    public void SavePrefab(GameObject model, string filename)
    {
        if (model != null)
        {
            // Ensure the prefab directory exists
            if (!Directory.Exists(unityEditorPrefabDirectory))
            {
                Directory.CreateDirectory(unityEditorPrefabDirectory);
            }

            // Check if Unity is in Play Mode
            if (!EditorApplication.isPlaying)
            {
                // Save the prefab only when not in Play Mode
                string prefabPath = Path.Combine(unityEditorPrefabDirectory, $"{filename}.prefab");
                Debug.Log($"Prefab to be saved at: {prefabPath}");
                PrefabUtility.SaveAsPrefabAsset(model, prefabPath);
                Debug.Log($"Prefab saved to: {prefabPath}");
            }
            else
            {
                Debug.LogWarning("Cannot save prefab while in Play Mode.");
            }
        }
        else
        {
            Debug.LogError("Cannot save prefab. Model is null.");
        }

        // Debug.Log for non-UnityEditor environment (when not using the Unity Editor)
        Debug.Log($"[{nameof(SF3DAPIClient)}] - {nameof(SavePrefab)} - does not work outside UnityEditor.");
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }
}
