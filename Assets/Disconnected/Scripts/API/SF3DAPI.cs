using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

public class SF3DAPI : MonoBehaviour
{
    [Header("Stable Fast 3D API Settings")]
    public string apiUrl = "https://api.stability.ai/v2beta/3d/stable-fast-3d"; // The Stable Fast 3D endpoint
    public string apiKey = "YOUR_API_KEY";  // Replace with your actual API key
    public string imagePath = "Assets/Textures/cat-statue.png";  // Path to your image (cat statue)

    void Start()
    {
        // Start the coroutine to generate the 3D model
        StartCoroutine(Generate3DModel());
    }

    IEnumerator Generate3DModel()
    {
        // Create the form data to send the image
        byte[] imageBytes = File.ReadAllBytes(imagePath);  // Read image file as byte array
        WWWForm form = new WWWForm();
        form.AddBinaryData("image", imageBytes, Path.GetFileName(imagePath), "image/png");

        // Create the UnityWebRequest to send the image data to the API
        using (UnityWebRequest request = UnityWebRequest.Post(apiUrl, form))
        {
            // Add authorization header
            request.SetRequestHeader("Authorization", $"Bearer {apiKey}");
            request.downloadHandler = new DownloadHandlerBuffer();  // Handle the response as raw data

            // Wait for the request to complete
            yield return request.SendWebRequest();

            // Check if the request was successful
            if (request.result == UnityWebRequest.Result.Success)
            {
                byte[] responseData = request.downloadHandler.data;
                Debug.Log("3D model generated successfully!");

                // Process the returned data (3D model in GLB format)
                SaveModelToFile(responseData);
            }
            else
            {
                Debug.LogError($"Error {request.responseCode}: {request.error}");
            }
        }
    }

    // Save the model to a file
    void SaveModelToFile(byte[] modelData)
    {
        string outputPath = "Assets/Models/3d-cat-statue.glb";  // Path to save the generated model
        File.WriteAllBytes(outputPath, modelData);  // Write the response data to a GLB file
        Debug.Log($"3D model saved to: {outputPath}");

        // Optionally, you can load the model into Unity here
        // LoadModel(outputPath);
    }

    // Example function to load the model (using GLTF or another importer)
    void LoadModel(string path)
    {
        // You can implement a GLTF loader here or use a plugin to load .glb files.
        // This would involve importing a package like the GLTFUtility for Unity.
        Debug.Log("Model loaded into Unity.");
    }
}
