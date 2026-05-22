using Photon.Deterministic;
using Quantum;
using UnityEngine;

namespace TurnBasedFramework
{
    /// <summary>
    /// Computes a latency-corrected turn timer by accounting for round-trip time and input offset.
    /// </summary>
    public class FairTimer : QuantumSceneViewComponent
    {
        public bool ConsiderHalfRtt;
        public bool ConsiderInputOffset;

        [Header("TimerCorrection")] public float Offset;
        // higher factor makes convergence to real time faster
        public float InterpolationFactor = 0.3f; 
        public float InterpolationMin = 0.001f;
        public float InterpolationMax = 0.01f;

        private float _timerCorrection;

        public float RealTime { get; private set; }
        public float CorrectedTime { get; private set; }

        /// <summary>
        /// Subscribes to the TurnTimerReset Quantum event to recalculate latency correction when a new turn begins.
        /// </summary>
        private void Start()
        {
            QuantumEvent.Subscribe<EventTurnTimerReset>(this, OnTurnTimerReset);
        }

        /// <summary>
        /// Recalculates the timer correction offset whenever a new active play turn starts.
        /// </summary>
        private void OnTurnTimerReset(EventTurnTimerReset eventInfo)
        {
            Frame frame = eventInfo.Game.Frames.Verified;
            TurnConfig config = frame.FindAsset<TurnConfig>(eventInfo.Turn.ConfigRef.Id);

            if (config == null || config.UsesTimer == false || eventInfo.Turn.Status != ETurnStatus.Active)
                return;

            _timerCorrection = CalculateCorrection(eventInfo.Game);
        }

        /// <summary>
        /// Reads the current turn's remaining ticks and computes both the raw and latency-corrected time values each frame.
        /// </summary>
        public override void OnUpdateView()
        {
            Frame frame = Game.Frames.Verified;
            
            TurnContainer turnContainer = frame.GetSingleton<TurnContainer>();
            TurnData currentTurn = turnContainer.CurrentTurn;
            TurnConfig config = frame.FindAsset<TurnConfig>(currentTurn.ConfigRef.Id);
            if (config == null || config.UsesTimer == false || currentTurn.Status != ETurnStatus.Active)
                return;

            float realTime, correctedTime;
            TurnConfig globalTurnConfig = frame.FindAsset<TurnConfig>(turnContainer.CurrentTurn.ConfigRef.Id);
            UpdateFairTimer(globalTurnConfig.TurnDurationInTicks,
                turnContainer.CurrentTurn.Timer.RemainingFrames(frame),
                out realTime, out correctedTime);

            RealTime = realTime;
            CorrectedTime = correctedTime;
        }

        /// <summary>
        /// Computes the correction offset from the configured base offset, optional half-RTT, and optional input offset.
        /// </summary>
        private float CalculateCorrection(QuantumGame game)
        {
            float correction = Offset;
            if (ConsiderHalfRtt)
            {
                // half RTT in seconds
                correction += QuantumRunner.Default.Communicator.RoundTripTime / 2000.0f;
            }

            if (ConsiderInputOffset)
            {
                DeterministicSession session = game.Session;
                float inputOffset = session.LocalInputOffset;
                float inputOffsetInSeconds = inputOffset / session.SimulationRate;
                correction += inputOffsetInSeconds;
            }

            return correction;
        }

        /// <summary>
        /// Calculates real and corrected remaining seconds, clamps the result, and gradually decays the stored correction each frame.
        /// </summary>
        private void UpdateFairTimer(int timerDurationInTicks, int currentTicks, out float realTimeLeft,
            out float correctedTimeLeft)
        {
            // calculate actual remaining seconds
            realTimeLeft = (float)currentTicks / Game.Session.SimulationRate;

            // correct real time
            correctedTimeLeft = realTimeLeft + _timerCorrection;

            // consider input offset on remaining time
            if (ConsiderInputOffset)
            {
                float inputOffset = Game.Session.LocalInputOffset;
                float inputOffsetInSeconds = inputOffset / Game.Session.SimulationRate;
                correctedTimeLeft -= inputOffsetInSeconds;
            }

            // clamp corrected time
            float turnDuration = (float)timerDurationInTicks / Game.Session.SimulationRate;
            correctedTimeLeft = Mathf.Clamp(correctedTimeLeft, 0, turnDuration);

            // update correction
            float interpolation = _timerCorrection * InterpolationFactor / turnDuration;
            _timerCorrection -= Mathf.Clamp(interpolation, InterpolationMin, InterpolationMax);
            if (_timerCorrection < 0)
            {
                _timerCorrection = 0;
            }
        }

        /// <summary>
        /// Unsubscribes from all Quantum events when this component is destroyed.
        /// </summary>
        private void OnDestroy()
        {
            QuantumEvent.UnsubscribeListener(this);
        }
    }
}