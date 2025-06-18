using UnityEngine;
using System;

namespace Disconnected.Scripts.DataSchema
{
    [Serializable]
    public class TransformData
    {
        /// <summary>
        /// Local Position.
        /// </summary>
        public Vector3 position;

        /// <summary>
        /// Local Rotation.
        /// </summary>
        public Quaternion rotation;

        /// <summary>
        /// Local Scale.
        /// </summary>
        public Vector3 scale;
    }
}