using UnityEngine;

public class ParalaxClouds : MonoBehaviour
{
    [Header("References")]
    public Camera mainCamera;
    public Renderer cloudRenderer;

    [Header("Parallax Settings (Foreground)")]
    [Tooltip("The inverse movement factor when fully zoomed OUT (fadeStartZoom).")]
    public float minInverseMoveFactor = 0.2f;
    [Tooltip("The inverse movement factor when fully zoomed IN (fadeEndZoom).")]
    public float maxInverseMoveFactor = 5.0f;
    
    [Header("Zoom Through Settings")]
    [Tooltip("The zoom level (orthographic size) at which clouds START scaling up and fading out (Zoomed Out).")]
    public float fadeStartZoom = 8f;
    [Tooltip("The zoom level at which clouds are completely invisible and fully scaled (Zoomed In).")]
    public float fadeEndZoom = 4f;
    
    [Tooltip("How much the clouds scale up when fully zoomed in.")]
    public float maxZoomScaleMultiplier = 6.0f;
    [Tooltip("The scale multiplier when fully zoomed out.")]
    public float minZoomScaleMultiplier = 1.0f;
    
    [Header("Opacity Settings")]
    [Tooltip("The maximum opacity of the clouds when fully zoomed out.")]
    [Range(0f, 1f)]
    public float maxOpacity = 0.78f;
    [Tooltip("The minimum opacity of the clouds when fully zoomed in.")]
    [Range(0f, 1f)]
    public float minOpacity = 0.0f;
    
    [Tooltip("The exact name of the Opacity property in your cloud shader.")]
    public string opacityPropertyName = "_CloudOpacity";

    [Header("Debug (View Only)")]
    public float debugCurrentZoom;
    public float debugCalculatedOpacity;
    public float debugCalculatedMoveFactor;

    private Vector3 startCameraPos;
    private Vector3 startCloudPos;
    private Vector3 startScale;
    private Material cloudMaterial;
    private int opacityPropertyID;

    void Start()
    {
        if (mainCamera == null) mainCamera = Camera.main;
        if (cloudRenderer == null) cloudRenderer = GetComponent<Renderer>();

        if (mainCamera != null)
        {
            startCameraPos = mainCamera.transform.position;
        }
        
        startCloudPos = transform.position;
        startScale = transform.localScale;

        if (cloudRenderer != null)
        {
            // Create an instance of the material so we don't change the shared asset
            cloudMaterial = cloudRenderer.material;
        }
        
        // Cache the property ID for maximum reliability
        opacityPropertyID = Shader.PropertyToID(opacityPropertyName);
    }

    void LateUpdate()
    {
        if (mainCamera == null || cloudMaterial == null) return;

        float currentZoom = mainCamera.orthographicSize;
        debugCurrentZoom = currentZoom;

        // Calculate zoom percentage (0 = at fadeStartZoom (Zoomed Out), 1 = at fadeEndZoom (Zoomed In))
        // InverseLerp clamps between 0 and 1 automatically.
        float zoomT = Mathf.InverseLerp(fadeStartZoom, fadeEndZoom, currentZoom);
        
        // Smooth the transition for better visuals
        float smoothT = Mathf.SmoothStep(0f, 1f, zoomT);

        // 1. Handle Parallax (Movement)
        Vector3 cameraDelta = mainCamera.transform.position - startCameraPos;
        
        // Interpolate the inverse move factor based on zoom
        // Zoomed OUT (smoothT = 0) -> minInverseMoveFactor
        // Zoomed IN (smoothT = 1) -> maxInverseMoveFactor
        float currentMoveFactor = Mathf.Lerp(minInverseMoveFactor, maxInverseMoveFactor, smoothT);
        debugCalculatedMoveFactor = currentMoveFactor;
        
        // Target position is the start position MINUS the camera movement (inverse)
        Vector3 targetPos = startCloudPos - (cameraDelta * currentMoveFactor);
        
        // Smoothly interpolate position
        transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * 10f);

        // 2. Handle Fading & Scaling (Zooming Through)
        
        // As we zoom in (smoothT goes 0 -> 1), opacity goes maxOpacity -> minOpacity
        float targetOpacity = Mathf.Lerp(maxOpacity, minOpacity, smoothT);
        debugCalculatedOpacity = targetOpacity;
        
        // As we zoom in, scale goes minZoomScaleMultiplier -> maxZoomScaleMultiplier
        float targetScaleMult = Mathf.Lerp(minZoomScaleMultiplier, maxZoomScaleMultiplier, smoothT);
        transform.localScale = startScale * targetScaleMult;

        // Apply to the shader directly using the Property ID (most reliable method)
        cloudMaterial.SetFloat(opacityPropertyID, targetOpacity);
        
        // Fallback: Also try setting it by string just in case
        cloudMaterial.SetFloat(opacityPropertyName, targetOpacity);
    }
}
