using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Pixel-precise region hover using a grayscale region-map texture on a world-space quad.
///
/// Region encoding (gray = regionIndex / 10):
///   Region 1 → 0.1,  Region 2 → 0.2, … Region 10 → 1.0
///   Gray ≈ 0.0 or transparent → no region (background).
///
/// Casts a ray from the camera through the cursor, intersects the quad plane,
/// converts to UV, samples the cached texture, and resolves the region index.
/// Tooltip follows the cursor and shows the region's average stance.
/// </summary>
public class RegionMapHover : MonoBehaviour
{
    // ──────────────────────────── Inspector ───────────────────────────────

    [Header("World-Space Map")]
    [Tooltip("Transform of the quad displaying the region map")]
    public Transform mapQuad;

    [Tooltip("Grayscale region texture  (gray ≈ regionIndex / 10)")]
    public Texture2D regionTexture;

    [Header("References")]
    public Camera mainCamera;
    public SimulationHandler simulationHandler;

    [Header("UI")]
    [Tooltip("Canvas used for tooltip rendering (Screen Space)")]
    public Canvas tooltipCanvas;

    [Header("Detection Tuning")]
    [Tooltip("Max distance from the nearest 0.1 gray step to still count as that region")]
    [Range(0.01f, 0.08f)]
    public float grayTolerance = 0.045f;

    [Tooltip("Pixels with alpha below this are treated as background")]
    [Range(0, 255)]
    public byte minAlpha = 10;

    // ──────────────────────────── Internals ───────────────────────────────

    private Color32[] pixels;
    private int texW, texH;

    private int hoveredRegion = -1;

    private GameObject tooltipGO;
    private TextMeshProUGUI tooltipLabel;
    private RectTransform tooltipRect;

    // ──────────────────────────── Lifecycle ───────────────────────────────

    void Start()
    {
        CacheTexture();
        BuildTooltip();
    }

    void Update()
    {
        int region = RaycastRegion();

        if (region != hoveredRegion)
        {
            hoveredRegion = region;
            tooltipGO.SetActive(hoveredRegion >= 0);
        }

        if (hoveredRegion >= 0)
        {
            RefreshTooltipText();
            PositionTooltip();
        }
    }

    void OnDestroy()
    {
        if (tooltipGO != null)
            Destroy(tooltipGO);
    }

    // ──────────────────────── Texture cache ───────────────────────────────

    /// <summary>
    /// Reads all pixels into a CPU array via RenderTexture blit
    /// (works even if Read/Write is disabled on the source texture).
    /// </summary>
    private void CacheTexture()
    {
        RenderTexture rt = RenderTexture.GetTemporary(
            regionTexture.width, regionTexture.height, 0, RenderTextureFormat.ARGB32);
        Graphics.Blit(regionTexture, rt);

        RenderTexture prev = RenderTexture.active;
        RenderTexture.active = rt;

        Texture2D copy = new Texture2D(
            regionTexture.width, regionTexture.height, TextureFormat.RGBA32, false);
        copy.ReadPixels(new Rect(0, 0, regionTexture.width, regionTexture.height), 0, 0);
        copy.Apply();

        RenderTexture.active = prev;
        RenderTexture.ReleaseTemporary(rt);

        pixels = copy.GetPixels32();
        texW = copy.width;
        texH = copy.height;

        Destroy(copy);
    }

    // ────────────────────── Ray → Quad → Region ──────────────────────────

