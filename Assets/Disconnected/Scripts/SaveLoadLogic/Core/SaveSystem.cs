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
        [GUIColor(1f, 0.6f, 0.4f)]
        public void SaveLevelFromEditor(string levelName)
        {

            _ = SaveLevelAsync(levelName);
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
                string finalAssetFilename = "";

                switch (tracker.sourceType)
                {
                    case AssetSourceType.LocalFile:
                        finalAssetFilename = tracker.referenceKey;
                        // TODO: Logic to copy the file if needed.
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
                objectData.assetReferenceKey = "placeholder_cube.glb";
                Debug.LogWarning($"Object '{currentGO.name}' has no AssetSourceTracker. Saving as placeholder.");
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
        [Button]
        public async Task LoadLevelAsync(string levelName)
        {
            string directoryPath = Path.Combine(Application.persistentDataPath, "levels", levelName);
            string filePath = Path.Combine(directoryPath, "level.json");

            if (!File.Exists(filePath))
            {
                Debug.LogError($"Save file not found at: {filePath}");
                return;
            }

            string json = File.ReadAllText(filePath);
            LevelData levelData = JsonUtility.FromJson<LevelData>(json);

            // This dictionary helps us find GameObjects by their GUID quickly during the second pass.
            Dictionary<string, GameObject> createdObjects = new Dictionary<string, GameObject>();

            // --- FIRST PASS: Instantiate all objects from their correct sources ---
            foreach (SceneObjectData objectData in levelData.objectsInScene)
            {
                GameObject newObject = null;

                switch (objectData.assetSource)
                {
                    case AssetSourceType.LocalFile:
                        var gltf = new GLTFast.GltfImport();
                        string glbPath = Path.Combine(directoryPath, objectData.assetReferenceKey);
                        var success = await gltf.Load(glbPath);
                        if (success)
                        {
                            newObject = new GameObject(objectData.objectName);
                            await gltf.InstantiateMainSceneAsync(newObject.transform);
                        }
                        else
                        {
                            Debug.LogWarning($"Failed to load GLB at path: {glbPath}. Spawning placeholder.");
                        }

                        break;

                    case AssetSourceType.Addressable:
                        // TODO: Implement Addressables loading logic.
                        Debug.Log($"Placeholder for Addressable: {objectData.assetReferenceKey}");
                        break;
                }

                // If instantiation failed or was not implemented, spawn a placeholder cube.
                if (newObject == null)
                {
                    newObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    newObject.name = objectData.objectName;
                }

                newObject.AddComponent<UniqueID>().SetGuid(objectData.guid);
                createdObjects.Add(objectData.guid, newObject);
            }

            // --- SECOND PASS: Set hierarchy, transforms, and component data ---
            foreach (SceneObjectData objectData in levelData.objectsInScene)
            {
                if (!createdObjects.ContainsKey(objectData.guid)) continue;
                GameObject targetObject = createdObjects[objectData.guid];

                // Set Parent by finding it in our dictionary of created objects.
                if (!string.IsNullOrEmpty(objectData.parentGuid) && createdObjects.ContainsKey(objectData.parentGuid))
                {
                    targetObject.transform.SetParent(createdObjects[objectData.parentGuid].transform, false);
                }

                // Set Local Transform.
                targetObject.transform.localPosition = objectData.transformData.position;
                targetObject.transform.localRotation = objectData.transformData.rotation;
                targetObject.transform.localScale = objectData.transformData.scale;

                // Set Component Data.
                if (objectData.hasAudioSource)
                {
                    AudioSource audioSource = targetObject.AddComponent<AudioSource>();
                    // Logic to populate audioSource properties...
                }
            }

            Debug.Log($"Level '{levelName}' loaded successfully!");
        }
    }
}