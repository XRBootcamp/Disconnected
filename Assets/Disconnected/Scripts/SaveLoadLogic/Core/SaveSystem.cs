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
    /// Loads a level from a persistent file. To be implemented in Task 2.2.
    /// </summary>
    /// <param name="levelName">The name of the level to load.</param>
    public void LoadLevel(string levelName)
    {
        Debug.Log($"Attempting to load level: {levelName}...");
        // Loading logic will be implemented here.
    }
}
}