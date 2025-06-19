using Disconnected.Scripts.DataSchema;
using UnityEngine;

namespace Disconnected.Scripts.Core
{
    /// <summary>
    /// A simple data component that "tags" a GameObject with its asset file name.
    /// This allows the SaveSystem to know which file to save/load for this object.
    /// </summary>
    public class AssetSourceTracker : MonoBehaviour
    {
        public AssetSourceType sourceType;
        public string referenceKey;
    }
}