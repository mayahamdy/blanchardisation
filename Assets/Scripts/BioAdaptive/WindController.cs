using UnityEngine;

namespace QuantumMiniGolf
{
    [RequireComponent(typeof(Rigidbody))]
    public class WindController : MonoBehaviour
    {
        public static WindController Instance { get; private set; }

        /// <summary>Normalised world-space direction the wind is blowing towards.</summary>
        public Vector3 WindDirection { get; private set; }

        private Rigidbody _rb;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(this); return; }
            Instance = this;
        }

        private void Start()
        {
            _rb = GetComponent<Rigidbody>();
            float angle  = Random.Range(0f, 360f);
            WindDirection = Quaternion.Euler(0, angle, 0) * Vector3.forward;
        }

        private void FixedUpdate()
        {
            if (StressAdapter.Instance == null) return;
            if (_rb.linearVelocity.magnitude < 0.05f) return;
            _rb.AddForce(WindDirection * StressAdapter.Instance.WindForce, ForceMode.Force);
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }
    }
}
