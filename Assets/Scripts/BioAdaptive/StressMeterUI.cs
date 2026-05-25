using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace QuantumMiniGolf
{
    /// <summary>
    /// Horizontal fill bar that shows the current smoothed stress level.
    /// </summary>
    public class StressMeterUI : MonoBehaviour
    {
        public Image              fillBar;
        public TextMeshProUGUI    label;

        private static readonly Color COL_LOW  = new Color(0.40f, 0.90f, 0.55f);
        private static readonly Color COL_MED  = new Color(1.00f, 0.80f, 0.20f);
        private static readonly Color COL_HIGH = new Color(1.00f, 0.45f, 0.15f);
        private static readonly Color COL_MAX  = new Color(1.00f, 0.20f, 0.20f);

        private void Update()
        {
            if (StressAdapter.Instance == null) return;

            float stress = StressAdapter.Instance.Stress;

            if (fillBar != null)
            {
                fillBar.fillAmount = stress;
                fillBar.color      = StressToColor(stress);
            }
        }

        private static Color StressToColor(float s)
        {
            if (s < 0.25f) return COL_LOW;
            if (s < 0.50f) return COL_MED;
            if (s < 0.75f) return COL_HIGH;
            return COL_MAX;
        }
    }
}
