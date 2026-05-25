using TMPro;
using UnityEngine;

namespace QuantumMiniGolf
{
    /// <summary>
    /// Counts up from zero when a hole starts and stops when the player wins.
    /// </summary>
    public class HoleTimer : MonoBehaviour
    {
        public static HoleTimer Instance { get; private set; }

        [Header("UI")]
        [Tooltip("TMP label that shows elapsed time in MM:SS format.")]
        public TextMeshProUGUI timerLabel;

        public float Elapsed   { get; private set; }
        public bool  IsRunning { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(this); return; }
            Instance = this;
        }

        private void Update()
        {
            if (!IsRunning) return;

            Elapsed += Time.deltaTime;

            if (timerLabel != null)
            {
                int mins = Mathf.FloorToInt(Elapsed / 60f);
                int secs = Mathf.FloorToInt(Elapsed % 60f);
                timerLabel.text = string.Format("{0:D2}:{1:D2}", mins, secs);
            }
        }

        /// <summary>Call this when a new hole begins.</summary>
        public void StartHole()
        {
            Elapsed    = 0f;
            IsRunning  = true;
        }

        public void StopTimer() => IsRunning = false;
    }
}
