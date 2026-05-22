using System.Collections.Generic;
using UnityEngine;

namespace QuantumMiniGolf
{
    /// <summary>
    /// Manages a pool of AudioSource components, recycling inactive sources and expanding the pool on demand to avoid runtime allocations.
    /// </summary>
    public class AudioSourcePool : MonoBehaviour
    {
        [SerializeField] private GameObject _audioSourcePrefab;

        private List<AudioSource> _active;
        private List<AudioSource> _inactive;

        /// <summary>
        /// Initialises the active and inactive lists and pre-warms the pool with one AudioSource.
        /// </summary>
        private void Awake()
        {
            _active = new List<AudioSource>();
            _inactive = new List<AudioSource>();
            SpawnSource(1);
        }

        /// <summary>
        /// Instantiates the given number of AudioSource prefabs, parents them to this transform, and adds them to the inactive list.
        /// </summary>
        private void SpawnSource(int amount)
        {
            for (int i = 0; i < amount; i++)
            {
                AudioSource source = Instantiate(_audioSourcePrefab).GetComponent<AudioSource>();
                source.transform.parent = transform;
                _inactive.Add(source);
            }
        }

        /// <summary>
        /// Each frame, moves any AudioSources that have finished playing from the active list back to the inactive list.
        /// </summary>
        private void Update()
        {
            for (int i = 0; i < _active.Count; i++)
            {
                AudioSource source = _active[i];
                if (!source.isPlaying)
                {
                    _inactive.Add(source);
                    _active.Remove(source);
                }
            }
        }

        /// <summary>
        /// Returns an available AudioSource from the pool, expanding the pool by three if none are currently inactive.
        /// </summary>
        public AudioSource GetAudioSource()
        {
            if (_inactive.Count == 0)
            {
                SpawnSource(3);
            }

            AudioSource source = _inactive[_inactive.Count - 1];
            _inactive.RemoveAt(_inactive.Count - 1);
            _active.Add(source);
            return source;
        }
    }
}