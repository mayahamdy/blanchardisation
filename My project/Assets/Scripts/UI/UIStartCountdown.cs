using Quantum;
using UnityEngine;
using UnityEngine.UI;

namespace QuantumMiniGolf
{
    /// <summary>
    /// Shows a countdown panel at the start of each turn using the start countdown turn configuration.
    /// </summary>
    public class UIStartCountdown : QuantumSceneViewComponent
    {
        public GameObject Panel;
        public Text Countdown;

        private TurnConfig _startCountdownConfig;

        private bool _initialized;

        /// <summary>
        /// Checks whether the current turn is the start countdown, shows or hides the panel accordingly, and updates the countdown number.
        /// </summary>
        public override void OnUpdateView()
        {
            Frame frame = Game.Frames.Verified;
            if (frame == null)
                return;
            if (_startCountdownConfig == null || _startCountdownConfig.Guid == AssetGuid.Invalid)
            {
                _startCountdownConfig = ConfigAssetsHelper.GetStartCountdownConfig(frame);
            }

            TurnContainer turnContainer = frame.GetSingleton<TurnContainer>();
            if (_startCountdownConfig != null &&
                turnContainer.CurrentTurn.ConfigRef.Id == _startCountdownConfig.Guid)
            {
                if (_initialized == false)
                {
                    _initialized = true;
                    Panel.SetActive(true);
                }

                UpdateCountdown(frame);
            }
            else
            {
                Panel.SetActive(false);
            }
        }

        /// <summary>
        /// Reads the remaining ticks of the current turn timer and displays the ceiling value in whole seconds.
        /// </summary>
        private void UpdateCountdown(Frame frame)
        {
            TurnContainer turnContainer = frame.GetSingleton<TurnContainer>();
            float remainingTicks = turnContainer.CurrentTurn.Timer.RemainingFrames(frame);
            Countdown.text = Mathf.CeilToInt(remainingTicks / QuantumRunner.Default.Session.SimulationRate).ToString();
        }
    }
}