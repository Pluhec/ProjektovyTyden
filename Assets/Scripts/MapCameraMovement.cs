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

    [Header("Wind Audio Settings")]
    public AudioSource windAudioSource;
    [Tooltip("Small movement threshold to avoid tiny camera jitter creating sound.")]
    public float movementDeadZone = 0.05f;
    [Range(0f, 1f)] public float minWindVolume = 0.05f;
    [Range(0f, 1f)] public float maxWindVolume = 0.8f;
    [Range(0.1f, 3f)] public float minWindPitch = 0.95f;
    [Range(0.1f, 3f)] public float maxWindPitch = 1.08f;
    [Tooltip("How fast wind reacts when speed increases (seconds). Lower = snappier.")]
    public float windAttackTime = 0.08f;
    [Tooltip("How fast wind decays when speed drops (seconds).")]
    public float windReleaseTime = 0.2f;
    public float pitchAttackTime = 0.1f;
    public float pitchReleaseTime = 0.25f;
    [Tooltip("Pan speed (units/second) where pan reaches full influence.")]
    public float panSpeedForMaxWind = 25f;
    [Tooltip("Zoom speed (ortho units/second) where zoom reaches full influence.")]
    public float zoomSpeedForMaxWind = 8f;
    [Tooltip("Panning contribution to wind intensity. Keep this above zoom for louder side movement.")]
    public float panLoudness = 1.0f;
    [Tooltip("Zoom contribution to wind intensity.")]
    public float zoomLoudness = 0.55f;

    private Camera cam;
    private float targetOrthoSize;
    private float zoomVelocity;

    private Vector3 targetPosition;
    private Vector3 positionVelocity;

    private bool isDragging = false;
    private Vector3 dragStartMousePos;
    private Vector3 dragStartCamPos;
    private Vector3 lastCameraPosition;
    private float lastOrthoSize;
    private float windVolumeVelocity;
    private float windPitchVelocity;

    void Start()
    {
        cam = GetComponent<Camera>();
        cam.orthographic = true; // Force orthographic mode
        
        UpdateMaxZoom();
        if (cam.orthographicSize > maxZoom)
        {
            cam.orthographicSize = maxZoom;
        }

        targetOrthoSize = cam.orthographicSize;
        targetPosition = transform.position;
        lastCameraPosition = transform.position;
        lastOrthoSize = cam.orthographicSize;

        if (windAudioSource != null)
        {
            windAudioSource.volume = 0f;
            windAudioSource.pitch = minWindPitch;
            if (!windAudioSource.isPlaying && windAudioSource.clip != null)
            {
                windAudioSource.Play();
            }
        }
    }

    private void UpdateMaxZoom()
    {
        if (!useBounds) return;

        float mapWidth = mapMaxBounds.x - mapMinBounds.x;
        float mapHeight = mapMaxBounds.y - mapMinBounds.y;

        float maxZoomHeight = mapHeight / 2f;
        float maxZoomWidth = mapWidth / (2f * cam.aspect);

        maxZoom = Mathf.Min(maxZoomHeight, maxZoomWidth);
        minZoom = Mathf.Min(minZoom, maxZoom);
    }

    void Update()
    {
        UpdateMaxZoom();
        if (targetOrthoSize > maxZoom)
        {
            targetOrthoSize = maxZoom;
        }

        HandleZoom();
        HandlePan();
        ClampTargetPosition();
    }

    void LateUpdate()
    {
        // Smoothly interpolate the camera's orthographic size and position
        cam.orthographicSize = Mathf.SmoothDamp(cam.orthographicSize, targetOrthoSize, ref zoomVelocity, zoomSmoothTime);
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref positionVelocity, panSmoothTime);
        UpdateWindAudio();
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
        if (Input.GetMouseButtonDown(1) || Input.GetMouseButtonDown(2)) // Right click
        {
            if (!requireMapHoverToPan || IsHoveringMap())
            {
                isDragging = true;
                dragStartMousePos = Input.mousePosition;
                dragStartCamPos = targetPosition;
            }
        }

        if ((Input.GetMouseButton(1) || Input.GetMouseButton(2)) && isDragging)
        {
            Vector3 currentMousePos = Input.mousePosition;
            
            // Calculate exact world delta based on the TARGET zoom to prevent feedback loops/jitter
            float height = targetOrthoSize * 2f;
            float width = height * cam.aspect;

            float dx = ((currentMousePos.x - dragStartMousePos.x) / Screen.width) * width;
            float dy = ((currentMousePos.y - dragStartMousePos.y) / Screen.height) * height;

            targetPosition = dragStartCamPos - new Vector3(dx, dy, 0f);
        }

        if (Input.GetMouseButtonUp(1) || Input.GetMouseButtonUp(2))
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

    private void UpdateWindAudio()
    {
        if (windAudioSource == null) return;

        float deltaTime = Mathf.Max(Time.deltaTime, 0.0001f);
        Vector3 frameDelta = transform.position - lastCameraPosition;
        float panSpeed = new Vector2(frameDelta.x, frameDelta.y).magnitude / deltaTime;
        float zoomSpeed = Mathf.Abs(cam.orthographicSize - lastOrthoSize) / deltaTime;
        lastCameraPosition = transform.position;
        lastOrthoSize = cam.orthographicSize;

        float panNormalized = Mathf.InverseLerp(movementDeadZone, panSpeedForMaxWind, panSpeed) * panLoudness;
        float zoomNormalized = Mathf.InverseLerp(0f, zoomSpeedForMaxWind, zoomSpeed) * zoomLoudness;
        float combinedNormalized = Mathf.Clamp01(Mathf.Max(panNormalized, zoomNormalized));

        float targetVolume = 0f;
        float targetPitch = minWindPitch;
        if (combinedNormalized > 0f)
        {
            targetVolume = Mathf.Lerp(minWindVolume, maxWindVolume, combinedNormalized);
            targetPitch = Mathf.Lerp(minWindPitch, maxWindPitch, combinedNormalized);
        }

        float smoothTime = targetVolume > windAudioSource.volume ? windAttackTime : windReleaseTime;
        windAudioSource.volume = Mathf.SmoothDamp(
            windAudioSource.volume,
            targetVolume,
            ref windVolumeVelocity,
            Mathf.Max(0.01f, smoothTime)
        );

        float pitchSmoothTime = targetPitch > windAudioSource.pitch ? pitchAttackTime : pitchReleaseTime;
        windAudioSource.pitch = Mathf.SmoothDamp(
            windAudioSource.pitch,
            targetPitch,
            ref windPitchVelocity,
            Mathf.Max(0.01f, pitchSmoothTime)
        );
    }
}
