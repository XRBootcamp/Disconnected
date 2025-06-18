using UnityEngine;
using System.IO; 
using System.Collections.Generic; 
using Disconnected.Scripts.DataSchema;

namespace Disconnected.Scripts.Core
{


public class SaveSystem : MonoBehaviour
{
    // A global, static instance for easy access from other scripts.
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
    public void SaveLevel(string levelName)
    {
        LevelData levelData = new LevelData();
        levelData.levelName = levelName;

        // Find all GameObjects with a UniqueID to ensure we only save objects that are part of the level.
        UniqueID[] sceneObjects = FindObjectsOfType<UniqueID>();

        foreach (UniqueID uniqueID in sceneObjects)
        {
            // TODO: Implement the logic to convert a GameObject to SceneObjectData.
            // This is our next step. We will populate the levelData.objectsInScene list here.
        }

        // For easier debugging.
        string json = JsonUtility.ToJson(levelData, true);

        string directoryPath = Path.Combine(Application.persistentDataPath, "levels", levelName);
        string filePath = Path.Combine(directoryPath, "level.json");

        Directory.CreateDirectory(directoryPath);
        File.WriteAllText(filePath, json);

        Debug.Log($"Level saved to: {filePath}");

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