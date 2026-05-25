using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace QuantumMiniGolf
{
    /// <summary>
    /// End-of-hole results panel. Attach to the ResultsPanel GameObject.
    /// Call Show() from LocalGameManager when the player wins.
    /// </summary>
    public class ResultsScreenUI : MonoBehaviour
    {
        [Header("Stats labels")]
        public TextMeshProUGUI timeText;
        public TextMeshProUGUI strokeText;
        public TextMeshProUGUI hrText;

        [Header("Average stress bar")]
        public Image avgStressFill;

        [Header("Zone fills")]
        public Image calmFill;
        public Image mediumFill;
        public Image highFill;
        public Image extremeFill;

        [Header("Zone time labels")]
        public TextMeshProUGUI calmTimeText;
        public TextMeshProUGUI mediumTimeText;
        public TextMeshProUGUI highTimeText;
        public TextMeshProUGUI extremeTimeText;

        [Header("Replay")]
        public Button replayButton;

        private static readonly Color COL_CALM    = new Color(0.40f, 0.90f, 0.55f);
        private static readonly Color COL_MEDIUM  = new Color(1.00f, 0.80f, 0.20f);
        private static readonly Color COL_HIGH    = new Color(1.00f, 0.45f, 0.15f);
        private static readonly Color COL_EXTREME = new Color(1.00f, 0.20f, 0.20f);

        private void OnEnable()
        {
            if (replayButton != null)
                replayButton.onClick.AddListener(OnReplayClicked);
        }

        private void OnDisable()
        {
            if (replayButton != null)
                replayButton.onClick.RemoveListener(OnReplayClicked);
        }

        public void Show()
        {
            gameObject.SetActive(true);
            Populate();
        }

        private void Populate()
        {
            SessionStats s = SessionStats.Instance;

            // Finish time from HoleTimer
            if (timeText != null)
            {
                float elapsed = HoleTimer.Instance != null ? HoleTimer.Instance.Elapsed : 0f;
                int mins = Mathf.FloorToInt(elapsed / 60f);
                int secs = Mathf.FloorToInt(elapsed % 60f);
                timeText.text = string.Format("{0:D2}:{1:D2}", mins, secs);
            }

            if (s == null) return;

            if (strokeText != null)
                strokeText.text = s.StrokeCount.ToString();

            if (hrText != null)
                hrText.text = s.AvgHeartRate > 1f
                    ? string.Format("{0:F0} bpm", s.AvgHeartRate)
                    : "--";

            if (avgStressFill != null)
            {
                avgStressFill.fillAmount = s.AvgStress;
                avgStressFill.color      = StressToColor(s.AvgStress);
            }

            float total = s.TimeCalm + s.TimeMedium + s.TimeHigh + s.TimeExtreme;
            if (total < 1f) total = 1f;

            SetZone(calmFill,    calmTimeText,    s.TimeCalm,    total, COL_CALM);
            SetZone(mediumFill,  mediumTimeText,  s.TimeMedium,  total, COL_MEDIUM);
            SetZone(highFill,    highTimeText,    s.TimeHigh,    total, COL_HIGH);
            SetZone(extremeFill, extremeTimeText, s.TimeExtreme, total, COL_EXTREME);
        }

        private void SetZone(Image fill, TextMeshProUGUI label, float seconds, float total, Color col)
        {
            if (fill != null)
            {
                fill.fillAmount = seconds / total;
                fill.color      = col;
            }
            if (label != null)
                label.text = Mathf.RoundToInt(seconds) + "s";
        }

        private Color StressToColor(float s)
        {
            if (s < 0.25f) return COL_CALM;
            if (s < 0.50f) return COL_MEDIUM;
            if (s < 0.75f) return COL_HIGH;
            return COL_EXTREME;
        }

        private void OnReplayClicked()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
}
