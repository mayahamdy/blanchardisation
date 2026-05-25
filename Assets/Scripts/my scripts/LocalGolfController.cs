using BioAdaptive;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

[RequireComponent(typeof(Rigidbody))]
public class LocalGolfController : MonoBehaviour
{
    [Header("Shot Settings")]
    public float maxStrikeForce = 3000f;
    public LocalGameManager gameManager;
    public float chargeSpeed   = 1.5f;
    public float rotationSpeed = 10f;
    [Tooltip("How far in front of the ball the arrow sits")]
    public float arrowDistance = 0.5f;
    [Tooltip("Upward launch angle (degrees) added when the charge bar is at maximum.")]
    public float airLaunchAngle = 35f;

    [Header("UI Elements")]
    public Image    chargeBarFill;
    public GameObject aimingArrow;

    public float AimAngle  => _aimAngle;
    public bool  IsRolling => _rollTimer > 0f;

    [Header("Roll timeout")]
    [Tooltip("Seconds after a shot before the ball is forced to stop.")]
    public float rollTimeout = 7f;

    private Rigidbody _rb;
    private float _chargeRaw      = 0f;
    private bool  _isCharging     = false;
    private int   _chargeDir      = 1;
    private float _aimAngle       = 0f;
    private float _rollTimer      = 0f;

    private void Start()
    {
        _rb = GetComponent<Rigidbody>();

        if (chargeBarFill != null)
            chargeBarFill.fillAmount = 0f;

        // Detach arrow so it doesn't tumble when the ball rolls.
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
        // Auto-stop: force ball to rest after rollTimeout seconds.
        // If the ball is still airborne, wait another 2 s before trying again.
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
                    _rollTimer = 2f; // still airborne — retry in 2 s
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
    // First click = start charge, second click = shoot.
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

        float force = _chargeRaw * maxStrikeForce;

        // Full charge → launch into the air at airLaunchAngle degrees upward.
        float tilt = (_chargeRaw >= 0.99f) ? -airLaunchAngle : 0f;
        Vector3 dir = Quaternion.Euler(tilt, _aimAngle, 0) * Vector3.forward;
        _rb.AddForce(dir * force, ForceMode.Impulse);

        if (chargeBarFill != null) chargeBarFill.fillAmount = 0f;
        _chargeRaw = 0f;
    }

    // ── Hole detection ────────────────────────────────────────────────────────

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Flag") && gameManager != null)
        {
            gameManager.ShowWinScreen();
            gameObject.SetActive(false);
        }
    }
}
