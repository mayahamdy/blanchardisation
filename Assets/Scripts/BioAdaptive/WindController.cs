using UnityEngine;

namespace QuantumMiniGolf
{
    [RequireComponent(typeof(Rigidbody))]
    public class WindController : MonoBehaviour
    {
        public static WindController Instance { get; private set; }

        /// <summary>Normalised world-space direction the wind is blowing towards.</summary>
        public Vector3 WindDirection { get; private set; }

        [Tooltip("Wind force multiplier when the ball is airborne (not touching the ground).")]
        public float airWindMultiplier = 4f;

        private Rigidbody _rb;
        private LocalGolfController _golf;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(this); return; }
            Instance = this;
        }

        private void Start()
        {
            _rb   = GetComponent<Rigidbody>();
            _golf = GetComponent<LocalGolfController>();

            float angle = Random.Range(0f, 360f);
            WindDirection = Quaternion.Euler(0, angle, 0) * Vector3.forward;
        }

        private void FixedUpdate()
        {
            if (StressAdapter.Instance == null) return;

            // Only push the ball while it is actively rolling after a shot.
            if (_golf != null && !_golf.IsRolling) return;

            if (_rb.linearVelocity.magnitude < 0.2f) return;

            bool grounded = Physics.Raycast(transform.position, Vector3.down, 0.6f);
            float multiplier = grounded ? 1f : airWindMultiplier;
            _rb.AddForce(WindDirection * StressAdapter.Instance.WindForce * multiplier, ForceMode.Force);
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }
    }
}
