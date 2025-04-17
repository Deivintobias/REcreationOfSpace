using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Follow Settings")]
    public float followSpeed = 5f;
    public float heightAboveGround = 20f;
    public float lookAngle = 60f;
    public float distanceBehind = 10f;

    [Header("Boundaries")]
    public float minHeight = 15f;
    public float maxHeight = 25f;
    public float minAngle = 45f;
    public float maxAngle = 75f;
    public float zoomSpeed = 5f;

    private Transform target;
    private Vector3 offset;
    private float currentHeight;
    private float currentAngle;

    void Start()
    {
        currentHeight = heightAboveGround;
        currentAngle = lookAngle;
        UpdateCameraPosition();
    }

    void LateUpdate()
    {
        if (target == null) return;

        HandleZoom();
        HandleRotation();
        FollowTarget();
    }

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
        if (target != null)
        {
            UpdateCameraPosition();
        }
    }

    void HandleZoom()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0)
        {
            // Adjust height
            currentHeight = Mathf.Clamp(
                currentHeight - scroll * zoomSpeed,
                minHeight,
                maxHeight
            );

            // Adjust angle based on height
            float heightRatio = (currentHeight - minHeight) / (maxHeight - minHeight);
            currentAngle = Mathf.Lerp(minAngle, maxAngle, heightRatio);

            UpdateCameraPosition();
        }
    }

    void HandleRotation()
    {
        // Optional: Add camera rotation controls if needed
        // For now, keeping fixed rotation for Diablo-style view
    }

    void FollowTarget()
    {
        if (target == null) return;

        // Calculate desired position
        Vector3 desiredPosition = CalculateCameraPosition();
        
        // Smoothly move camera
        transform.position = Vector3.Lerp(
            transform.position,
            desiredPosition,
            followSpeed * Time.deltaTime
        );

        // Always look at target
        transform.LookAt(target.position);
    }

    void UpdateCameraPosition()
    {
        if (target == null) return;

        // Immediately set camera position
        transform.position = CalculateCameraPosition();
        transform.LookAt(target.position);
    }

    Vector3 CalculateCameraPosition()
    {
        // Calculate position based on height and angle
        float angleInRadians = currentAngle * Mathf.Deg2Rad;
        float distanceZ = Mathf.Cos(angleInRadians) * distanceBehind;
        float distanceY = Mathf.Sin(angleInRadians) * distanceBehind;

        return target.position - Vector3.forward * distanceZ + Vector3.up * (currentHeight + distanceY);
    }

    // Helper method to check if a position is visible to the camera
    public bool IsPositionVisible(Vector3 worldPosition)
    {
        Vector3 viewportPoint = GetComponent<Camera>().WorldToViewportPoint(worldPosition);
        return viewportPoint.x >= 0 && viewportPoint.x <= 1 &&
               viewportPoint.y >= 0 && viewportPoint.y <= 1 &&
               viewportPoint.z > 0;
    }

    // Get ground position from screen point
    public Vector3 GetGroundPosition(Vector2 screenPosition)
    {
        Ray ray = GetComponent<Camera>().ScreenPointToRay(screenPosition);
        Plane groundPlane = new Plane(Vector3.up, 0);
        
        float distance;
        if (groundPlane.Raycast(ray, out distance))
        {
            return ray.GetPoint(distance);
        }
        
        return Vector3.zero;
    }

    // Get direction from camera to point
    public Vector3 GetDirectionToPoint(Vector3 point)
    {
        return (point - transform.position).normalized;
    }

    void OnDrawGizmosSelected()
    {
        if (target != null)
        {
            // Draw camera focus point
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(target.position, 0.5f);

            // Draw camera angle
            Gizmos.color = Color.blue;
            Vector3 angleDirection = Quaternion.Euler(-currentAngle, 0, 0) * Vector3.forward;
            Gizmos.DrawRay(transform.position, angleDirection * 5f);
        }
    }
}
