using UnityEngine;

namespace REcreationOfSpace.Camera
{
    public class CameraController : MonoBehaviour
    {
        [Header("Follow Settings")]
        [SerializeField] private Transform target;
        [SerializeField] private Vector3 offset = new Vector3(0, 10, -10);
        [SerializeField] private float smoothSpeed = 5f;
        [SerializeField] private float rotationAngle = 45f;

        [Header("Zoom Settings")]
        [SerializeField] private float minZoom = 5f;
        [SerializeField] private float maxZoom = 15f;
        [SerializeField] private float zoomSpeed = 2f;
        [SerializeField] private float zoomSmoothSpeed = 10f;

        private float currentZoom;
        private float targetZoom;

        private void Start()
        {
            // If no target is set, try to find the player
            if (target == null)
            {
                var player = GameObject.FindGameObjectWithTag("Player");
                if (player != null)
                    target = player.transform;
            }

            // Initialize zoom
            currentZoom = offset.magnitude;
            targetZoom = currentZoom;

            // Set initial rotation
            transform.rotation = Quaternion.Euler(rotationAngle, 0, 0);
        }

        private void LateUpdate()
        {
            if (target == null)
                return;

            // Handle zoom input
            float scrollInput = Input.GetAxis("Mouse ScrollWheel");
            if (scrollInput != 0)
            {
                targetZoom = Mathf.Clamp(targetZoom - scrollInput * zoomSpeed, minZoom, maxZoom);
            }

            // Smoothly adjust current zoom
            currentZoom = Mathf.Lerp(currentZoom, targetZoom, Time.deltaTime * zoomSmoothSpeed);

            // Calculate target position with zoom
            Vector3 zoomedOffset = offset.normalized * currentZoom;
            Vector3 targetPosition = target.position + zoomedOffset;

            // Smoothly move camera
            transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * smoothSpeed);

            // Optional: Add screen edge scrolling
            HandleScreenEdgeScrolling();
        }

        private void HandleScreenEdgeScrolling()
        {
            float edgeSize = 20f; // pixels from screen edge that triggers scrolling
            float scrollSpeed = 5f;
            Vector3 scrollDirection = Vector3.zero;

            // Check screen edges
            if (Input.mousePosition.x < edgeSize)
                scrollDirection.x = -1;
            else if (Input.mousePosition.x > Screen.width - edgeSize)
                scrollDirection.x = 1;

            if (Input.mousePosition.y < edgeSize)
                scrollDirection.z = -1;
            else if (Input.mousePosition.y > Screen.height - edgeSize)
                scrollDirection.z = 1;

            // Apply scrolling
            if (scrollDirection != Vector3.zero)
            {
                Vector3 right = transform.right;
                Vector3 forward = Vector3.Cross(right, Vector3.up);
                Vector3 scrollMovement = (right * scrollDirection.x + forward * scrollDirection.z) * scrollSpeed * Time.deltaTime;
                
                if (target != null)
                    target.position += scrollMovement;
            }
        }

        public void SetTarget(Transform newTarget)
        {
            target = newTarget;
        }

        public void SetOffset(Vector3 newOffset)
        {
            offset = newOffset;
            currentZoom = offset.magnitude;
            targetZoom = currentZoom;
        }
    }
}
