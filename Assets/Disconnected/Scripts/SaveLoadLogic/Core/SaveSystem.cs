using UnityEngine;
using System.IO; 
using System.Collections.Generic; 
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
    /// Loads a level from a persistent file and reconstructs the scene.
    /// </summary>
    /// <param name="levelName">The name of the level to load.</param>
    [Button]
    public void LoadLevel(string levelName)
    {
        // 1. Define file path and check if the save file exists.
        string directoryPath = Path.Combine(Application.persistentDataPath, "levels", levelName);
        string filePath = Path.Combine(directoryPath, "level.json");

        if (!File.Exists(filePath))
        {
            Debug.LogError($"Save file not found at: {filePath}");
            return;
        }

        // 2. Read the JSON file and deserialize it into our LevelData object.
        string json = File.ReadAllText(filePath);
        LevelData levelData = JsonUtility.FromJson<LevelData>(json);

        // TODO: Before loading, you might want to clear the current scene of any previously loaded level objects.

        // This dictionary will help us find GameObjects by their GUID quickly during the second pass.
        Dictionary<string, GameObject> createdObjects = new Dictionary<string, GameObject>();

        // --- FIRST PASS: Instantiate all objects ---
        foreach (SceneObjectData objectData in levelData.objectsInScene)
        {

            GameObject prefabToInstantiate = GameObject.CreatePrimitive(PrimitiveType.Cube);
            GameObject newObject = Instantiate(prefabToInstantiate);
            Destroy(prefabToInstantiate); // Destroy the temporary primitive template

            newObject.name = objectData.objectName;
            
            // Add and set the UniqueID component.
            UniqueID uniqueID = newObject.AddComponent<UniqueID>();
            uniqueID.SetGuid(objectData.guid);

            // Store the newly created object in our dictionary for the second pass.
            createdObjects.Add(objectData.guid, newObject);
        }

        // --- SECOND PASS: Set hierarchy, transforms, and component data ---
        foreach (SceneObjectData objectData in levelData.objectsInScene)
        {
            // Find the object we created in the first pass.
            GameObject targetObject = createdObjects[objectData.guid];

            // 1. Set Parent
            if (!string.IsNullOrEmpty(objectData.parentGuid))
            {
                // Find the parent GameObject from our dictionary and set it.
                GameObject parentObject = createdObjects[objectData.parentGuid];
                targetObject.transform.SetParent(parentObject.transform, false); // 'false' to keep local transform
            }

            // 2. Set Local Transform
            targetObject.transform.localPosition = objectData.transformData.position;
            targetObject.transform.localRotation = objectData.transformData.rotation;
            targetObject.transform.localScale = objectData.transformData.scale;
            
            // 3. Set Component Data (example with AudioSource)
            if (objectData.hasAudioSource)
            {
                AudioSource audioSource = targetObject.AddComponent<AudioSource>();
                AudioSourceData data = objectData.audioSourceData;

                audioSource.enabled = data.isEnabled;
                audioSource.loop = data.loop;
                audioSource.volume = data.volume;
                audioSource.pitch = data.pitch;
                audioSource.spatialBlend = data.spatialBlend;
                
                // TODO: Here we would load the actual AudioClip using data.audioClipReference
            }
        }
        
        Debug.Log($"Level '{levelName}' loaded successfully!");
    }
}
}