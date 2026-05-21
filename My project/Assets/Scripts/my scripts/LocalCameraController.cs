using UnityEngine;

public class LocalCameraController : MonoBehaviour
{
    [Header("Targeting")]
    public Transform target; // Drag your Golf Ball here in the inspector
    public Vector3 offset = new Vector3(0, 2f, -4f);
    public float followSpeed = 5f;

    void LateUpdate()
    {
        if (target == null) return;

        // Smoothly follow the target with the defined offset
        Vector3 desiredPosition = target.position + offset;
        transform.position = Vector3.Lerp(transform.position, desiredPosition, followSpeed * Time.deltaTime);
        
        // Always look at the target
        transform.LookAt(target);
    }
}