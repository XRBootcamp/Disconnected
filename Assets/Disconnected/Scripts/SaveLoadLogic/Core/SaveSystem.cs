using UnityEngine;
using System.IO; 
using System.Collections.Generic;
using System.Threading.Tasks;
using Disconnected.Scripts.DataSchema;
using GLTFast;
using Sirenix.OdinInspector;
using GLTFast.Export;

namespace Disconnected.Scripts.Core
{


    public class SaveSystem : MonoBehaviour
    {
        public static SaveSystem instance;

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
        [Button]
        [GUIColor(0.5f, 1f, 0.5f)]
        private void OpenTempAssetFolder()
        {
            string path = Path.Combine(Application.persistentDataPath, "temp_genai_assets");
            Directory.CreateDirectory(path);
            #if UNITY_EDITOR
            System.Diagnostics.Process.Start(path);
            #endif
        }
        
        [Button]
        [GUIColor(1f, 0.6f, 0.4f)]
        public void SaveLevelFromEditor(string levelName)
        {

            _ = SaveLevelAsync(levelName);
        }
        
        [Button]
        [GUIColor(0.2f, 1f, 0.5f)] 
        public void UploadLevelToCloud(string levelName)
        {
            // Primero, verifica que el CloudManager exista en la escena.
            if (CloudManager.instance == null)
            {
                Debug.LogError("CloudManager instance not found in the scene. Cannot upload.");
                return;
            }

            // Llama al método público de subida del CloudManager.
            Debug.Log($"[Odin Button] Triggering cloud upload for level: {levelName}");
            CloudManager.instance.UploadLevel(levelName);
        }
        


