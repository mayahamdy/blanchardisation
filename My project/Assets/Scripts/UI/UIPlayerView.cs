using Quantum;
using TurnBasedFramework;
using UnityEngine;
using UI = UnityEngine.UI;

namespace QuantumMiniGolf
{
    /// <summary>
    /// Displays player HUD information including turn number, turn status, timer, and console messages for single-player.
    /// </summary>
    public class UIPlayerView : QuantumSceneViewComponent
    {
        public UI.Text TurnNumber;
        public UI.Text TurnStatus;
        public UI.Text Console;
        public UI.Button SkipButton;
        public UI.Image FillableTimer;
        public FairTimerVisuals FairTimerVisuals;
        public CanvasGroup CanvasGroup;

        private int _maxStrokes;
        private EntityRef _ballRef;
        private const int SINGLE_PLAYER_REF = 0;

        /// <summary>
        /// Subscribes to the GameStarted callback to initialise the max strokes value once the game begins.
        /// </summary>
        private void Awake()
        {
            QuantumCallback.Subscribe<CallbackGameStarted>(this, OnGameState);
        }

        /// <summary>
        /// Resets the timer fill and reads the max strokes value from the game config on game start.
        /// </summary>
        private void OnGameState(CallbackGameStarted callback)
        {
            FillableTimer.fillAmount = 0;
            GameConfig gameConfig = ConfigAssetsHelper.GetGameConfig(callback.Game.Frames.Verified);
            _maxStrokes = gameConfig.MaxStrokes;
        }

        /// <summary>
        /// Refreshes all HUD elements each frame: skip button, timer, turn number, status, and console text.
        /// </summary>
        public override void OnUpdateView()
        {
            Frame frame = Game.Frames.Verified;
            if (frame == null)
                return;

            UpdateSkipButton(frame);
            UpdateTimer(frame);

            if (_ballRef == EntityRef.None)
            {
                _ballRef = GetPlayerBall(frame, SINGLE_PLAYER_REF);
            }

            if (_ballRef == EntityRef.None)
            {
                if (CanvasGroup.alpha > 0f)
                {
                    CanvasGroup.alpha = 0f;
                }

                return;
            }

            if (CanvasGroup.alpha < 1f)
            {
                CanvasGroup.alpha = 1f;
            }

            BallFields ballFields = frame.Get<BallFields>(_ballRef);

            UpdateTurnNumber(ballFields.TurnStats.Number, max: _maxStrokes);
            UpdateTurnStatus(ballFields.TurnStats.Status);
            UpdateConsole(frame, ballFields.HasCompletedCourse);
        }

        /// <summary>
        /// Enables or disables the skip button depending on whether it is the player's turn.
        /// </summary>
        private void UpdateSkipButton(Frame frame)
        {
            TurnContainer turnContainer = frame.GetSingleton<TurnContainer>();
            if (turnContainer.CurrentTurn.Type == ETurnType.Play)
            {
                SkipButton.interactable = true;
            }
            else
            {
                SkipButton.interactable = false;
            }
        }

        /// <summary>
        /// Sets the timer fill amount to reflect the current turn progress.
        /// </summary>
        private void UpdateTimer(Frame frame)
        {
            TurnContainer turnContainer = frame.GetSingleton<TurnContainer>();
            if (turnContainer.CurrentTurn.Type == ETurnType.Play)
            {
                FillableTimer.fillAmount = 1 - FairTimerVisuals.TimerPercentage;
            }
            else
            {
                FillableTimer.fillAmount = 0;
            }
        }

        /// <summary>
        /// Sets the turn-number label to a 'current/max' format string.
        /// </summary>
        private void UpdateTurnNumber(int current, int max)
        {
            TurnNumber.text = string.Format("{0}/{1}", current.ToString(), max.ToString());
        }

        /// <summary>
        /// Sets the turn-status label to the string representation of the given status.
        /// </summary>
        private void UpdateTurnStatus(ETurnStatus status)
        {
            TurnStatus.text = status.ToString();
        }

        /// <summary>
        /// Sets the console label to reflect whether the course is finished or clears it otherwise.
        /// </summary>
        private void UpdateConsole(Frame frame, bool hasCompletedCourse)
        {
            if (hasCompletedCourse)
            {
                Console.text = "Course Finished";
            }
            else
            {
                Console.text = string.Empty;
            }
        }

        /// <summary>
        /// Iterates all Actor components to find and return the entity belonging to the given player.
        /// </summary>
        private EntityRef GetPlayerBall(Frame frame, int player)
        {
            foreach (var (entity, actor) in frame.GetComponentIterator<Actor>())
            {
                if (actor.Player == player)
                {
                    return entity;
                }
            }

            return default(EntityRef);
        }
    }
}