using System;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using Siccity.GLTFUtility;
using UnityEditor;

public class SF3DAPIClient : IDisposable
{
    [Header("Stable Fast 3D API Settings")]
    private const string apiUrl = "https://api.stability.ai/v2beta/3d/stable-fast-3d";
    private string apiKey = "YOUR_API_KEY";

    public SF3DAPIClient(string newApiKey)
    {
        apiKey = newApiKey;
    }

    /// <summary>
    /// Generates a 3D model from an image using the Stable Fast 3D API.
    /// </summary>
    /// <param name="imagePath">Path to the input image file.</param>
    /// <param name="outputPath">Path to save the generated GLB model file.</param>
    /// <param name="prefabPath">Path to save the generated prefab.</param>
    /// <param name="parent">Parent transform for the loaded model.</param>
    public async Task<GameObject> Generate3DModelAsync(string imagePath, string outputPath, string prefabPath, Transform parent)
    {
        if (!File.Exists(imagePath))
        {
            Debug.LogError($"Image file not found at: {imagePath}");
            return null;
        }

        byte[] imageBytes = File.ReadAllBytes(imagePath);
        WWWForm form = new WWWForm();
        form.AddBinaryData("image", imageBytes, Path.GetFileName(imagePath), "image/png");

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
                SaveModelToFile(responseData, outputPath);
                GameObject loadedModel = LoadModel(outputPath, parent);
#if UNITY_EDITOR
                SavePrefab(loadedModel, prefabPath);
#endif
                return loadedModel;
            }
            else
            {
                Debug.LogError($"Error {request.responseCode}: {request.error}");
                return null;
            }
        }
    }

    // Save the model to a file
    private void SaveModelToFile(byte[] modelData, string outputPath)
    {
        File.WriteAllBytes(outputPath, modelData);
        Debug.Log($"3D model saved to: {outputPath}");
    }

    // Load the model using GLTFUtility, set parent and localPosition
    private GameObject LoadModel(string path, Transform parent)
    {
        if (File.Exists(path))
        {
            GameObject loadedModel = Importer.LoadFromFile(path);
            if (parent != null)
            {
                loadedModel.transform.SetParent(parent);
                // TODO: adjust position later on
                loadedModel.transform.localPosition = Vector3.zero;
            }
            // TODO: Add addressable classes
            Debug.Log("Model loaded into Unity.");
            return loadedModel;
        }
        else
        {
            Debug.LogError("GLB file not found at: " + path);
            return null;
        }
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }

    // TODO: review how things work - benjamin
#if UNITY_EDITOR
    // Save the model as a prefab at the given path
    private void SavePrefab(GameObject model, string prefabPath)
    {
        if (model != null)
        {
            PrefabUtility.SaveAsPrefabAsset(model, prefabPath);
            Debug.Log($"Prefab saved to: {prefabPath}");
        }
        else
        {
            Debug.LogError("Cannot save prefab. Model is null.");
        }
    }
#endif
}