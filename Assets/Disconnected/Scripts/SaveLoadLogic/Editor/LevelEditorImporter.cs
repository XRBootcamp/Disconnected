using System.Collections.Generic;
using System.IO;
using Disconnected.Scripts.Core;
using Disconnected.Scripts.DataSchema;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEngine;

    public class LevelEditorImporter
    {
        
       [MenuItem("Tools/Disconnected/Import Level From JSON...")]
    public static void ImportLevel()
    {
        string path = EditorUtility.OpenFilePanel("Select Level JSON", "", "json");
        if (string.IsNullOrEmpty(path)) return;

        string sourceDirectoryPath = Path.GetDirectoryName(path);
        string json = File.ReadAllText(path);
        LevelData levelData = JsonUtility.FromJson<LevelData>(json);

        if (levelData == null) return;
        
        GameObject levelContainer = new GameObject($"[IMPORTED] - {levelData.levelName}");
        Dictionary<string, GameObject> createdObjects = new Dictionary<string, GameObject>();

        // FIRST PASS: Instantiate all objects with real assets
        foreach (SceneObjectData objectData in levelData.objectsInScene)
        {
            GameObject newObject = null;

            if (string.IsNullOrEmpty(objectData.assetReferenceKey))
            {
                newObject = new GameObject(objectData.objectName);
            }
            else
            {
                GameObject prefabToInstantiate = FindAsset(objectData, sourceDirectoryPath);

                if (prefabToInstantiate != null)
                {
                    newObject = (GameObject)PrefabUtility.InstantiatePrefab(prefabToInstantiate);
                    newObject.name = objectData.objectName;
                }
                else
                {
                    newObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    newObject.name = objectData.objectName + " (Asset Not Found)";
                }
            }
            
            newObject.AddComponent<UniqueID>().SetGuid(objectData.guid);
            createdObjects.Add(objectData.guid, newObject);
        }

        // SECOND PASS: Set hierarchy and data
        foreach (SceneObjectData objectData in levelData.objectsInScene)
        {
            GameObject targetObject = createdObjects[objectData.guid];

            Transform parentTransform = levelContainer.transform;
            if (!string.IsNullOrEmpty(objectData.parentGuid) && createdObjects.ContainsKey(objectData.parentGuid))
            {
                parentTransform = createdObjects[objectData.parentGuid].transform;
            }
            targetObject.transform.SetParent(parentTransform, false);

            targetObject.transform.localPosition = objectData.transformData.position;
            targetObject.transform.localRotation = objectData.transformData.rotation;
            targetObject.transform.localScale = objectData.transformData.scale;

            if (objectData.hasAudioSource)
            {
                // TODO: Populate audioSource properties
            }
        }
        
        Selection.activeGameObject = levelContainer;
        EditorUtility.DisplayDialog("Import Complete", $"Successfully imported level '{levelData.levelName}'.", "OK");
    }

    private static GameObject FindAsset(SceneObjectData objectData, string sourceDirectoryPath)
    {
        // For backwards compatibility, if the key ends with .glb, always treat it as a LocalFile.
        if (objectData.assetReferenceKey.EndsWith(".glb", System.StringComparison.OrdinalIgnoreCase))
        {
            string targetAssetPath = "Assets/ImportedLevelAssets/" + objectData.assetReferenceKey;
                
            if (!File.Exists(targetAssetPath))
            {
                string sourceAssetPath = Path.Combine(sourceDirectoryPath, objectData.assetReferenceKey);
                if (File.Exists(sourceAssetPath))
                {
                    // --- START OF THE FIX ---
                    // Ensure the destination directory exists before copying.
                    string destinationDirectory = Path.GetDirectoryName(targetAssetPath);
                    if (!Directory.Exists(destinationDirectory))
                    {
                        Directory.CreateDirectory(destinationDirectory);
                    }
                    // --- END OF THE FIX ---
                    
                    FileUtil.CopyFileOrDirectory(sourceAssetPath, targetAssetPath);
                    AssetDatabase.Refresh();
                }
                else
                {
                    Debug.LogError($"Source asset file not found: {sourceAssetPath}");
                    return null;
                }
            }
            return AssetDatabase.LoadAssetAtPath<GameObject>(targetAssetPath);
        }
        
        // If it's not a .glb file, we assume it's an Addressable.
        if (objectData.assetSource == AssetSourceType.Addressable)
        {
             var settings = AddressableAssetSettingsDefaultObject.Settings;
            if (settings == null) return null;
            
            var entry = settings.FindAssetEntry(objectData.assetReferenceKey);
            if (entry != null)
            {
                return AssetDatabase.LoadAssetAtPath<GameObject>(entry.AssetPath);
            }
            Debug.LogError($"Addressable asset not found for key: {objectData.assetReferenceKey}");
            return null;
        }

        Debug.LogError($"Could not determine how to load asset with key: {objectData.assetReferenceKey}");
        return null;
    }
    }


