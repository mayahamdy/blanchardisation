using UnityEngine;

namespace QuantumMiniGolf
{
    /// <summary>
    /// Adds Perlin-noise camera shake proportional to StressAdapter.ShakeAmount.
    /// Attach to the Main Camera. Works alongside LocalCameraController:
    /// the shake is applied on top of whatever position the follow camera computes.
    /// </summary>
    public class AdaptiveCameraShake : MonoBehaviour
    {
        [Tooltip("How fast the noise cycles (higher = more jittery).")]
        public float noiseSpeed = 8f;

        // Offsets so the x/y noise samples are independent.
        private float _seedX;
        private float _seedY;

        private void Start()
        {
            _seedX = Random.Range(0f, 100f);
            _seedY = Random.Range(0f, 100f);
        }

        private void LateUpdate()
        {
            if (StressAdapter.Instance == null) return;

            float amp = StressAdapter.Instance.ShakeAmount;
            if (amp < 0.001f) return;

            float t  = Time.time * noiseSpeed;
            float dx = (Mathf.PerlinNoise(_seedX + t, 0f) - 0.5f) * 2f * amp;
            float dy = (Mathf.PerlinNoise(_seedY + t, 0f) - 0.5f) * 2f * amp;

            transform.position += new Vector3(dx, dy, 0f);
        }
    }
}
