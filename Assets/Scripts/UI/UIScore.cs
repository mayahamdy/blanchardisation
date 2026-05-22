using Quantum;
using UnityEngine;
using UnityEngine.UI;

namespace QuantumMiniGolf
{
    /// <summary>
    /// Displays the final score panel when gameplay ends, showing each player's stroke count.
    /// </summary>
    public class UIScore : QuantumSceneViewComponent
    {
        public GameObject Panel;
        public Text Score;

        /// <summary>
        /// Subscribes to the GameplayEnded event to show the score panel when the round finishes.
        /// </summary>
        private void Start()
        {
            QuantumEvent.Subscribe<EventGameplayEnded>(this, OnGameplayEnded);
        }

        /// <summary>
        /// Activates the score panel and populates it with the final score when gameplay ends.
        /// </summary>
        private void OnGameplayEnded(EventGameplayEnded e)
        {
            Panel.SetActive(true);
            int score = GetGameScore();
            UpdateScoreText(score);
        }

        /// <summary>
        /// Reads the single player's score from their BallFields component in the verified frame.
        /// </summary>
        private int GetGameScore()
        {
            Frame frame = Game.Frames.Verified;
            int score = 0;

            foreach (var (entity, actor) in frame.GetComponentIterator<Actor>())
            {
                var fields = frame.Get<BallFields>(entity);
                score = fields.Score;
                break; // Only one player in single-player mode
            }

            return score;
        }

        /// <summary>
        /// Updates the score label text with the final score.
        /// </summary>
        private void UpdateScoreText(int score)
        {
            Score.text = "Final Score: " + score + " strokes";
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