using UnityEngine;
using System;

namespace Disconnected.Scripts.Core
{

    /// <summary>
    /// This component provides a unique, persistent identity to a GameObject.
    /// It is fundamental for the save/load system.
    /// </summary>
    public class UniqueID : MonoBehaviour
    {
        /// <summary>
        /// The Global Unique Identifier.
        /// </summary>
        [SerializeField] // Exposed in the Inspector for debugging, but private to prevent accidental changes.
        private string guid;

        // Public property to safely access the GUID from other scripts.
        public string GUID
        {
            get { return guid; }
        }

        /// <summary>
        /// Awake is called when the script instance is being loaded.
        /// We ensure that the object has a GUID assigned.
        /// </summary>
        private void Awake()
        {
            // If the GUID is null or empty (because this is a new object), assign a new one.
            // If this object already has a GUID (because it was loaded from a save file), we do nothing.
            if (string.IsNullOrEmpty(guid))
            {
                guid = Guid.NewGuid().ToString();
            }
        }

        /// <summary>
        /// Allows to set the GUID externally, for example, when loading a level from a save file.
        /// </summary>
        /// <param name="loadedGuid">The GUID read from the JSON file.</param>
        public void SetGuid(string loadedGuid)
        {
            if (!string.IsNullOrEmpty(loadedGuid))
            {
                guid = loadedGuid;
            }
        }
    }
}