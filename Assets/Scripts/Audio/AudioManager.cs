using System;
using UnityEngine;

namespace QuantumMiniGolf
{
    /// <summary>
    /// Singleton manager that provides a centralised API for playing sounds by name using a pooled AudioSource.
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        // Instance is intentionally public so other components can access the singleton without a property wrapper.
        public static AudioManager Instance;
        public AudioSourcePool SourcePool;
        public Sound[] Sounds;

        /// <summary>
        /// Enforces the singleton pattern, destroying the GameObject if a duplicate instance is detected.
        /// </summary>
        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// Retrieves a sound by name, configures a pooled AudioSource with its settings, and plays it at the given world position.
        /// </summary>
        public void Play(string name, Vector3 position, Transform parent = null)
        {
            Sound s = Array.Find(Sounds, sound => sound.Name == name);

            AudioSource source = SourcePool.GetAudioSource();
            source.clip = s.Clip;
            source.volume = s.Volume;
            source.loop = s.Loop;
            source.transform.position = position;

            if (parent != null)
            {
                source.transform.parent = parent;
            }

            source.Play();
        }
    }
}
