using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic; 
using System.IO;
using Disconnected.Scripts.DataSchema; 

namespace Disconnected.Scripts.Core 
{
    public class CloudManager : MonoBehaviour
    {
        // --- Singleton Pattern ---
        public static CloudManager instance;

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
            }
            else
            {
                instance = this;
            }
        }

        private string uploadUrl = "https://uploadfile-c4piqdcjga-uc.a.run.app";

        /// <summary>
        /// Inicia el proceso de subida para un nivel completo.
        /// </summary>
        /// <param name="levelName">El nombre de la carpeta del nivel a subir.</param>
        public void UploadLevel(string levelName)
        {
            StartCoroutine(UploadLevelRoutine(levelName));
        }

        private IEnumerator UploadLevelRoutine(string levelName)
        {
            string levelFolderPath = Path.Combine(Application.persistentDataPath, "levels", levelName);
            string jsonPath = Path.Combine(levelFolderPath, "level.json");

            if (!File.Exists(jsonPath))
            {
                Debug.LogError($"[Cloud] Cannot find level.json for level '{levelName}' at path: {jsonPath}");
                yield break;
            }

xs            string jsonContent = File.ReadAllText(jsonPath);
            LevelData levelData = JsonUtility.FromJson<LevelData>(jsonContent);

            // Creamos una lista de todos los archivos que necesitamos subir
            List<string> filesToUpload = new List<string>();
            filesToUpload.Add("level.json"); // Siempre subimos el JSON

            foreach (SceneObjectData objectData in levelData.objectsInScene)
            {
                // Solo añadimos los assets que tienen una referencia y no están ya en la lista
                if (!string.IsNullOrEmpty(objectData.assetReferenceKey) && !filesToUpload.Contains(objectData.assetReferenceKey))
                {
                    filesToUpload.Add(objectData.assetReferenceKey);
                }
            }

            // --- 2. Subir cada archivo de la lista ---
            Debug.Log($"[Cloud] Starting upload for {filesToUpload.Count} files in level '{levelName}'.");
            foreach (string fileName in filesToUpload)
            {
                string localPath = Path.Combine(levelFolderPath, fileName);
                string cloudPath = $"levels/{levelName}/{fileName}";
                
                // Usamos 'yield return' para esperar a que cada subida termine antes de empezar la siguiente
                yield return StartCoroutine(UploadFileRoutine(localPath, cloudPath));
            }

            // --- 3. Notificar que todo el nivel está subido ---
            // TODO: Aquí llamarías a la Cloud Function 'createLevelMetadata' para crear la entrada en la base de datos.
            Debug.Log($"[Cloud] All files for level '{levelName}' have been uploaded successfully!");
        }

        private IEnumerator UploadFileRoutine(string localPath, string cloudPath)
        {
            if (!File.Exists(localPath))
            {
                Debug.LogError($"[Cloud] File to upload not found at: {localPath}");
                yield break;
            }

            byte[] fileData = File.ReadAllBytes(localPath);

            // --- INICIO DE LA LÓGICA CORREGIDA ---

            // 1. Creamos un formulario con múltiples partes.
            List<IMultipartFormSection> formData = new List<IMultipartFormSection>();
        
            // 2. Añadimos el archivo al formulario.
            // Le damos un "nombre de campo" ("file" en este caso), los datos, el nombre del archivo y su tipo.
            formData.Add(new MultipartFormFileSection("file", fileData, Path.GetFileName(localPath), "application/octet-stream"));

            // 3. Usamos UnityWebRequest.Post, que está diseñado para enviar este tipo de formularios.
            // Él se encargará de poner las cabeceras correctas (Content-Type: multipart/form-data).
            UnityWebRequest www = UnityWebRequest.Post(uploadUrl, formData);
        
            // 4. Todavía podemos añadir nuestra cabecera personalizada para decirle a la Cloud Function dónde guardarlo.
            www.SetRequestHeader("x-destination-path", cloudPath);

            // --- FIN DE LA LÓGICA CORREGIDA ---

            Debug.Log($"[Cloud] Uploading {localPath} to {cloudPath}...");
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"[Cloud] Error uploading file: {www.error} - {www.downloadHandler.text}");
            }
            else
            {
                Debug.Log($"[Cloud] File uploaded successfully: {www.downloadHandler.text}");
            }
        }
    }
}