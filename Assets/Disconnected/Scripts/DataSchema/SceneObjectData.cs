using System;

[Serializable]
public class SceneObjectData
{
    /// <summary>
    /// Id Identifiesr
    /// </summary>
    public string guid;

    /// <summary>
    /// Name in hierarchy
    /// </summary>
    public string objectName;

    /// <summary>
    ///  GUID  of the parent, if null will be a child of the level container.
    /// </summary>
    public string parentGuid;

    /// <summary>
    /// Base asset reference in cloud
    /// </summary>
    public string assetReference;

    /// <summary>
    /// Local Data transformation.
    /// </summary>
    public TransformData transformData;

    // --- Data Components ---
    
    public bool hasAudioSource;
    public AudioSourceData audioSourceData;

    // here we can add another components later
    // public bool hasLight;
    // public LightData lightData;
}