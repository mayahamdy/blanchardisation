using Quantum;
using TurnBasedFramework;
using UnityEngine.UI;

namespace QuantumMiniGolf
{
    /// <summary>
    /// Displays the current turn timer text sourced from FairTimerVisuals.
    /// </summary>
    public class UITurnClock : QuantumSceneViewComponent
    {
        public Text TimerText;
        private FairTimerVisuals _fairTimerVisuals;

        /// <summary>
        /// Caches the FairTimerVisuals component from the same GameObject.
        /// </summary>
        public override void OnActivate(Frame frame)
        {
            _fairTimerVisuals = GetComponent<FairTimerVisuals>();
        }

        /// <summary>
        /// Refreshes the clock display each frame if the verified frame is available.
        /// </summary>
        public override void OnUpdateView()
        {
            Frame frame = Game.Frames.Verified;
            if (frame == null)
                return;

            UpdateTimer();
        }

        /// <summary>
        /// Copies the formatted timer string from FairTimerVisuals to the text component.
        /// </summary>
        private void UpdateTimer()
        {
            TimerText.text = _fairTimerVisuals.TimerText;
        }
    }
}