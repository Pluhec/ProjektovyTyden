using UnityEngine;

[RequireComponent(typeof(Camera))]
public class MapCameraMovement : MonoBehaviour
{
    [Header("Zoom Settings")]
    public float zoomSpeed = 5f;
    public float minZoom = 2f;
    public float maxZoom = 15f;
    public float zoomSmoothTime = 0.1f;

    [Header("Pan Settings")]
    public float panSmoothTime = 0.1f;
    [Tooltip("If true, you can only start panning when hovering over an object tagged 'Map' with a Collider.")]
    public bool requireMapHoverToPan = true;

    [Header("Map Bounds")]
    public bool useBounds = true;
    [Tooltip("The bottom-left corner of your map")]
    public Vector2 mapMinBounds = new Vector2(-20, -20);
    [Tooltip("The top-right corner of your map")]
    public Vector2 mapMaxBounds = new Vector2(20, 20);

    private Camera cam;
    private float targetOrthoSize;
    private float zoomVelocity;

    private Vector3 targetPosition;
    private Vector3 positionVelocity;

    private bool isDragging = false;
    private Vector3 dragStartMousePos;
    private Vector3 dragStartCamPos;

    void Start()
    {
        cam = GetComponent<Camera>();
        cam.orthographic = true; // Force orthographic mode
        targetOrthoSize = cam.orthographicSize;
        targetPosition = transform.position;
    }

    void Update()
    {
        HandleZoom();
        HandlePan();
        ClampTargetPosition();
    }

    void LateUpdate()
    {
        // Smoothly interpolate the camera's orthographic size and position
        cam.orthographicSize = Mathf.SmoothDamp(cam.orthographicSize, targetOrthoSize, ref zoomVelocity, zoomSmoothTime);
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref positionVelocity, panSmoothTime);
    }

    private void HandleZoom()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0f)
        {
            float oldOrthoSize = targetOrthoSize;
            targetOrthoSize -= scroll * zoomSpeed;
            targetOrthoSize = Mathf.Clamp(targetOrthoSize, minZoom, maxZoom);

            if (targetOrthoSize != oldOrthoSize)
            {
                // Calculate the exact world-space shift needed to keep the mouse over the same spot
                float zoomDelta = targetOrthoSize - oldOrthoSize;
                
                float xRatio = (Input.mousePosition.x / Screen.width) - 0.5f;
                float yRatio = (Input.mousePosition.y / Screen.height) - 0.5f;

                float heightChange = zoomDelta * 2f;
                float widthChange = heightChange * cam.aspect;

                targetPosition.x -= xRatio * widthChange;
                targetPosition.y -= yRatio * heightChange;
            }
        }
    }

    private void HandlePan()
    {
        if (Input.GetMouseButtonDown(1)) // Right click
        {
            if (!requireMapHoverToPan || IsHoveringMap())
            {
                isDragging = true;
                dragStartMousePos = Input.mousePosition;
                dragStartCamPos = targetPosition;
            }
        }

        if (Input.GetMouseButton(1) && isDragging)
        {
            Vector3 currentMousePos = Input.mousePosition;
            
            // Calculate exact world delta based on the TARGET zoom to prevent feedback loops/jitter
            float height = targetOrthoSize * 2f;
            float width = height * cam.aspect;

            float dx = ((currentMousePos.x - dragStartMousePos.x) / Screen.width) * width;
            float dy = ((currentMousePos.y - dragStartMousePos.y) / Screen.height) * height;

            targetPosition = dragStartCamPos - new Vector3(dx, dy, 0f);
        }

        if (Input.GetMouseButtonUp(1))
        {
            isDragging = false;
        }
    }

    private void ClampTargetPosition()
    {
        if (!useBounds) return;

        float camHeight = targetOrthoSize;
        float camWidth = targetOrthoSize * cam.aspect;

        float minX = mapMinBounds.x + camWidth;
        float maxX = mapMaxBounds.x - camWidth;
        float minY = mapMinBounds.y + camHeight;
        float maxY = mapMaxBounds.y - camHeight;

        // If the map is smaller than the camera view, center it
        if (minX > maxX) 
        {
            targetPosition.x = (mapMinBounds.x + mapMaxBounds.x) / 2f;
        }
        else 
        {
            targetPosition.x = Mathf.Clamp(targetPosition.x, minX, maxX);
        }

        if (minY > maxY) 
        {
            targetPosition.y = (mapMinBounds.y + mapMaxBounds.y) / 2f;
        }
        else 
        {
            targetPosition.y = Mathf.Clamp(targetPosition.y, minY, maxY);
        }
    }

    private bool IsHoveringMap()
    {
        // Check 2D Physics
        Vector2 mouseWorldPos = cam.ScreenToWorldPoint(Input.mousePosition);
        Collider2D[] hits2D = Physics2D.OverlapPointAll(mouseWorldPos);
        foreach (var hit in hits2D)
        {
            if (hit.CompareTag("Map")) return true;
        }

        // Check 3D Physics just in case
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit3D))
        {
            if (hit3D.collider.CompareTag("Map")) return true;
        }

        return false;
    }

    private void OnDrawGizmos()
    {
        if (!useBounds) return;

        Gizmos.color = Color.green;

        Vector3 bottomLeft = new Vector3(mapMinBounds.x, mapMinBounds.y, 0);
        Vector3 topLeft = new Vector3(mapMinBounds.x, mapMaxBounds.y, 0);
        Vector3 topRight = new Vector3(mapMaxBounds.x, mapMaxBounds.y, 0);
        Vector3 bottomRight = new Vector3(mapMaxBounds.x, mapMinBounds.y, 0);

        Gizmos.DrawLine(bottomLeft, topLeft);
        Gizmos.DrawLine(topLeft, topRight);
        Gizmos.DrawLine(topRight, bottomRight);
        Gizmos.DrawLine(bottomRight, bottomLeft);
    }
}
