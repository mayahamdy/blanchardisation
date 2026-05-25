using BioAdaptive;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

[RequireComponent(typeof(Rigidbody))]
public class LocalGolfController : MonoBehaviour
{
    [Header("Shot Settings")]
    public float maxStrikeForce = 1000f;
    public LocalGameManager gameManager;
    public float chargeSpeed   = 1.5f;
    public float rotationSpeed = 10f;
    [Tooltip("How far in front of the ball the arrow sits")]
    public float arrowDistance = 0.5f;

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

        // EMG shot trigger — fires once per muscle release.
        if (BioBridge.Instance != null)
            BioBridge.Instance.OnShotTriggered += OnEMGShot;
    }

    private void OnDestroy()
    {
        if (BioBridge.Instance != null)
            BioBridge.Instance.OnShotTriggered -= OnEMGShot;
    }

    private void Update()
    {
        // Auto-stop: force ball to rest after rollTimeout seconds.
        if (_rollTimer > 0f)
        {
            _rollTimer -= Time.deltaTime;
            if (_rollTimer <= 0f)
            {
                _rollTimer = 0f;
                _rb.linearVelocity    = Vector3.zero;
                _rb.angularVelocity   = Vector3.zero;
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

    // Left-click starts charging; a second left-click fires as a fallback
    // (useful when testing without the BITalino sensor).
    private void HandleChargeToggle()
    {
        if (Mouse.current == null) return;
        if (!Mouse.current.leftButton.wasPressedThisFrame) return;

        if (!_isCharging)
        {
            StartCharge();
        }
        else
        {
            // Fallback: fire on second click (sensor not connected).
            if (BioBridge.Instance == null || !BioBridge.Instance.IsConnected)
                Shoot();
        }
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

    // Called by BioBridge when the EMG muscle is released.
    private void OnEMGShot()
    {
        if (!_isCharging) return; // ignore if not in aiming phase
        Shoot();
    }

    private void Shoot()
    {
        _isCharging = false;
        _rollTimer  = rollTimeout;

        float force = _chargeRaw * maxStrikeForce;
        Vector3 dir = Quaternion.Euler(0, _aimAngle, 0) * Vector3.forward;
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
