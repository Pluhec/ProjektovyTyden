using UnityEngine;
using UnityEngine.UI;
using TMPro;

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

    public GameObject tooltipObject;
    public TextMeshProUGUI populationText;
    public TextMeshProUGUI vekText;
    public TextMeshProUGUI educationText;
    public TextMeshProUGUI regionText;
    public TextMeshProUGUI stanceText;

    [Header("Detection Tuning")]

    [Tooltip("Pixels with alpha below this are treated as background")]
    [Range(0, 255)]
    public byte minAlpha = 10;

    // ──────────────────────────── Internals ───────────────────────────────

    private Color32[] pixels;
    private int texW, texH;

    private int hoveredRegion = -1;
    private int shaderHoveredRegion = -1;

    private TextMeshProUGUI tooltipLabel;
    private RectTransform tooltipRect;
    
    public Material regionHoverMaterial;
    private float[] shaderRegionHovers = new float[10];

    // ──────────────────────────── Lifecycle ───────────────────────────────

    void Start()
    {
        CacheTexture();

        if (tooltipObject != null)
            tooltipRect = tooltipObject.GetComponent<RectTransform>();
    }

    void Update()
    {
        // Shader highlight always follows hover
        shaderHoveredRegion = RaycastRegion();

        // Tooltip only on right-click hold
        if (Input.GetMouseButton(1))
        {
            if (shaderHoveredRegion != hoveredRegion)
            {
                hoveredRegion = shaderHoveredRegion;
                tooltipObject.SetActive(hoveredRegion >= 0);
            }

            if (hoveredRegion >= 0)
            {
                RefreshTooltipText();
                PositionTooltip();
            }
        }
        else if (hoveredRegion >= 0)
        {
            hoveredRegion = -1;
            tooltipObject.SetActive(false);
        }
        UpdateShaderRegion();
    }
    // ──────────────────────── Texture cache ───────────────────────────────

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

    private int RaycastRegion()
    {
        if (mainCamera == null || mapQuad == null || pixels == null)
            return -1;

        Vector3 mousePos = Input.mousePosition;
        if (!float.IsFinite(mousePos.x) || !float.IsFinite(mousePos.y))
            return -1;

        Ray ray = mainCamera.ScreenPointToRay(mousePos);

        Vector3 planeNormal = mapQuad.forward;
        Vector3 planeOrigin = mapQuad.position;

        float denom = Vector3.Dot(planeNormal, ray.direction);
        if (Mathf.Abs(denom) < 1e-7f) return -1;

        float t = Vector3.Dot(planeOrigin - ray.origin, planeNormal) / denom;
        if (t < 0f) return -1;

        Vector3 worldHit = ray.origin + ray.direction * t;
        Vector3 local    = mapQuad.InverseTransformPoint(worldHit);

        float u = local.x + 0.5f;
        float v = local.y + 0.5f;

        if (u < 0f || u > 1f || v < 0f || v > 1f)
            return -1;


        int px = Mathf.Clamp((int)(u * texW), 0, texW - 1);
        int py = Mathf.Clamp((int)(v * texH), 0, texH - 1);

        Color32 c = pixels[py * texW + px];

        if (c.a < minAlpha) return -1;

        int region = Mathf.FloorToInt(c.r / 256.0f * 10f);

        if (region < 0 || region > 9)
            return -1;

        return region;  // 0-based
    }

    // ──────────────────────────── Tooltip ─────────────────────────────────

    private void RefreshTooltipText()
    {
        if (simulationHandler == null) return;

        if (populationText != null)
            populationText.text = simulationHandler.GetRegionPopulation(hoveredRegion).ToString();

        if (educationText != null)
            educationText.text = $"{simulationHandler.GetRegionEducationAverage(hoveredRegion):F1}";

        if (vekText != null)
            vekText.text = $"{simulationHandler.GetRegionAgeAverage(hoveredRegion):F1}";

        if(stanceText != null)
            stanceText.text = $"{simulationHandler.GetRegionAverage(hoveredRegion):F1}";

        if (regionText != null)
            regionText.text = $"Region: {hoveredRegion + 1}";
    }

    private void PositionTooltip()
    {
        if (tooltipRect == null || tooltipCanvas == null) return;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            tooltipCanvas.transform as RectTransform,
            Input.mousePosition,
            tooltipCanvas.worldCamera,
            out Vector2 localPoint);

        // Set the tooltip's top-left corner to the mouse position (no offset)
        Vector2 pivot = tooltipRect.pivot;
        Vector2 adjustedPosition = localPoint + new Vector2(tooltipRect.rect.width * pivot.x, -tooltipRect.rect.height * (1 - pivot.y));
        tooltipRect.anchoredPosition = adjustedPosition;
    }

    void UpdateShaderRegion()
    {
        if (regionHoverMaterial == null) return;

        for (int i = 0; i < shaderRegionHovers.Length; i++)
            shaderRegionHovers[i] = (i == shaderHoveredRegion) ? 1f : 0f;
        regionHoverMaterial.SetFloatArray("_RegionHovers", shaderRegionHovers);
    }
}
