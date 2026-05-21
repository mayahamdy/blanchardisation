using UnityEngine;

namespace QuantumMiniGolf
{
    /// <summary>
    /// Data container holding the configuration for a single named sound, including its clip, volume, and loop settings.
    /// </summary>
    [System.Serializable]
    public class Sound
    {
        public string Name;
        public AudioClip Clip;

        [Range(0f, 1f)] public float Volume;
        public bool Loop;
    }
}
