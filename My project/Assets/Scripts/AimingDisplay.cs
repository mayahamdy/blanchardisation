using UnityEngine;
using UnityEngine.UI;
using Quantum;
using Input = UnityEngine.Input;

namespace QuantumMiniGolf
{
    /// <summary>
    /// Displays the aiming arrow and force bar UI for the local and remote players during their turn.
    /// </summary>
    public class AimingDisplay : QuantumSceneViewComponent
    {
        public QuantumEntityView Entity;
        public Transform Arrow;
        public Image ForceBar;
        public Image ForceBarMark;
        [Range(0, 0.5f)] public float LocalPlayerInterpolationFactor;
        [Range(0, 0.5f)] public float RemotePlayerInterpolationFactor = 0.05f;

        private Quantum.Input _mostRecentInput;
        private PlayerRef _playerRef;
        private bool _initPlayer;

        /// <summary>
        /// Updates the aiming UI each frame by reading input and refreshing the arrow direction and force bar.
        /// </summary>
        public override void OnUpdateView()
        {
            Frame frame = Game.Frames.Verified;
            if (frame == null)
                return;

            if (_initPlayer == false)
            {
                _initPlayer = true;
                _playerRef = frame.Get<Actor>(Entity.EntityRef).Player;
            }

            BallFields fields = frame.Get<BallFields>(Entity.EntityRef);
            if (fields.TurnStats.Status != ETurnStatus.Active)
            {
                EnableUI(false);
                return;
            }

            Quantum.Input currentInput = GetCurrentInput(frame, _playerRef);
            if (currentInput.Equals(default(Quantum.Input)) == false)
            {
                EnableUI(true);
                _mostRecentInput = currentInput;
            }

            UpdateShotPreview(_mostRecentInput, Game.PlayerIsLocal(_playerRef));
        }

        /// <summary>
        /// Returns the most relevant input for the given player, preferring live local input during aiming over the verified frame input.
        /// </summary>
        private Quantum.Input GetCurrentInput(Frame frame, int player)
        {
            Quantum.Input currentInput;
            if (Game.PlayerIsLocal(player) && StrikeInput.Instance.PlayerIsAiming)
            {
                currentInput = InputPoller.Instance.RegisteredInput;
            }
            else
            {
                currentInput = frame.GetPlayerInputValue(player);
            }

            return currentInput;
        }

        /// <summary>
        /// Rotates the arrow and repositions the force bar mark to reflect the current aiming input, with optional interpolation.
        /// </summary>
        private void UpdateShotPreview(Quantum.Input input, bool playerIsLocal)
        {
            float interpolationFactor =
                playerIsLocal ? LocalPlayerInterpolationFactor : RemotePlayerInterpolationFactor;
            Vector3 inputVector = input.Direction.ToUnityVector3();
            Quaternion targetRotation = Quaternion.identity;
            if (inputVector != Vector3.zero)
            {
                targetRotation = Quaternion.LookRotation(inputVector);
            }

            if (interpolationFactor > 0)
            {
                Arrow.rotation = Quaternion.Lerp(Arrow.rotation, targetRotation, Time.deltaTime / interpolationFactor);
            }
            else
            {
                Arrow.rotation = targetRotation;
            }

            float forceBarHeight = ((RectTransform)ForceBarMark.rectTransform.parent.transform).rect.height;
            Vector2 targetPosition = (forceBarHeight / 2) * input.ForceBarMarkPos.AsFloat * Vector2.up;
            if (interpolationFactor > 0)
            {
                ForceBarMark.rectTransform.anchoredPosition =
                    Vector2.Lerp(ForceBarMark.rectTransform.anchoredPosition, targetPosition,
                        Time.deltaTime / interpolationFactor);
            }
            else
            {
                ForceBarMark.rectTransform.anchoredPosition = targetPosition;
            }
        }

        /// <summary>
        /// Shows or hides the arrow and force bar GameObjects only when their active state actually needs to change.
        /// </summary>
        private void EnableUI(bool value)
        {
            if (Arrow.gameObject.activeSelf != value)
            {
                Arrow.gameObject.SetActive(value);
            }

            if (ForceBar.gameObject.activeSelf != value)
            {
                ForceBar.gameObject.SetActive(value);
            }
        }
    }
}