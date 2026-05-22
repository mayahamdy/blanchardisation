using BioAdaptive;
using TMPro;
using UnityEngine;

namespace QuantumMiniGolf
{
    /// <summary>
    /// Per-hole countdown timer whose duration comes from StressAdapter.HoleTimerSeconds.
    /// The timer refreshes each time a new hole starts (call StartHole()) and fires
    /// OnTimeUp when it reaches zero — hook this to your GameManager's fail/next-hole logic.
    /// </summary>
    public class HoleTimer : MonoBehaviour
    {
        public static HoleTimer Instance { get; private set; }

        [Header("UI")]
        [Tooltip("Optional TMP label that shows the remaining seconds.")]
        public TextMeshProUGUI timerLabel;

        [Tooltip("Colour when time is running out (< 5 s).")]
        public Color urgentColor = new Color(1f, 0.3f, 0.2f);
        private Color _normalColor = Color.white;

        public System.Action OnTimeUp;

        public float Remaining  { get; private set; }
        public bool  IsRunning  { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(this); return; }
            Instance = this;
        }

        private void Start()
        {
            if (timerLabel != null) _normalColor = timerLabel.color;
        }

        private void Update()
        {
            if (!IsRunning) return;

            Remaining -= Time.deltaTime;

            if (timerLabel != null)
            {
                timerLabel.text  = Mathf.CeilToInt(Mathf.Max(0, Remaining)).ToString();
                timerLabel.color = Remaining < 5f ? urgentColor : _normalColor;
            }

            if (Remaining <= 0f)
            {
                IsRunning = false;
                Remaining = 0f;
                OnTimeUp?.Invoke();
            }
        }

        /// <summary>Call this when a new hole begins to start a fresh countdown.</summary>
        public void StartHole()
        {
            float duration = StressAdapter.Instance != null
                ? StressAdapter.Instance.HoleTimerSeconds
                : 30f;

            Remaining = duration;
            IsRunning = true;
            if (timerLabel != null) timerLabel.color = _normalColor;
        }

        public void StopTimer() => IsRunning = false;
    }
}
