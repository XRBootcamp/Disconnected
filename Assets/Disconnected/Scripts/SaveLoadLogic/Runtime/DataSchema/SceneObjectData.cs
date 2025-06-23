using System;

namespace Disconnected.Scripts.DataSchema 
{
    [Serializable]
    public class SceneObjectData
    {
        /// <summary>
        /// The Global Unique Identifier for this object.
        /// </summary>
        public string guid;

        /// <summary>
        /// The name of the object in the hierarchy.
        /// </summary>
        public string objectName;

        /// <summary>
        /// The GUID of the parent object. If null or empty, it's a direct child of the [LevelContainer].
        /// </summary>
        public string parentGuid;

        // --- ASSET REFERENCE ---
    
        /// <summary>
        /// Defines where to load the asset from (e.g., Addressables system or a local file).
        /// </summary>
        public AssetSourceType assetSource;

        /// <summary>
        /// The key or path for the asset. 
        /// If assetSource is Addressable, this is the Addressable Key.
        /// If assetSource is LocalFile, this is the relative file path (e.g., "my_model.glb").
        /// </summary>
        public string assetReferenceKey;

        /// <summary>
        /// The local transform data for the object.
        /// </summary>
        public TransformData transformData;

        // --- COMPONENT DATA ---
        // We use a boolean to know if we should read this component's data on load.
    
        public bool hasAudioSource;
        public AudioSourceData audioSourceData;

        // We can add more component data here in the future.
        // public bool hasLight;
        // public LightData lightData;
    }
}