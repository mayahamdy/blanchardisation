using TMPro;
using UnityEngine;

namespace QuantumMiniGolf
{
    /// <summary>
    /// Wii-Golf-style wind compass.
    /// - Arrow rotates to show the wind direction relative to what the camera is facing.
    ///   (arrow pointing right = wind pushing ball rightward from the player's view)
    /// - Speed text and arrow colour update with stress in real time.
    /// </summary>
    public class WindIndicatorUI : MonoBehaviour
    {
        [Header("References — wired automatically")]
        public RectTransform       arrowRect;   // the ▲ TMP that rotates
        public TextMeshProUGUI     arrowTMP;    // same object's TMP component
        public TextMeshProUGUI     speedText;
        public TextMeshProUGUI     titleText;

        // Colour thresholds matching the stress table
        private static readonly Color COL_CALM    = new Color(0.40f, 0.90f, 0.55f); // green
        private static readonly Color COL_MEDIUM  = new Color(1.00f, 0.80f, 0.20f); // yellow
        private static readonly Color COL_HIGH    = new Color(1.00f, 0.45f, 0.15f); // orange
        private static readonly Color COL_EXTREME = new Color(1.00f, 0.20f, 0.20f); // red

        private void Update()
        {
            if (WindController.Instance == null || StressAdapter.Instance == null) return;

            UpdateArrow();
            UpdateSpeed();
        }

        private void UpdateArrow()
        {
            Vector3 windDir = WindController.Instance.WindDirection;

            // Project wind onto the camera's local horizontal axes so the arrow
            // is relative to the player's current view — just like Wii Golf.
            Camera cam = Camera.main;
            if (cam == null) return;

            Vector3 camForward = cam.transform.forward; camForward.y = 0f; camForward.Normalize();
            Vector3 camRight   = cam.transform.right;   camRight.y   = 0f; camRight.Normalize();

            float dotForward = Vector3.Dot(windDir, camForward);
            float dotRight   = Vector3.Dot(windDir, camRight);

            // atan2 gives us the angle: 0 = forward (up on screen), 90 = right, etc.
            float angle = Mathf.Atan2(dotRight, dotForward) * Mathf.Rad2Deg;

            // In Unity UI positive Z-rotation is counter-clockwise, so negate.
            arrowRect.localEulerAngles = new Vector3(0f, 0f, -angle);
        }

        private void UpdateSpeed()
        {
            float speed = StressAdapter.Instance.WindForce;
            Color col   = SpeedToColor(speed);

            if (speed < 0.1f)
            {
                speedText.text  = "calme";
                speedText.color = COL_CALM;
                if (arrowTMP != null) arrowTMP.color = COL_CALM;
            }
            else
            {
                speedText.text  = $"{speed:F1} m/s";
                speedText.color = col;
                if (arrowTMP != null) arrowTMP.color = col;
            }
        }

        private static Color SpeedToColor(float speed)
        {
            // Maps StressAdapter.windMax (8) to the four stress zones.
            if (speed < 2.0f) return COL_CALM;
            if (speed < 4.5f) return COL_MEDIUM;
            if (speed < 6.5f) return COL_HIGH;
            return COL_EXTREME;
        }
    }
}
