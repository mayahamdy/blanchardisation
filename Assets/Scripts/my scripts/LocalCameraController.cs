using UnityEngine;

public class LocalCameraController : MonoBehaviour
{
    [Header("Targeting")]
    public Transform target;
    public LocalGolfController golfController;

    [Header("Position")]
    public float distance    = 2.5f;
    public float height      = 1.0f;
    public float followSpeed = 6f;

    [Tooltip("How far above the ball the camera looks — keeps the horizon visible.")]
    public float lookAheadHeight = 0.4f;

    void LateUpdate()
    {
        if (target == null) return;

        float angle = golfController != null ? golfController.AimAngle : 0f;

        // Unit vector pointing in the aim direction.
        Vector3 aimDir = Quaternion.Euler(0, angle, 0) * Vector3.forward;

        // Camera sits behind the ball along the aim axis, slightly above it.
        Vector3 desired = target.position - aimDir * distance + Vector3.up * height;
        transform.position = Vector3.Lerp(transform.position, desired, followSpeed * Time.deltaTime);

        transform.LookAt(target.position + Vector3.up * lookAheadHeight);
    }
}
