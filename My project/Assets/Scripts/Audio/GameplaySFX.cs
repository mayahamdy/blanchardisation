using UnityEngine;
using Quantum;

namespace QuantumMiniGolf
{
    /// <summary>
    /// Subscribes to Quantum gameplay events and plays the corresponding sound effects at the character's world position.
    /// </summary>
    public class GameplaySFX : QuantumSceneViewComponent
    {
        /// <summary>
        /// Subscribes to all relevant Quantum character events for sound effect playback.
        /// </summary>
        private void Start()
        {
            QuantumEvent.Subscribe<EventBallShot>(this, OnBallShot);
            QuantumEvent.Subscribe<EventBallHit>(this, OnBallHit);
            QuantumEvent.Subscribe<EventGameplayEnded>(this, OnGameEnded);
        }

        /// <summary>
        /// Plays the shot sound effect at the ball's position when a shot event is received.
        /// </summary>
        private void OnBallShot(EventBallShot eventData)
        {
            Transform3D transform = PredictedFrame.Get<Transform3D>(eventData.Ball);
            AudioManager.Instance.Play("shot", transform.Position.ToUnityVector3());
        }
        
        /// <summary>
        /// Plays the shot sound effect at the ball's position when a shot event is received.
        /// </summary>
        private void OnBallHit(EventBallHit eventData)
        {
            Transform3D transform = PredictedFrame.Get<Transform3D>(eventData.Ball);
            AudioManager.Instance.Play("hit", transform.Position.ToUnityVector3());
        }
        
        /// <summary>
        /// Plays the end game sound effect when the end game event is received.
        /// </summary>
        private void OnGameEnded(EventGameplayEnded eventData)
        {
            AudioManager.Instance.Play("game-ended", Vector3.zero);
        }

        /// <summary>
        /// Unsubscribes from all Quantum events when this component is disabled.
        /// </summary>
        public override void OnDisable()
        {
            QuantumEvent.UnsubscribeListener(this);
        }
    }
}
