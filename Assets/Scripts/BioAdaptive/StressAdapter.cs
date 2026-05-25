using BioAdaptive;
using UnityEngine;

namespace QuantumMiniGolf
{
    /// <summary>
    /// Central stress value hub for the game scene.
    /// Reads raw stress from BioBridge and smooths it with a Lerp over ~3-5 s
    /// so that environmental changes never feel abrupt.
    /// Other components read StressAdapter.Instance.Stress instead of BioBridge directly.
    /// </summary>
    public class StressAdapter : MonoBehaviour
    {
        public static StressAdapter Instance { get; private set; }

        [Tooltip("Seconds to reach the new stress level (the Lerp time constant).")]
        [Range(1f, 10f)]
        public float smoothingTime = 4f;

        /// <summary>Current smoothed stress value 0 (calm) → 1 (high stress).</summary>
        public float Stress { get; private set; }

        // ── Stress → gameplay parameter ranges ───────────────────────────────

        [Header("Wind force range")]
        public float windMin = 0f;
        public float windMax = 2f;

        [Header("Camera shake range")]
        public float shakeMin = 0f;
        public float shakeMax = 0.06f;

        [Header("Hole timer range (seconds)")]
        public float timerMax = 30f;
        public float timerMin = 10f;

        /// <summary>Wind magnitude, smoothed and mapped from Stress.</summary>
        public float WindForce   => Mathf.Lerp(windMin,  windMax,  Stress);

        /// <summary>Camera shake amplitude, mapped from Stress.</summary>
        public float ShakeAmount => Mathf.Lerp(shakeMin, shakeMax, Stress);

        /// <summary>Seconds allowed per hole, mapped from Stress.</summary>
        public float HoleTimerSeconds => Mathf.Lerp(timerMax, timerMin, Stress);

        // ── Unity lifecycle ──────────────────────────────────────────────────

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(this); return; }
            Instance = this;
        }

        private void Update()
        {
            if (BioBridge.Instance == null) return;

            float rawStress = BioBridge.Instance.LastData?.stress ?? 0f;
            // Smooth: move Stress towards rawStress by smoothingTime seconds.
            Stress = Mathf.Lerp(Stress, rawStress, Time.deltaTime / smoothingTime);
        }
    }
}
