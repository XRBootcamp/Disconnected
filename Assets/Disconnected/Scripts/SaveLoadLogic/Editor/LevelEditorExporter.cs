using UnityEngine;
using UnityEditor;
using UnityEditor.AddressableAssets;
using System.IO;
using System.Collections.Generic;
using Disconnected.Scripts.DataSchema;
using Disconnected.Scripts.Core;
using GLTFast.Export;

namespace Disconnected.Scripts.Editor

{
    public class LevelEditorExporter
    {
        [MenuItem("Tools/Disconnected/Export Scene to JSON...")]
        public static async void ExportLevel() 
        {
            GameObject levelContainer = Selection.activeGameObject;
            if (levelContainer == null)
            {
                EditorUtility.DisplayDialog("Error",
                    "Please select the root container GameObject of the level you want to export.", "OK");
                return;
            }

            string path = EditorUtility.SaveFilePanel("Save Level JSON", "", $"{levelContainer.name}.json", "json");
            if (string.IsNullOrEmpty(path)) return;

            string destinationDirectory = Path.GetDirectoryName(path);

            Debug.Log($"Starting export for level '{levelContainer.name}' to {destinationDirectory}...");

            LevelData levelData = new LevelData();
            levelData.levelName = levelContainer.name;

            UniqueID[] sceneObjects = levelContainer.GetComponentsInChildren<UniqueID>();

            // Usamos un bucle for en lugar de foreach para poder usar await dentro
            for (int i = 0; i < sceneObjects.Length; i++)
            {
                UniqueID uniqueID = sceneObjects[i];
                SceneObjectData objectData = new SceneObjectData();
                GameObject currentGO = uniqueID.gameObject;

                // --- Recolectar todos los datos, igual que en SaveSystem ---
                objectData.guid = uniqueID.GUID;
                objectData.objectName = currentGO.name;

                Transform parent = currentGO.transform.parent;
                if (parent != null && parent.TryGetComponent<UniqueID>(out UniqueID parentUniqueID))
                {
                    objectData.parentGuid = parentUniqueID.GUID;
                }

                objectData.transformData = new TransformData
                {
                    position = currentGO.transform.localPosition,
                    rotation = currentGO.transform.localRotation,
                    scale = currentGO.transform.localScale
                };

                // --- Lógica de exportación de assets ---
                if (currentGO.TryGetComponent<AssetSourceTracker>(out AssetSourceTracker tracker))
                {
                    objectData.assetSource = tracker.sourceType;
                    string finalAssetFilename = "";

                    switch (tracker.sourceType)
                    {
                        case AssetSourceType.LocalFile:
                            // Para archivos locales, asumimos que el asset original está en el proyecto
                            finalAssetFilename = tracker.referenceKey;
                            // Aquí copiaríamos el asset desde la carpeta del proyecto a la carpeta de exportación
                            break;

                        case AssetSourceType.Addressable:
                            finalAssetFilename = $"{objectData.guid}.glb";
                            string exportPath = Path.Combine(destinationDirectory, finalAssetFilename);

                            var exportSettings = new ExportSettings { Format = GltfFormat.Binary };
                            var exporter = new GameObjectExport(exportSettings);
                            exporter.AddScene(new[] { currentGO });
                            bool success = await exporter.SaveToFileAndDispose(exportPath);

                            if (!success) Debug.LogError($"Failed to export {currentGO.name} to GLB.");
                            break;
                    }

                    objectData.assetReferenceKey = finalAssetFilename;
                }
                else
                {
                    objectData.assetReferenceKey = string.Empty;
                }

                // ... Lógica para AudioSource y otros componentes ...

                levelData.objectsInScene.Add(objectData);
            }

            // --- Serializar y escribir el archivo JSON ---
            string json = JsonUtility.ToJson(levelData, true);
            File.WriteAllText(path, json);

            EditorUtility.DisplayDialog("Export Complete", $"Successfully exported level to:\n{path}", "OK");
            Debug.Log($"Export complete. All files saved to: {destinationDirectory}");
        }
    }
}
    