using UnityEngine;

namespace DwarfClone.Core
{
    public class CameraController : MonoBehaviour
    {
        public static CameraController Instance { get; private set; }

        [Header("Movement")]
        [SerializeField] private float panSpeed = 20f;
        [SerializeField] private float fastPanMultiplier = 2.5f;
        [SerializeField] private float edgePanBorder = 15f;
        [SerializeField] private bool enableEdgePan = false;

        [Header("Zoom")]
        [SerializeField] private float zoomSpeed = 4f;
        [SerializeField] private float minOrthoSize = 4f;
        [SerializeField] private float maxOrthoSize = 30f;
        [SerializeField] private float smoothTime = 0.15f;

        private Camera cam;
        private Vector3 targetPosition;
        private float targetOrthoSize;
        private Vector3 dragOrigin;
        private bool isDragging = false;
        private Transform followTarget = null;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            cam = GetComponent<Camera>();
            targetPosition = transform.position;
            targetOrthoSize = cam.orthographicSize;
        }

        private void Update()
        {
            HandleKeyboardPan();
            HandleMouseDrag();
            HandleZoom();
            HandleFollow();
            ApplyMovement();
        }

        private void HandleKeyboardPan()
        {
            Vector3 move = Vector3.zero;

            float speed = panSpeed * (Input.GetKey(KeyCode.LeftShift) ? fastPanMultiplier : 1f);

            if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) move.y += 1f;
            if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) move.y -= 1f;
            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) move.x -= 1f;
            if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) move.x += 1f;

            if (enableEdgePan && !isDragging)
            {
                Vector3 mouse = Input.mousePosition;
                if (mouse.x < edgePanBorder && mouse.x >= 0) move.x -= 1f;
                else if (mouse.x > Screen.width - edgePanBorder && mouse.x <= Screen.width) move.x += 1f;
                if (mouse.y < edgePanBorder && mouse.y >= 0) move.y -= 1f;
                else if (mouse.y > Screen.height - edgePanBorder && mouse.y <= Screen.height) move.y += 1f;
            }

            if (move.sqrMagnitude > 0.01f)
            {
                followTarget = null; // Break follow
                move.Normalize();
                targetPosition += move * (speed * Time.unscaledDeltaTime);
                ClampTargetPosition();
            }
        }

        private void HandleMouseDrag()
        {
            // Middle mouse drag
            if (Input.GetMouseButtonDown(2))
            {
                dragOrigin = cam.ScreenToWorldPoint(Input.mousePosition);
                isDragging = true;
                followTarget = null;
            }

            if (Input.GetMouseButton(2) && isDragging)
            {
                Vector3 currentPos = cam.ScreenToWorldPoint(Input.mousePosition);
                Vector3 diff = dragOrigin - currentPos;
                targetPosition += diff;
                ClampTargetPosition();
            }

            if (Input.GetMouseButtonUp(2))
            {
                isDragging = false;
            }
        }

        private void HandleZoom()
        {
            float scroll = Input.mouseScrollDelta.y;
            if (Mathf.Abs(scroll) > 0.01f)
            {
                targetOrthoSize = Mathf.Clamp(targetOrthoSize - scroll * zoomSpeed, minOrthoSize, maxOrthoSize);
            }
        }

        private void HandleFollow()
        {
            if (followTarget != null)
            {
                targetPosition = new Vector3(followTarget.position.x, followTarget.position.y, transform.position.z);
                ClampTargetPosition();
            }
        }

        private void ApplyMovement()
        {
            transform.position = Vector3.Lerp(transform.position, targetPosition, 10f * Time.unscaledDeltaTime);
            cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, targetOrthoSize, 10f * Time.unscaledDeltaTime);
        }

        private void ClampTargetPosition()
        {
            float minX = 0f;
            float maxX = Constants.WORLD_WIDTH * Constants.TILE_WORLD_SIZE;
            float minY = 0f;
            float maxY = Constants.WORLD_HEIGHT * Constants.TILE_WORLD_SIZE;

            targetPosition.x = Mathf.Clamp(targetPosition.x, minX, maxX);
            targetPosition.y = Mathf.Clamp(targetPosition.y, minY, maxY);
        }

        public void FocusOn(Vector3 worldPosition)
        {
            followTarget = null;
            targetPosition = new Vector3(worldPosition.x, worldPosition.y, transform.position.z);
            ClampTargetPosition();
        }

        public void Follow(Transform target)
        {
            followTarget = target;
        }
    }
}
