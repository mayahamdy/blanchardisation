using UnityEngine;
using Quantum;

namespace QuantumMiniGolf
{
    /// <summary>
    /// Handles drag-based aiming and force bar input for the local player's golf strike, then dispatches the play command.
    /// </summary>
    public class StrikeInput : QuantumSceneViewComponent
    {
        public static StrikeInput Instance { get; private set; }
        public bool PlayerIsAiming { get; private set; }

        [SerializeField] [Range(0, 800)] private float _aimSensitivity = 400;
        [SerializeField] private bool _reverseControls = true;
        [SerializeField] private bool _clampDirection = true;
        [SerializeField] [Range(0, 360)] private int _maxStrikeOpeningAngle = 120;

        private Vector2 _initViewportPos;
        private Vector3 _strikeInitDirection;
        private Vector3 _strikeDirection;
        private float _forceBarVel;
        private float _forceBarMarkPos = -1;
        private bool _forceBarIsRising = true;
        private int _minAngle;
        private int _maxAngle;
        private int _minForce;
        private int _maxForce;
        private Camera _mainCamera;
        private CameraController _cameraController;

        /// <summary>
        /// Enforces the singleton pattern, destroying any duplicate instance.
        /// </summary>
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
            }
            else
            {
                Instance = this;
            }
        }

        /// <summary>
        /// Caches the main camera and CameraController, subscribes to input events, and listens for the TurnEnded Quantum event.
        /// </summary>
        private void Start()
        {
            _mainCamera = Camera.main;
            _cameraController = _mainCamera.GetComponent<CameraController>();

            InputManager.OnInputDown += OnInputDown;
            InputManager.OnInputDrag += OnInputDrag;
            InputManager.OnInputUp += OnInputUp;

            QuantumEvent.Subscribe<EventTurnEnded>(this, OnTurnEnded);
        }

        /// <summary>
        /// Reads gameplay configuration values from the verified frame and caches them locally for use during aiming.
        /// </summary>
        private void InitConfigs(QuantumGame game)
        {
            Frame frame = game.Frames.Verified;
            ConfigAssets configAsset = frame.FindAsset<ConfigAssets>(frame.RuntimeConfig.ConfigAssets.Id);
            GameConfig gameConfig = frame.FindAsset<GameConfig>(configAsset.GameConfig.Id);
            _minForce = gameConfig.MinStrikeForce;
            _maxForce = gameConfig.MaxStrikeForce;
            _forceBarVel = gameConfig.ForceBarVelocity;
            _minAngle = gameConfig.MinStrikeAngle;
            _maxAngle = gameConfig.MaxStrikeAngle;
        }

        /// <summary>
        /// Defers config initialisation until the verified frame is ready, then ticks the force bar oscillation while aiming.
        /// </summary>
        public override void OnUpdateView()
        {
            Frame frame = Game.Frames.Verified;
            if (frame == null)
                return;

            // Defer config init until the verified frame is available
            if (_forceBarVel == 0)
            {
                InitConfigs(Game);
            }

            if (PlayerIsAiming)
            {
                UpdateForceBarMark();
            }
        }

        /// <summary>
        /// Advances the force bar mark position back and forth between -1 and 1 at the configured velocity, reversing direction at each extreme.
        /// </summary>
        private void UpdateForceBarMark()
        {
            if (_forceBarIsRising)
            {
                _forceBarMarkPos += _forceBarVel * Time.deltaTime;
                if (_forceBarMarkPos < 1)
                    return;

                _forceBarMarkPos = 1;
                _forceBarIsRising = false;
            }
            else
            {
                _forceBarMarkPos -= _forceBarVel * Time.deltaTime;
                if (_forceBarMarkPos > -1)
                    return;

                _forceBarMarkPos = -1;
                _forceBarIsRising = true;
            }
        }

        /// <summary>
        /// Begins aiming when the press lands on the current target, recording the initial viewport position and resetting the aim state.
        /// </summary>
        private void OnInputDown(Vector2 viewportPos)
        {
            if (_cameraController.IsClickingOnTarget(viewportPos) == false)
                return;


            PlayerIsAiming = true;
            _initViewportPos = viewportPos;
            ResetAim();
        }

        /// <summary>
        /// Updates the strike direction based on the drag delta and registers the current input with the InputPoller.
        /// </summary>
        private void OnInputDrag(Vector2 viewportPos)
        {
            if (PlayerIsAiming == false)
                return;

            ApplyDrag(viewportPos - _initViewportPos);
            InputPoller.Instance.RegisterInput(_strikeDirection.normalized.ToFPVector3(), _forceBarMarkPos.ToFP());
        }

        /// <summary>
        /// Fires the play command with the computed force on release, then clears the aiming state.
        /// </summary>
        private void OnInputUp(Vector2 viewportPos)
        {
            if (PlayerIsAiming)
            {
                float strikeForce = Mathf.Abs(_forceBarMarkPos) * _minForce +
                                    (1 - Mathf.Abs(_forceBarMarkPos)) * _maxForce;
                CommandDispatcher.Instance.SendPlayCommand(_strikeDirection.normalized.ToFPVector3(),
                    strikeForce.ToFP());
            }

            PlayerIsAiming = false;
        }

        /// <summary>
        /// Clears the aiming flag and resets the registered input when the current turn ends.
        /// </summary>
        private void OnTurnEnded(EventTurnEnded e)
        {
            PlayerIsAiming = false;
            InputPoller.Instance?.ResetRegisteredInput(e.Turn.Player);
        }

        /// <summary>
        /// Resets the force bar position and re-derives the initial strike direction from the camera's forward vector.
        /// </summary>
        private void ResetAim()
        {
            _forceBarMarkPos = -1;
            _strikeInitDirection = _mainCamera.transform.forward;
            _strikeInitDirection.y = 0;
            _strikeInitDirection = _strikeInitDirection.normalized;
            _strikeDirection = _strikeInitDirection;
        }

        /// <summary>
        /// Converts the drag delta into a world-space strike direction, applying sensitivity, optional inversion, and angle clamping.
        /// </summary>
        private void ApplyDrag(Vector2 drag)
        {
            drag *= _aimSensitivity;
            if (_reverseControls)
            {
                drag *= -1;
            }

            if (_clampDirection)
            {
                drag.y = Mathf.Clamp(drag.y, _minAngle, _maxAngle);
                drag.x = Mathf.Clamp(drag.x, min: -_maxStrikeOpeningAngle / 2.0f, max: _maxStrikeOpeningAngle / 2.0f);
            }

            _strikeDirection = Quaternion.AngleAxis(-drag.y, _mainCamera.transform.right) * _strikeInitDirection;
            _strikeDirection = Quaternion.AngleAxis(drag.x, Vector3.up) * _strikeDirection;
        }

        /// <summary>
        /// Unsubscribes from all input events and Quantum events when this component is destroyed.
        /// </summary>
        private void OnDestroy()
        {
            InputManager.OnInputDown -= OnInputDown;
            InputManager.OnInputDrag -= OnInputDrag;
            InputManager.OnInputUp -= OnInputUp;
            QuantumEvent.UnsubscribeListener(this);
        }
    }
}