namespace Disconnected.Scripts.DataSchema
{
    /// <summary>
    /// Defines the source from which an asset should be loaded.
    /// </summary>
    public enum AssetSourceType
    {
        /// <summary>
        /// The asset should be loaded from the Addressables system using its key.
        /// This is for pre-made content provided by the developers.
        /// </summary>
        Addressable,

        /// <summary>
        /// The asset should be loaded from a local file path within the level's save folder.
        /// This is for user-generated content (e.g., GenAI models).
        /// </summary>
        LocalFile
    }
}