    /// <summary>
    /// Asynchronously saves the current user-created scene to a persistent file.
    /// Exports all referenced assets into the level's folder.
    /// </summary>
    public async Task SaveLevelAsync(string levelName)
    {
        LevelData levelData = new LevelData();
        levelData.levelName = levelName;

        string directoryPath = Path.Combine(Application.persistentDataPath, "levels", levelName);
        Directory.CreateDirectory(directoryPath);

        UniqueID[] sceneObjects = FindObjectsOfType<UniqueID>();

        foreach (UniqueID uniqueID in sceneObjects)
        {
            SceneObjectData objectData = new SceneObjectData();
            GameObject currentGO = uniqueID.gameObject;

            // --- 1. Get basic data (GUID, name) ---
            objectData.guid = uniqueID.GUID;
            objectData.objectName = currentGO.name;

            // --- 2. Get Parent GUID (Hierarchy) ---
            Transform parent = currentGO.transform.parent;
            if (parent != null && parent.TryGetComponent<UniqueID>(out UniqueID parentUniqueID))
            {
                objectData.parentGuid = parentUniqueID.GUID;
            }

            // --- 3. Get local transform data ---
            objectData.transformData = new TransformData
            {
                position = currentGO.transform.localPosition,
                rotation = currentGO.transform.localRotation,
                scale = currentGO.transform.localScale
            };

            // --- 4. Handle Asset Reference (The intelligent logic) ---
            if (currentGO.TryGetComponent<AssetSourceTracker>(out AssetSourceTracker tracker))
            {
                objectData.assetSource = tracker.sourceType;

                Debug.Log($"[SAVE DEBUG] Guardando objeto '{currentGO.name}'. Su AssetSourceTracker tiene el tipo: {tracker.sourceType}");
                string finalAssetFilename = "";

                switch (tracker.sourceType)
                {
                    case AssetSourceType.LocalFile:
                        // The asset is already a local file (e.g., from GenAI).
                        finalAssetFilename = tracker.referenceKey;
                        
                        // We must copy the file from its source to the level's save folder
                        // to make the level self-contained.
                        string tempAssetFolder = Path.Combine(Application.persistentDataPath, "temp_genai_assets");
                        string sourcePath = Path.Combine(tempAssetFolder, finalAssetFilename);
                        string destinationPath = Path.Combine(directoryPath, finalAssetFilename);

                        if (File.Exists(sourcePath))
                        {
                            File.Copy(sourcePath, destinationPath, true); // 'true' allows overwriting
                        }
                        else
                        {
                            Debug.LogWarning($"Source asset file not found at {sourcePath}. It will not be included in the save.");
                        }
                        break;

                    case AssetSourceType.Addressable:
                        finalAssetFilename = $"{objectData.guid}.glb";
                        string exportPath = Path.Combine(directoryPath, finalAssetFilename);


                        var exportSettings = new ExportSettings { Format = GltfFormat.Binary }; 
                        var exporter = new GameObjectExport(exportSettings);
                        var success = await exporter.SaveToFileAndDispose(exportPath);

                        if (!success) Debug.LogError($"Failed to export {currentGO.name} to GLB.");
                        break;
                }
                objectData.assetReferenceKey = finalAssetFilename;
            }
            else
            {
                objectData.assetReferenceKey = string.Empty;
                Debug.Log($"Object '{currentGO.name}' has no AssetSourceTracker. Saving as an empty container.");
            }

            // --- 5. Get component data (example with AudioSource) ---
            if (currentGO.TryGetComponent<AudioSource>(out AudioSource audioSource))
            {
                objectData.hasAudioSource = true;
                objectData.audioSourceData = new AudioSourceData
                {
                    isEnabled = audioSource.enabled,
                    loop = audioSource.loop,
                    volume = audioSource.volume,
                    pitch = audioSource.pitch,
                    spatialBlend = audioSource.spatialBlend,
                    // TODO: The audio clip reference also needs a tracker system.
                    audioClipReference = "some_sound.wav"
                };
            }
            else
            {
                objectData.hasAudioSource = false;
            }

            // --- 6. Add the populated object data to our level list ---
            levelData.objectsInScene.Add(objectData);
        }

        // --- Finalize and Write JSON ---
        string filePath = Path.Combine(directoryPath, "level.json");
        string json = JsonUtility.ToJson(levelData, true);
        await File.WriteAllTextAsync(filePath, json);

        Debug.Log($"Level '{levelName}' saved successfully to: {directoryPath}");
    }
    
    
    /// <summary>
    /// Asynchronously loads a level from a persistent file and reconstructs the scene.
    /// </summary>
    public async Task LoadLevelAsync(string levelName)
    {
        string directoryPath = Path.Combine(Application.persistentDataPath, "levels", levelName);
        string filePath = Path.Combine(directoryPath, "level.json");

        if (!File.Exists(filePath))
        {
            Debug.LogError($"[Load] Save file not found at: {filePath}");
            return;
        }

        string json = await File.ReadAllTextAsync(filePath);
        LevelData levelData = JsonUtility.FromJson<LevelData>(json);

        Dictionary<string, GameObject> createdObjects = new Dictionary<string, GameObject>();

        Debug.Log($"[Load] Starting to load {levelData.objectsInScene.Count} objects...");

        // --- FIRST PASS: Instantiate all objects ---
        foreach (SceneObjectData objectData in levelData.objectsInScene)
        {
            GameObject newObject = null;


            if (string.IsNullOrEmpty(objectData.assetReferenceKey))
            {
                newObject = new GameObject(objectData.objectName);
            }
            else
            {
                if (objectData.assetSource == AssetSourceType.LocalFile)
                {
                    string assetPath = Path.Combine(directoryPath, objectData.assetReferenceKey);
                
                    if (File.Exists(assetPath))
                    {
                        var gltf = new GltfImport();
                        var success = await gltf.Load("file://" + assetPath);

                        if (success)
                        {
                            newObject = new GameObject(objectData.objectName);
                            await gltf.InstantiateMainSceneAsync(newObject.transform);
                        }
                    }
                }
                // TODO:"case AssetSourceType.Addressable:" en the future.
            }

            if (newObject == null) 
            {
                Debug.LogWarning($"[Load] newObject is null for '{objectData.objectName}'. Spawning fallback cube.");
                newObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
                newObject.name = objectData.objectName + " (Fallback)";
            }
            
            newObject.AddComponent<UniqueID>().SetGuid(objectData.guid);
            createdObjects.Add(objectData.guid, newObject);
        }

        // --- SECOND PASS: Set hierarchy and data ---
        foreach (SceneObjectData objectData in levelData.objectsInScene)
        {
            if (!createdObjects.ContainsKey(objectData.guid)) continue;
            GameObject targetObject = createdObjects[objectData.guid];

            if (!string.IsNullOrEmpty(objectData.parentGuid) && createdObjects.ContainsKey(objectData.parentGuid))
            {
                targetObject.transform.SetParent(createdObjects[objectData.parentGuid].transform, false);
            }

            targetObject.transform.localPosition = objectData.transformData.position;
            targetObject.transform.localRotation = objectData.transformData.rotation;
            targetObject.transform.localScale = objectData.transformData.scale;

            if (objectData.hasAudioSource)
            {
                AudioSource audioSource = targetObject.AddComponent<AudioSource>();
                // Logic to populate audioSource properties...
            }
        }
        
        Debug.Log($"[Load] Level '{levelName}' loading process finished!");
    }
    }
}