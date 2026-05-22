using UnityEngine;

namespace QuantumMiniGolf
{
    /// <summary>
    /// Triggers background music playback as soon as the scene starts, attaching the audio to the main camera.
    /// </summary>
    public sealed class BackgroundMusic : MonoBehaviour
    {
        /// <summary>
        /// Plays the background music clip at the origin, parented to the main camera so it follows the listener.
        /// </summary>
        private void Start()
        {
            AudioManager.Instance.Play("forest_bg", Vector3.zero, Camera.main.transform);
        }
    }
}
