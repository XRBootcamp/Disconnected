using System;

namespace Disconnected.Scripts.DataSchema
{
    [Serializable]
    public class AudioSourceData
    {
        public bool isEnabled;

        /// <summary>
        /// Audio source.
        /// </summary>
        public string audioClipReference;

        public bool loop;
        public float volume;
        public float pitch;

        /// <summary>
        /// 2D - 3D Mix.
        /// </summary>
        public float spatialBlend;
    }
}