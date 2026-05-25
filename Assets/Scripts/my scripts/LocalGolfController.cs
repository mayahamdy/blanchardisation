using BioAdaptive;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

[RequireComponent(typeof(Rigidbody))]
public class LocalGolfController : MonoBehaviour
{
    [Header("Shot Settings")]
    public float maxStrikeForce = 2000f;
    public LocalGameManager gameManager;
    public float chargeSpeed   = 0.75f;
    public float rotationSpeed = 10f;
    [Tooltip("How far in front of the ball the arrow sits")]
    public float arrowDistance = 0.5f;
    [Tooltip("Maximum upward launch angle (degrees) reached at full charge.")]
    public float airLaunchAngle = 8f;
    [Tooltip("Charge fraction (0-1) at which the upward tilt begins to ramp up.")]
    [Range(0f, 0.9f)]
    public float liftStartCharge = 0.6f;
    [Tooltip("Force curve exponent. 0.5 = sqrt (lots of power at low charge). 1.0 = linear.")]
    [Range(0.2f, 1f)]
    public float forceCurve = 0.7f;

    [Header("Out of Bounds")]
    [Tooltip("How far below the start position the ball must fall before it respawns.")]
    public float outOfBoundsDepth = 5f;

    [Header("UI Elements")]
    public Image      chargeBarFill;
    public GameObject aimingArrow;

    public float AimAngle  => _aimAngle;
    public bool  IsRolling => _rollTimer > 0f;

    [Header("Roll timeout")]
    [Tooltip("Seconds after a shot before the ball is forced to stop.")]
    public float rollTimeout = 7f;

    private Rigidbody _rb;
    private float _chargeRaw  = 0f;
    private bool  _isCharging = false;
    private int   _chargeDir  = 1;
    private float _aimAngle   = 0f;
    private float _rollTimer  = 0f;
    private Vector3 _startPosition;

    private void Start()
    {
        _rb            = GetComponent<Rigidbody>();
        _startPosition = transform.position;

        if (chargeBarFill != null)
            chargeBarFill.fillAmount = 0f;

        if (aimingArrow != null)
            aimingArrow.transform.SetParent(null);

        if (BioBridge.Instance != null)
        {
            BioBridge.Instance.OnShotStart += OnEMGShotStart;
            BioBridge.Instance.OnShotEnd   += OnEMGShotEnd;
        }
    }

    private void OnDestroy()
    {
        if (BioBridge.Instance != null)
        {
            BioBridge.Instance.OnShotStart -= OnEMGShotStart;
            BioBridge.Instance.OnShotEnd   -= OnEMGShotEnd;
        }
    }

    private void Update()
    {
        // Out-of-bounds: ball fell off the course.
        if (transform.position.y < _startPosition.y - outOfBoundsDepth)
            ResetToStart();

        // Auto-stop after rollTimeout; waits until ball is grounded if airborne.
        if (_rollTimer > 0f)
        {
            _rollTimer -= Time.deltaTime;
            if (_rollTimer <= 0f)
            {
                bool grounded = Physics.Raycast(transform.position, Vector3.down, 0.6f);
                if (grounded)
                {
                    _rollTimer          = 0f;
                    _rb.linearVelocity  = Vector3.zero;
                    _rb.angularVelocity = Vector3.zero;
                }
                else
                {
                    _rollTimer = 2f;
                }
            }
        }

        bool isMoving = _rb.linearVelocity.magnitude > 0.1f;

        if (aimingArrow != null)
            aimingArrow.SetActive(!isMoving && !_isCharging);

        if (isMoving) return;

        HandleAim();
        HandleChargeToggle();
        TickChargebar();
    }

    // ── Aiming ───────────────────────────────────────────────────────────────

    private void HandleAim()
    {
        if (_isCharging || Mouse.current == null) return;

        _aimAngle += Mouse.current.delta.x.ReadValue() * rotationSpeed * Time.deltaTime;

        if (aimingArrow == null) return;
        Vector3 forward = Quaternion.Euler(0, _aimAngle, 0) * Vector3.forward;
        aimingArrow.transform.position = transform.position + forward * arrowDistance + new Vector3(0, 0.05f, 0);
        aimingArrow.transform.rotation = Quaternion.Euler(90f, _aimAngle - 90f, 0f);
    }

    // ── Charge bar ───────────────────────────────────────────────────────────

    // Mouse fallback: only active when the BITalino sensor is not connected.
    private void HandleChargeToggle()
    {
        if (Mouse.current == null) return;
        if (!Mouse.current.leftButton.wasPressedThisFrame) return;
        if (BioBridge.Instance != null && BioBridge.Instance.IsConnected) return;

        if (!_isCharging) StartCharge();
        else              Shoot();
    }

    private void StartCharge()
    {
        _isCharging = true;
        _chargeRaw  = 0f;
        _chargeDir  = 1;
    }

    private void TickChargebar()
    {
        if (!_isCharging) return;

        _chargeRaw += _chargeDir * chargeSpeed * Time.deltaTime;

        if (_chargeRaw >= 1f) { _chargeRaw = 1f; _chargeDir = -1; }
        else if (_chargeRaw <= 0f) { _chargeRaw = 0f; _chargeDir = 1; }

        if (chargeBarFill != null)
            chargeBarFill.fillAmount = _chargeRaw;
    }

    // ── Firing ───────────────────────────────────────────────────────────────

    private void OnEMGShotStart()
    {
        if (_isCharging) return;
        StartCharge();
    }

    private void OnEMGShotEnd()
    {
        if (!_isCharging) return;
        Shoot();
    }

    private void Shoot()
    {
        _isCharging = false;
        _rollTimer  = rollTimeout;
        if (QuantumMiniGolf.SessionStats.Instance != null)
            QuantumMiniGolf.SessionStats.Instance.AddStroke();

        // Non-linear force curve: small charge still hits hard.
        float force = Mathf.Pow(_chargeRaw, forceCurve) * maxStrikeForce;

        // Tilt ramps from 0 at liftStartCharge up to airLaunchAngle at full charge.
        float liftFraction = Mathf.InverseLerp(liftStartCharge, 1f, _chargeRaw);
        float tilt = -airLaunchAngle * liftFraction;
        Vector3 dir = Quaternion.Euler(tilt, _aimAngle, 0) * Vector3.forward;
        _rb.AddForce(dir * force, ForceMode.Impulse);

        if (chargeBarFill != null) chargeBarFill.fillAmount = 0f;
        _chargeRaw = 0f;
    }

    // ── Out of bounds ─────────────────────────────────────────────────────────

    private void ResetToStart()
    {
        _rb.linearVelocity  = Vector3.zero;
        _rb.angularVelocity = Vector3.zero;
        transform.position  = _startPosition;
        _rollTimer          = 0f;
        _isCharging         = false;
        _chargeRaw          = 0f;
        if (chargeBarFill != null) chargeBarFill.fillAmount = 0f;
    }

    // ── Collision / trigger detection ─────────────────────────────────────────

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Flag") && gameManager != null)
        {
            gameManager.ShowWinScreen();
            gameObject.SetActive(false);
        }
        else if (other.CompareTag("Roughfield"))
        {
            ResetToStart();
        }
    }

    // Handles roughfield as a solid (non-trigger) collider.
    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag("Roughfield"))
            ResetToStart();
    }
}
