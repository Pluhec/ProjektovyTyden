using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Region hover using a grayscale texture on a world-space quad.
/// Red channel encodes region: 0 → Kraj 1, 1 → Kraj 2, … 9 → Kraj 10.
/// Transparent pixels = background (no region).
/// </summary>
public class RegionMapHover : MonoBehaviour
{
    [Header("World-Space Map")]
    public Transform mapQuad;
    public Texture2D regionTexture;

    [Header("References")]
    public Camera mainCamera;
    public SimulationHandler simulationHandler;

    [Header("UI")]
    public Canvas tooltipCanvas;

    private Color32[] pixels;
    private int texW, texH;
    private int hoveredRegion = -1;

    private GameObject tooltipGO;
    private TextMeshProUGUI tooltipLabel;
    private RectTransform tooltipRect;

    void Start()
    {
        CacheTexture();
        BuildTooltip();
    }

    void Update()
    {
        int region = SampleRegionUnderCursor();

        if (region != hoveredRegion)
        {
            hoveredRegion = region;
            tooltipGO.SetActive(hoveredRegion >= 0);
        }

        if (hoveredRegion < 0) return;

        RefreshTooltipText();
        PositionTooltip();
    }

    void OnDestroy()
    {
        if (tooltipGO != null)
            Destroy(tooltipGO);
    }

    // ─────────────────────── Texture cache ────────────────────────────────

    private void CacheTexture()
    {
        var rt = RenderTexture.GetTemporary(regionTexture.width, regionTexture.height, 0, RenderTextureFormat.ARGB32);
        Graphics.Blit(regionTexture, rt);

        var prev = RenderTexture.active;
        RenderTexture.active = rt;

        var copy = new Texture2D(regionTexture.width, regionTexture.height, TextureFormat.RGBA32, false);
        copy.ReadPixels(new Rect(0, 0, regionTexture.width, regionTexture.height), 0, 0);
        copy.Apply();

        RenderTexture.active = prev;
        RenderTexture.ReleaseTemporary(rt);

        pixels = copy.GetPixels32();
        texW = copy.width;
        texH = copy.height;
        Destroy(copy);
    }

    // ──────────────────── Cursor → Region index ──────────────────────────

    private int SampleRegionUnderCursor()
    {
        if (mainCamera == null || mapQuad == null || pixels == null)
            return -1;

        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        Vector3 normal = mapQuad.forward;
        float denom = Vector3.Dot(normal, ray.direction);

        if (Mathf.Abs(denom) < 1e-7f) return -1;

        float t = Vector3.Dot(mapQuad.position - ray.origin, normal) / denom;
        if (t < 0f) return -1;

        Vector3 local = mapQuad.InverseTransformPoint(ray.GetPoint(t));
        float u = local.x + 0.5f;
        float v = local.y + 0.5f;

        if (u < 0f || u > 1f || v < 0f || v > 1f)
            return -1;

        int px = Mathf.Clamp((int)(u * texW), 0, texW - 1);
        int py = Mathf.Clamp((int)(v * texH), 0, texH - 1);
        Color32 c = pixels[py * texW + px];

        if (c.a < 10) return -1;

        int region = c.r;
        return (region >= 0 && region <= 9) ? region : -1;
    }

    // ──────────────────────── Tooltip ─────────────────────────────────────

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
            out Vector2 pt);

        tooltipRect.anchoredPosition = pt + new Vector2(18f, -18f);
    }

    private void BuildTooltip()
    {
        if (tooltipCanvas == null)
        {
            Debug.LogError("[RegionMapHover] Assign a Canvas for the tooltip!", this);
            return;
        }

        tooltipGO = new GameObject("RegionTooltip");
        tooltipGO.transform.SetParent(tooltipCanvas.transform, false);

        tooltipRect = tooltipGO.AddComponent<RectTransform>();
        tooltipRect.sizeDelta = new Vector2(160f, 56f);
        tooltipRect.pivot = new Vector2(0f, 1f);

        var bg = tooltipGO.AddComponent<Image>();
        bg.color = new Color(0.1f, 0.1f, 0.1f, 0.92f);
        bg.raycastTarget = false;

        var txtGO = new GameObject("Label");
        txtGO.transform.SetParent(tooltipGO.transform, false);

        var tr = txtGO.AddComponent<RectTransform>();
        tr.anchorMin = Vector2.zero;
        tr.anchorMax = Vector2.one;
        tr.offsetMin = new Vector2(8f, 4f);
        tr.offsetMax = new Vector2(-8f, -4f);

        tooltipLabel = txtGO.AddComponent<TextMeshProUGUI>();
        tooltipLabel.fontSize = 15f;
        tooltipLabel.color = Color.white;
        tooltipLabel.alignment = TextAlignmentOptions.Center;
        tooltipLabel.raycastTarget = false;

        var overlay = tooltipGO.AddComponent<Canvas>();
        overlay.overrideSorting = true;
        overlay.sortingOrder = 9999;

        tooltipGO.SetActive(false);
    }
}
