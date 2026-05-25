using BioAdaptive;
using UnityEngine;

namespace QuantumMiniGolf
{
    /// <summary>
    /// Accumulates per-hole gameplay and biometric stats.
    /// Call StartSession() on hole start and StopSession() on win.
    /// </summary>
    public class SessionStats : MonoBehaviour
    {
        public static SessionStats Instance { get; private set; }

        public int   StrokeCount   { get; private set; }
        public float AvgStress     { get; private set; }
        public float AvgHeartRate  { get; private set; }
        public float TimeCalm      { get; private set; }
        public float TimeMedium    { get; private set; }
        public float TimeHigh      { get; private set; }
        public float TimeExtreme   { get; private set; }

        private bool  _running;
        private float _sampleTimer;
        private float _stressSum;
        private float _hrSum;
        private int   _sampleCount;

        private const float SAMPLE_INTERVAL = 1f;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(this); return; }
            Instance = this;
        }

        public void StartSession()
        {
            StrokeCount  = 0;
            AvgStress    = 0f;
            AvgHeartRate = 0f;
            TimeCalm     = 0f;
            TimeMedium   = 0f;
            TimeHigh     = 0f;
            TimeExtreme  = 0f;
            _stressSum   = 0f;
            _hrSum       = 0f;
            _sampleCount = 0;
            _sampleTimer = 0f;
            _running     = true;
        }

        public void StopSession()
        {
            _running = false;
            if (_sampleCount > 0)
            {
                AvgStress    = _stressSum / _sampleCount;
                AvgHeartRate = _hrSum     / _sampleCount;
            }
        }

        public void AddStroke() => StrokeCount++;

        private void Update()
        {
            if (!_running) return;

            _sampleTimer += Time.deltaTime;
            if (_sampleTimer < SAMPLE_INTERVAL) return;
            _sampleTimer -= SAMPLE_INTERVAL;

            float stress = StressAdapter.Instance != null ? StressAdapter.Instance.Stress : 0f;
            float hr     = (BioBridge.Instance != null && BioBridge.Instance.LastData != null)
                           ? BioBridge.Instance.LastData.heart_rate : 0f;

            _stressSum   += stress;
            _hrSum       += hr;
            _sampleCount++;

            if      (stress < 0.25f) TimeCalm    += SAMPLE_INTERVAL;
            else if (stress < 0.50f) TimeMedium  += SAMPLE_INTERVAL;
            else if (stress < 0.75f) TimeHigh    += SAMPLE_INTERVAL;
            else                     TimeExtreme += SAMPLE_INTERVAL;
        }
    }
}