    /// <summary>
    /// Returns 0-based region index (0–9) or -1 when the cursor is not
    /// over any valid region.
    /// </summary>
    private int RaycastRegion()
    {
        if (mainCamera == null || mapQuad == null || pixels == null)
            return -1;

        // 1. Build a ray from the camera through the cursor
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

        // 2. Intersect the quad's plane  (local XY plane, normal = forward)
        Vector3 planeNormal = mapQuad.forward;
        Vector3 planeOrigin = mapQuad.position;

        float denom = Vector3.Dot(planeNormal, ray.direction);
        if (Mathf.Abs(denom) < 1e-7f) return -1;            // ray parallel to quad

        float t = Vector3.Dot(planeOrigin - ray.origin, planeNormal) / denom;
        if (t < 0f) return -1;                                // hit behind camera

        // 3. Convert world hit → quad-local → UV
        Vector3 worldHit = ray.origin + ray.direction * t;
        Vector3 local    = mapQuad.InverseTransformPoint(worldHit);

        // Default Unity Quad: local coords span [−0.5, +0.5] on X and Y
        float u = local.x + 0.5f;
        float v = local.y + 0.5f;

        if (u < 0f || u > 1f || v < 0f || v > 1f)
            return -1;                                        // outside quad bounds

        // 4. Sample the cached texture
        int px = Mathf.Clamp((int)(u * texW), 0, texW - 1);
        int py = Mathf.Clamp((int)(v * texH), 0, texH - 1);

        Color32 c = pixels[py * texW + px];

        // 5. Reject transparent pixels (background)
        if (c.a < minAlpha) return -1;

        // 6. Decode gray value → region
        float gray    = c.r / 255f;
        float snapped = Mathf.Round(gray * 10f) * 0.1f;

        if (Mathf.Abs(gray - snapped) > grayTolerance)
            return -1;                                        // anti-alias fringe

        int region = Mathf.RoundToInt(snapped * 10f) - 1;     // 0-based

        if (region < 0 || region > 9)
            return -1;                                        // gray ≈ 0.0 or overflow

        return region;
    }

    // ──────────────────────────── Tooltip ─────────────────────────────────

    private void RefreshTooltipText()
    {
        if (simulationHandler == null || tooltipLabel == null) return;

        float avg = simulationHandler.GetRegionAverage(hoveredRegion);
        tooltipLabel.text = $"Kraj {hoveredRegion + 1}\nPrůměr: {avg:F1}";
    }

    private void PositionTooltip()
    {
        if (tooltipRect == null || tooltipCanvas == null) return;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            tooltipCanvas.transform as RectTransform,
            Input.mousePosition,
            tooltipCanvas.worldCamera,
            out Vector2 localPoint);

        tooltipRect.anchoredPosition = localPoint + new Vector2(18f, -18f);
    }

    private void BuildTooltip()
    {
        if (tooltipCanvas == null)
        {
            Debug.LogError("[RegionMapHover] Assign a Canvas for the tooltip!", this);
            return;
        }

        // Root panel
        tooltipGO = new GameObject("RegionTooltip");
        tooltipGO.transform.SetParent(tooltipCanvas.transform, false);

        tooltipRect = tooltipGO.AddComponent<RectTransform>();
        tooltipRect.sizeDelta = new Vector2(160f, 56f);
        tooltipRect.pivot    = new Vector2(0f, 1f);   // opens downward-right

        Image bg = tooltipGO.AddComponent<Image>();
        bg.color = new Color(0.1f, 0.1f, 0.1f, 0.92f);
        bg.raycastTarget = false;

        // Label
        GameObject txtGO = new GameObject("Label");
        txtGO.transform.SetParent(tooltipGO.transform, false);

        RectTransform tr = txtGO.AddComponent<RectTransform>();
        tr.anchorMin = Vector2.zero;
        tr.anchorMax = Vector2.one;
        tr.offsetMin = new Vector2(8f, 4f);
        tr.offsetMax = new Vector2(-8f, -4f);

        tooltipLabel = txtGO.AddComponent<TextMeshProUGUI>();
        tooltipLabel.fontSize      = 15f;
        tooltipLabel.color         = Color.white;
        tooltipLabel.alignment     = TextAlignmentOptions.Center;
        tooltipLabel.raycastTarget = false;

        // Render on top
        Canvas overlay = tooltipGO.AddComponent<Canvas>();
        overlay.overrideSorting = true;
        overlay.sortingOrder   = 9999;

        tooltipGO.SetActive(false);
    }
}
