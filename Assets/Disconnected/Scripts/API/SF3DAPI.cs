using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using Siccity.GLTFUtility;
using UnityEditor;

public class SF3DAPI : MonoBehaviour
{
    [Header("Stable Fast 3D API Settings")] [Tooltip("API endpoint for Stable Fast 3D")]
    public string apiUrl = "https://api.stability.ai/v2beta/3d/stable-fast-3d"; // API URL for Stable Fast 3D

    [Tooltip("Your Stable Fast 3D API Key")]
    public string apiKey = "YOUR_API_KEY"; // Replace with your actual API key

    [Header("Paths for Image and Output")] [Tooltip("Path to the image to send to the API for 3D generation")]
    public string imagePath = "Assets/Textures/cat-statue.png"; // Path to your image (cat statue)

    [Tooltip("Directory where the GLB file should be saved")]
    public string outputDirectory = "Assets/Models/"; // Folder to save the generated model

    [Tooltip("The name for the generated GLB file")]
    public string outputFileName = "3d-cat-statue.glb"; // Name of the generated file

    [Header("Prefab Settings")] [Tooltip("Directory where prefabs will be saved")]
    public string prefabDirectory = "Assets/Prefabs/"; // Folder to save prefabs

    [Tooltip("Prefab name")] public string prefabName = "3d-cat-statue.prefab"; // Name of the prefab

    void Start()
    {
        // Start the coroutine to generate the 3D model when the scene starts
        StartCoroutine(Generate3DModel(imagePath));
    }

    // Generate the 3D model by sending an image to the API
    public IEnumerator Generate3DModel(string imagePath)
    {
        // Read image file as byte array
        byte[] imageBytes = File.ReadAllBytes(imagePath);

        // Create form data for the image
        WWWForm form = new WWWForm();
        form.AddBinaryData("image", imageBytes, Path.GetFileName(imagePath), "image/png");

        // Send image to the API to generate 3D model
        using (UnityWebRequest request = UnityWebRequest.Post(apiUrl, form))
        {
            // Add authorization header
            request.SetRequestHeader("Authorization", $"Bearer {apiKey}");
            request.downloadHandler = new DownloadHandlerBuffer(); // Handle the response as raw data

            // Wait for the request to complete
            yield return request.SendWebRequest();

            // Check if the request was successful
            if (request.result == UnityWebRequest.Result.Success)
            {
                byte[] responseData = request.downloadHandler.data;
                Debug.Log("3D model generated successfully!");

                // Save and load the model
                string modelFilePath = SaveModelToFile(responseData);
                GameObject loadedModel = LoadModel(modelFilePath);

                // Optionally, save this loaded model as a prefab
                SavePrefab(loadedModel);
            }
            else
            {
                Debug.LogError($"Error {request.responseCode}: {request.error}");
            }
        }
    }

    // Save the generated model to a file and return the file path
    string SaveModelToFile(byte[] modelData)
    {
        // Ensure the output directory exists
        if (!Directory.Exists(outputDirectory))
        {
            Directory.CreateDirectory(outputDirectory);
        }

        // Generate the full path to save the GLB file
        string outputPath = Path.Combine(outputDirectory, outputFileName);
        File.WriteAllBytes(outputPath, modelData); // Save the model to the specified path
        Debug.Log($"3D model saved to: {outputPath}");

        return outputPath; // Return the path to the saved model
    }

    // Load the generated model using GLTFUtility
    GameObject LoadModel(string modelPath)
    {
        if (System.IO.File.Exists(modelPath))
        {
            // Load the GLB model using GLTFUtility
            GameObject loadedModel = Importer.LoadFromFile(modelPath);
            loadedModel.transform.position = new Vector3(0, 0, 0); // Position the model at the origin
            // loadedModel.transform.SetParent(this.transform);  // Optionally parent it to this object
            Debug.Log("Model loaded into Unity.");
            return loadedModel;
        }
        else
        {
            Debug.LogError($"GLB file not found at: {modelPath}");
            return null;
        }
    }

    // Save the loaded model as a prefab for later use
    void SavePrefab(GameObject model)
    {
        if (model != null)
        {
            // Ensure the prefab directory exists
            if (!Directory.Exists(prefabDirectory))
            {
                Directory.CreateDirectory(prefabDirectory);
            }

            string prefabPath = Path.Combine(prefabDirectory, prefabName);
            PrefabUtility.SaveAsPrefabAsset(model, prefabPath); // Save the model as a prefab
            Debug.Log($"Prefab saved to: {prefabPath}");
        }
        else
        {
            Debug.LogError("Cannot save prefab. Model is null.");
        }
    }
}