using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

[RequireComponent(typeof(Rigidbody))]
public class LocalGolfController : MonoBehaviour
{
    [Header("Shot Settings")]
    public float maxStrikeForce = 1000f;
    public LocalGameManager gameManager; // Link to the manager
    public float chargeSpeed = 1.5f; 
    public float rotationSpeed = 10f;
    [Tooltip("How far in front of the ball the arrow sits")]
    public float arrowDistance = 0.5f; 

    [Header("UI Elements")]
    public Image chargeBarFill;
    public GameObject aimingArrow; 

    private Rigidbody rb;
    private float currentChargeRaw = 0f; 
    private bool isCharging = false;
    private int chargeDirection = 1; 
    
    // Tracks the aiming direction independently of how the ball rolls
    private float currentAimAngle = 0f;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        
        if (chargeBarFill != null)
        {
            chargeBarFill.fillAmount = 0f;
        }
        
        // Detach the arrow from the ball so it doesn't tumble when the ball rolls
        if (aimingArrow != null)
        {
            aimingArrow.transform.SetParent(null);
        }
    }

    void Update()
    {
        bool isMoving = rb.linearVelocity.magnitude > 0.1f;

        // Toggle the arrow visibility
        if (aimingArrow != null)
        {
            aimingArrow.SetActive(!isMoving && !isCharging);
        }

        if (isMoving) return;

        Aim();
        HandleShooting();
    }

    private void Aim()
    {
        if (!isCharging && Mouse.current != null)
        {
            // Update our independent aim angle
            float mouseX = Mouse.current.delta.x.ReadValue();
            currentAimAngle += mouseX * rotationSpeed * Time.deltaTime;

            // Position and rotate the arrow independently
            if (aimingArrow != null)
            {
                // Calculate the forward direction based on our angle
                Vector3 aimForward = Quaternion.Euler(0, currentAimAngle, 0) * Vector3.forward;
                
                // Keep the arrow at the ball's position, offset forward, and raised slightly
                aimingArrow.transform.position = transform.position + (aimForward * arrowDistance) + new Vector3(0, 0.05f, 0);
                
                // Rotate the arrow to face the aim direction (X is 90 to lie flat)
                aimingArrow.transform.rotation = Quaternion.Euler(90f, currentAimAngle - 90f, 0f);
            }
        }
    }

    private void HandleShooting()
    {
        if (Mouse.current == null) return;

        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            if (!isCharging)
            {
                isCharging = true;
                currentChargeRaw = 0f;
                chargeDirection = 1;
            }
            else
            {
                Shoot();
            }
        }

        if (isCharging)
        {
            currentChargeRaw += chargeDirection * chargeSpeed * Time.deltaTime;

            if (currentChargeRaw >= 1f)
            {
                currentChargeRaw = 1f;
                chargeDirection = -1; 
            }
            else if (currentChargeRaw <= 0f)
            {
                currentChargeRaw = 0f;
                chargeDirection = 1; 
            }

            if (chargeBarFill != null)
            {
                chargeBarFill.fillAmount = currentChargeRaw;
            }
        }
    }

    private void Shoot()
    {
        isCharging = false;
        
        float finalForce = currentChargeRaw * maxStrikeForce;
        
        // Shoot in our independent aim direction, ignoring the ball's local rotation
        Vector3 shootDirection = Quaternion.Euler(0, currentAimAngle, 0) * Vector3.forward;
        rb.AddForce(shootDirection * finalForce, ForceMode.Impulse);
        
        if (chargeBarFill != null)
        {
            chargeBarFill.fillAmount = 0f;
        }
        currentChargeRaw = 0f;
    }

    // This built-in method fires whenever the ball enters a trigger collider
    private void OnTriggerEnter(Collider other)
    {
        // Check if the thing we hit has the "Flag" tag
        if (other.CompareTag("Flag"))
        {
            // Tell the GameManager to show the win screen
            if (gameManager != null)
            {
                gameManager.ShowWinScreen();
            }

            // Hide the ball
            gameObject.SetActive(false);
        }
    }
}