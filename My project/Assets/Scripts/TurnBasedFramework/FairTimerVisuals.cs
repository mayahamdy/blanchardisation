using UnityEngine;
using Quantum;

namespace TurnBasedFramework
{
    /// <summary>
    /// Reads from FairTimer and exposes formatted time string and fill percentage for UI components to consume.
    /// </summary>
    public enum RoundingMethod
    {
        None,
        FloorToInt,
        CeilToInt,
        RoundToInt,
    }

    [RequireComponent(typeof(FairTimer))]
    public class FairTimerVisuals : QuantumSceneViewComponent
    {
        public RoundingMethod RoundingMethod;
        public string TimerText;
        public float TimerPercentage;

        private FairTimer _fairTimer;

        /// <summary>
        /// Caches the FairTimer component from the same GameObject.
        /// </summary>
        public override void OnActivate(Frame frame)
        {
            _fairTimer = GetComponent<FairTimer>();
        }

        /// <summary>
        /// Updates the timer text and fill percentage each frame.
        /// </summary>
        public override void OnUpdateView()
        {
            UpdateText();
            UpdateImageTimerPercentage();
        }

        /// <summary>
        /// Calculates how far through the turn the timer is and exposes it as a 0-1 fill percentage.
        /// </summary>
        private void UpdateImageTimerPercentage()
        {
            Frame frame = Game.Frames.Verified;
            float timeInSec = _fairTimer.CorrectedTime;

            int simulationRate = QuantumDeterministicSessionConfigAsset.Global.Config.UpdateFPS;
            TurnContainer turnContainer = frame.GetSingleton<TurnContainer>();
            TurnConfig turnConfig = frame.FindAsset<TurnConfig>(turnContainer.CurrentTurn.ConfigRef.Id);
            if (turnConfig != null)
            {
                float turnDurationInSeconds = turnConfig.TurnDurationInTicks / simulationRate;
                TimerPercentage = (float)(turnDurationInSeconds - timeInSec) / turnDurationInSeconds;
            }
        }

        /// <summary>
        /// Formats the corrected remaining time as a MM:SS string, applying the configured rounding method.
        /// </summary>
        private void UpdateText()
        {
            float timeInSec = _fairTimer.CorrectedTime;

            switch (RoundingMethod)
            {
                case RoundingMethod.FloorToInt:
                    timeInSec = Mathf.FloorToInt(timeInSec);
                    break;
                case RoundingMethod.CeilToInt:
                    timeInSec = Mathf.CeilToInt(timeInSec);
                    break;
                case RoundingMethod.RoundToInt:
                    timeInSec = Mathf.RoundToInt(timeInSec);
                    break;
            }

            int min = (int)timeInSec / 60;
            int sec = (int)timeInSec % 60;

            TimerText = string.Format(sec < 10 ? "{0}:0{1}" : "{0}:{1}", min.ToString(), sec.ToString());
        }
    }
}