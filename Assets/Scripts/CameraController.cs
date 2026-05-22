using Quantum;
using UnityEngine;

namespace QuantumMiniGolf
{
    /// <summary>
    /// Controls camera positioning, zoom, and rotation, following the current player's ball entity.
    /// </summary>
    [RequireComponent(typeof(Camera))]
    public class CameraController : QuantumSceneViewComponent
    {
        public bool FollowTarget = true;
        public float LerpVelocity = 3f;
        [Range(0, 300)] public float ZoomSensitivity = 150;
        [Range(0, 5)] public float ClosestDistance = 2;
        [Range(5, 20)] public float FarthestDistance = 10;
        [Range(0, 100)] public float RotationSensitivity = 50;
        public bool InvertRotation = false;
        [Range(0, 0.1f)] public float TargetClickableAreaRadius = 0.05f;

        [SerializeField] private Vector3 _relativeEulerRotation = new Vector3(-20, 0, 0);
        [SerializeField] private Vector3 _relativePosition = new Vector3(0, 1.5f, -3);
        private Camera _camera;
        private Transform _dummy;
        private QuantumEntityView _target;
        private Vector2 _lastClickPos;
        private bool _isRotating;

        public Vector3? TargetPosition => _target == null ? null : _target.transform.position;

        /// <summary>
        /// Caches the Camera component, creates the dummy transform, and subscribes to input events.
        /// </summary>
        public override void OnInitialize()
        {
            _camera = GetComponent<Camera>();
            InitDummy();
            InputManager.OnInputDown += OnInputDown;
            InputManager.OnInputDrag += OnInputDrag;
            InputManager.OnMouseScrollChanged += OnMouseScrollChanged;
        }

        /// <summary>
        /// Creates a hidden dummy GameObject whose transform the camera lerps towards, preserving the initial transform.
        /// </summary>
        private void InitDummy()
        {
            _dummy = new GameObject("CameraDummy").transform;
            Transform initTransform = transform;
            _dummy.position = initTransform.position;
            _dummy.rotation = initTransform.rotation;
        }

        /// <summary>
        /// Finds the current turn entity, updates the follow target, and smoothly lerps the camera towards the dummy.
        /// </summary>
        public override void OnUpdateView()
        {
            Frame frames = Game.Frames.Verified;
            if (frames == null)
                return;


            UpdateTarget(frames);
            LerpTowards(_dummy);
        }

        /// <summary>
        /// Checks whether the current turn entity has changed and, if so, reassigns the follow target and repositions the dummy.
        /// </summary>
        private void UpdateTarget(Frame frame)
        {
            TurnContainer turnContainer = frame.GetSingleton<TurnContainer>();
            EntityRef currentTurnEntity = turnContainer.CurrentTurn.Entity;
            if (frame.Exists(currentTurnEntity) == false)
                return;
            if (_target != null && _target.EntityRef == currentTurnEntity)
                return;
            
            _target = Updater.GetView(currentTurnEntity);
            if(_target == null)
                return;
            
            UpdateDummy(_target.transform);
        }

        /// <summary>
        /// Repositions the dummy relative to the target using the configured offset, orients it, and optionally parents it to the target.
        /// </summary>
        private void UpdateDummy(Transform target)
        {
            _dummy.position = target.position + _relativePosition;
            _dummy.LookAt(target);
            _dummy.Rotate(_relativeEulerRotation);
            if (FollowTarget)
            {
                _dummy.parent = _target.transform;
            }
        }

        /// <summary>
        /// Smoothly moves and rotates the camera towards the dummy transform each frame using the configured lerp velocity.
        /// </summary>
        private void LerpTowards(Transform target)
        {
            transform.position = Vector3.Lerp(transform.position, target.position, LerpVelocity * Time.deltaTime);
            transform.rotation = Quaternion.Lerp(transform.rotation, target.rotation, LerpVelocity * Time.deltaTime);
        }

        /// <summary>
        /// Determines whether the press started on the target or in open space, and records the starting position for drag tracking.
        /// </summary>
        private void OnInputDown(Vector2 viewportPos)
        {
            _isRotating = IsClickingOnTarget(viewportPos) == false;
            _lastClickPos = viewportPos;
        }

        /// <summary>
        /// Rotates the dummy around the target when the user drags in open space, respecting the invert-rotation setting.
        /// </summary>
        private void OnInputDrag(Vector2 viewportPos)
        {
            if (_isRotating == false)
                return;

            Vector2 rotAngle = (viewportPos - _lastClickPos) * RotationSensitivity;
            if (InvertRotation)
            {
                rotAngle *= -1;
            }

            _dummy.RotateAround(_target.transform.position, Vector3.up, rotAngle.x);
            _dummy.Rotate(Vector3.right, -rotAngle.y, Space.Self);
            _lastClickPos = viewportPos;
        }

        /// <summary>
        /// Zooms the camera by moving the dummy closer to or further from the target, clamped to the configured distance range.
        /// </summary>
        private void OnMouseScrollChanged(Vector2 delta)
        {
            Vector3 targetPos = _target.transform.position;
            Vector3 dummyPos = _dummy.position;
            float targetDist = Vector3.Distance(dummyPos, targetPos);
            float zoom = ZoomSensitivity * delta.y;
            zoom = Mathf.Clamp(zoom, targetDist - FarthestDistance, targetDist - ClosestDistance);
            dummyPos += zoom * (targetPos - dummyPos).normalized;
            _dummy.position = dummyPos;
        }

        /// <summary>
        /// Returns true if the given viewport position is within the clickable radius of the current target's projected position.
        /// </summary>
        public bool IsClickingOnTarget(Vector2 viewportPos)
        {
            Vector3 targetViewportPos = _camera.WorldToViewportPoint(_target.transform.position);
            return _target && Vector2.Distance(viewportPos, targetViewportPos) <= TargetClickableAreaRadius;
        }

        /// <summary>
        /// Unsubscribes from all input events to prevent callbacks firing after this component has been destroyed.
        /// </summary>
        private void OnDestroy()
        {
            InputManager.OnInputDown -= OnInputDown;
            InputManager.OnInputDrag -= OnInputDrag;
            InputManager.OnMouseScrollChanged -= OnMouseScrollChanged;
        }
    }
}