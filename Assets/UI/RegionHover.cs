using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Attach to each region Image (Kraj 1–10) in the UI Canvas.
/// Uses alpha-hit-testing so only non-transparent pixels trigger hover,
/// which means irregular region shapes work automatically.
///
/// SETUP:
///  1) Stack all 10 region Images on top of each other (same RectTransform position & size).
///  2) Each Image uses its "Kraj X.png" sprite (transparent everywhere except its region area).
///  3) Assign normalSprite = "Kraj X.png", hoverSprite = "Kraj X hover.png".
///  4) In Unity, enable Read/Write on every Kraj sprite (Texture Import Settings).
///  5) Ensure the Canvas has a GraphicRaycaster and the scene has an EventSystem.
/// </summary>
public class RegionHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Region Setup")]
    [Tooltip("0-based region index (0 = Kraj 1, 9 = Kraj 10)")]
    [Range(0, 9)]
    public int regionIndex;

    [Tooltip("Normal sprite for this region")]
    public Sprite normalSprite;

    [Tooltip("Hover/highlight sprite for this region")]
    public Sprite hoverSprite;

    [Header("Alpha Hit-Test")]
    [Tooltip("Minimum alpha value (0-1) for a pixel to count as 'solid' for raycasting. " +
             "0.1 works well for anti-aliased edges.")]
    [Range(0.01f, 1f)]
    public float alphaThreshold = 0.1f;

    [Header("References")]
    [Tooltip("Drag the SimulationHandler GameObject here")]
    public SimulationHandler simulationHandler;

    // ── internals ──
    private Image regionImage;
    private bool isHovering;

    // Shared tooltip – created once, reused by all RegionHover instances
    private static GameObject sharedTooltip;
    private static TextMeshProUGUI sharedTooltipText;
    private static RectTransform sharedTooltipRect;
    private static Canvas rootCanvas;

    void Awake()
    {
        regionImage = GetComponent<Image>();
        if (regionImage != null && normalSprite == null)
            normalSprite = regionImage.sprite;
    }

    void Start()
    {
        // ── Diagnostics ──
        if (FindFirstObjectByType<EventSystem>() == null)
            Debug.LogError("[RegionHover] No EventSystem found in the scene! " +
                           "Add one via GameObject → UI → Event System.", this);

        Canvas parentCanvas = GetComponentInParent<Canvas>();
        if (parentCanvas != null && parentCanvas.GetComponent<GraphicRaycaster>() == null)
            Debug.LogError("[RegionHover] Canvas is missing a GraphicRaycaster component! " +
                           "Add one to the Canvas for pointer events to work.", parentCanvas);

        // Apply alpha hit-test so only non-transparent pixels trigger hover.
        // Must be done after sprites are fully loaded.
        ApplyAlphaHitTest();

        // Find root canvas (needed for tooltip positioning)
        if (rootCanvas == null && parentCanvas != null)
            rootCanvas = parentCanvas.rootCanvas;

        // Create the shared tooltip if it doesn't exist yet
        if (sharedTooltip == null)
            CreateTooltip();
    }

    private void ApplyAlphaHitTest()
    {
        if (regionImage == null || regionImage.sprite == null) return;

        Texture2D tex = regionImage.sprite.texture;
        if (tex == null) return;

        // Guard: only set if texture is truly readable
        if (!tex.isReadable)
        {
            Debug.LogWarning($"[RegionHover] Sprite '{regionImage.sprite.name}' texture is not readable. " +
                             "Enable Read/Write in Texture Import Settings. " +
                             "Alpha hit-test disabled for Kraj {regionIndex + 1}.", this);
            return;
        }

        try
        {
            regionImage.alphaHitTestMinimumThreshold = alphaThreshold;
        }
        catch (System.InvalidOperationException e)
        {
            Debug.LogWarning($"[RegionHover] Could not set alphaHitTestMinimumThreshold for Kraj {regionIndex + 1}: {e.Message}", this);
        }
    }

    // ──────────────────────────── Hover events ────────────────────────────

    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovering = true;

        // Swap sprite
        if (regionImage != null && hoverSprite != null)
        {
            regionImage.sprite = hoverSprite;
            // Re-apply threshold for the hover sprite too
            regionImage.alphaHitTestMinimumThreshold = alphaThreshold;
        }

        // Update & show tooltip
        UpdateTooltipText();
        if (sharedTooltip != null)
            sharedTooltip.SetActive(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovering = false;

        // Restore sprite
        if (regionImage != null && normalSprite != null)
        {
            regionImage.sprite = normalSprite;
            regionImage.alphaHitTestMinimumThreshold = alphaThreshold;
        }

        // Hide tooltip
        if (sharedTooltip != null)
            sharedTooltip.SetActive(false);
    }

    void Update()
    {
        if (!isHovering || sharedTooltip == null) return;

        // Follow the mouse
        PositionTooltipAtCursor();

        // Keep text fresh (stance changes every simulation step)
        UpdateTooltipText();
    }

    void OnDisable()
    {
        // Safety: reset sprite if the object is disabled while hovering
        if (isHovering && regionImage != null && normalSprite != null)
            regionImage.sprite = normalSprite;
        isHovering = false;
    }

    // ──────────────────────────── Tooltip helpers ─────────────────────────

    private void UpdateTooltipText()
    {
        if (simulationHandler == null || sharedTooltipText == null) return;

        float avg = simulationHandler.GetRegionAverage(regionIndex);

        sharedTooltipText.text = $"Kraj {regionIndex + 1}\nPrůměr: {avg:F1} %";
    }

    private void PositionTooltipAtCursor()
    {
        if (sharedTooltipRect == null || rootCanvas == null) return;

        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rootCanvas.transform as RectTransform,
            Input.mousePosition,
            rootCanvas.worldCamera,
            out localPoint
        );

        // Small offset so tooltip doesn't sit right under the cursor
        sharedTooltipRect.anchoredPosition = localPoint + new Vector2(16f, -16f);
    }

    // ──────────────────────────── Tooltip creation ────────────────────────

    private void CreateTooltip()
    {
        if (rootCanvas == null) return;

        // Panel
        sharedTooltip = new GameObject("RegionTooltip");
        sharedTooltip.transform.SetParent(rootCanvas.transform, false);
        sharedTooltipRect = sharedTooltip.AddComponent<RectTransform>();
        sharedTooltipRect.sizeDelta = new Vector2(180f, 60f);
        sharedTooltipRect.pivot = new Vector2(0f, 1f); // top-left pivot so it opens downward-right

        // Background image
        Image bg = sharedTooltip.AddComponent<Image>();
        bg.color = new Color(0.12f, 0.12f, 0.12f, 0.85f);
        bg.raycastTarget = false; // don't block raycasts

        // Text
        GameObject textGo = new GameObject("Text");
        textGo.transform.SetParent(sharedTooltip.transform, false);
        RectTransform textRect = textGo.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(8f, 4f);
        textRect.offsetMax = new Vector2(-8f, -4f);

        sharedTooltipText = textGo.AddComponent<TextMeshProUGUI>();
        sharedTooltipText.fontSize = 16f;
        sharedTooltipText.color = Color.white;
        sharedTooltipText.alignment = TextAlignmentOptions.Center;
        sharedTooltipText.raycastTarget = false;

        // Make sure tooltip renders on top of everything
        Canvas tooltipCanvas = sharedTooltip.AddComponent<Canvas>();
        tooltipCanvas.overrideSorting = true;
        tooltipCanvas.sortingOrder = 999;
        sharedTooltip.AddComponent<GraphicRaycaster>();

        sharedTooltip.SetActive(false);
    }
}
