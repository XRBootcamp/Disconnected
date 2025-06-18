using UnityEngine;
using System.IO; 
using System.Collections.Generic;
using System.Threading.Tasks;
using Disconnected.Scripts.DataSchema;
using Sirenix.OdinInspector;

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

        /// <summary>
        /// Saves the current user-created scene to a persistent file.
        /// </summary>
        /// <param name="levelName">The name for the level, used as the folder name.</param>
        [Button]
        public void SaveLevel(string levelName)
        {
            LevelData levelData = new LevelData();
            levelData.levelName = levelName;

            // Find all GameObjects with a UniqueID to ensure we only save objects that are part of the level.
            UniqueID[] sceneObjects = FindObjectsOfType<UniqueID>();

            foreach (UniqueID uniqueID in sceneObjects)
            {
                SceneObjectData objectData = new SceneObjectData();

                // 1. Get basic data: GUID and name
                objectData.guid = uniqueID.GUID;
                objectData.objectName = uniqueID.gameObject.name;

                // 2. Get Parent GUID (this is crucial for hierarchy)
                Transform parent = uniqueID.transform.parent;
                if (parent != null && parent.TryGetComponent<UniqueID>(out UniqueID parentUniqueID))
                {
                    objectData.parentGuid = parentUniqueID.GUID;
                }
                else
                {
                    // If there's no parent, or the parent is not part of the level (lacks a UniqueID),
                    // it's a root object within the level.
                    objectData.parentGuid = string.Empty;
                }

                // 3. Get local transform data
                objectData.transformData = new TransformData
                {
                    position = uniqueID.transform.localPosition,
                    rotation = uniqueID.transform.localRotation,
                    scale = uniqueID.transform.localScale
                };

                // 4. Get asset reference (using placeholders for now)
                // TODO: This logic will later be more intelligent.
                objectData.assetSource = AssetSourceType.LocalFile; // Assuming it's a user-generated asset
                objectData.assetReferenceKey = "primitive_cube"; // Placeholder key

                // 5. Get component data (example with AudioSource)
                if (uniqueID.TryGetComponent<AudioSource>(out AudioSource audioSource))
                {
                    objectData.hasAudioSource = true;
                    objectData.audioSourceData = new AudioSourceData
                    {
                        isEnabled = audioSource.enabled,
                        loop = audioSource.loop,
                        volume = audioSource.volume,
                        pitch = audioSource.pitch,
                        spatialBlend = audioSource.spatialBlend,
                        // TODO: The audio clip reference will also be dynamic later
                        audioClipReference = "some_sound.wav"
                    };
                }
                else
                {
                    objectData.hasAudioSource = false;
                }

                // 6. Add the populated object data to our level list
                levelData.objectsInScene.Add(objectData);
            }

            // Easier debugging.
            string json = JsonUtility.ToJson(levelData, true);

            string directoryPath = Path.Combine(Application.persistentDataPath, "levels", levelName);
            string filePath = Path.Combine(directoryPath, "level.json");

            Directory.CreateDirectory(directoryPath);
            File.WriteAllText(filePath, json);

            Debug.Log($"Level saved to: {filePath}");
            Debug.Log($"Persistent Data Path is: {Application.persistentDataPath}");

            // TODO: Also save binary assets like .glb or .png files into the 'directoryPath'.
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