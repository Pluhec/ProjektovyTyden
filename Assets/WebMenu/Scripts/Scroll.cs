using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// "Web-like" vertical scroll for UI built from Viewport + Content.
/// Attach to ScrollArea (any UI object). Assign viewport + content.
/// Requires: Viewport has Mask, Content uses LayoutGroup + ContentSizeFitter.
/// </summary>
public class Scroll : MonoBehaviour,
    IScrollHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("Refs")]
    [SerializeField] private RectTransform viewport; // visible area (masked)
    [SerializeField] private RectTransform content;  // moving content

    [Header("Tuning")]
    [SerializeField] private float wheelPixels = 160f;
    [SerializeField] private float dragMultiplier = 1f;
    [SerializeField] private bool invertWheel = false;

    [Header("Inertia")]
    [SerializeField] private bool useInertia = true;
    [SerializeField] private float damping = 12f;   // higher = stops faster
    [SerializeField] private float stopSpeed = 5f;

    [Header("Optional")]
    [SerializeField] private bool clampEveryFrame = false; // if layout changes at runtime

    private Vector2 lastDragPos;
    private float velocityY; // px/s
    private bool isDragging;

    private void Reset()
    {
        viewport = GetComponent<RectTransform>();
    }

    private void Awake()
    {
        if (!viewport) viewport = GetComponent<RectTransform>();
    }

    private void LateUpdate()
    {
        if (!viewport || !content) return;

        // if content size changes dynamically (adding/removing items), keep it clamped
        if (clampEveryFrame) ClampToViewport();

        if (!useInertia || isDragging) return;

        if (Mathf.Abs(velocityY) < stopSpeed)
        {
            velocityY = 0f;
            return;
        }

        Move(velocityY * Time.unscaledDeltaTime);

        // exponential-ish damping
        velocityY = Mathf.Lerp(velocityY, 0f, damping * Time.unscaledDeltaTime);
    }

    public void OnScroll(PointerEventData eventData)
    {
        if (!viewport || !content) return;

        // web-like direction
        float dir = invertWheel ? 1f : -1f;
        float dy = eventData.scrollDelta.y * wheelPixels * dir;

        Move(dy);

        // inertia based on last input
        velocityY = dy / Mathf.Max(Time.unscaledDeltaTime, 0.0001f);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!viewport || !content) return;

        isDragging = true;
        lastDragPos = eventData.position;
        velocityY = 0f;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!viewport || !content) return;

        Vector2 delta = eventData.position - lastDragPos;
        lastDragPos = eventData.position;

        // drag up => page down
        float dy = -delta.y * dragMultiplier;

        Move(dy);
        velocityY = dy / Mathf.Max(Time.unscaledDeltaTime, 0.0001f);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        isDragging = false;
    }

    /// <summary>
    /// Call this after you add/remove UI elements to ensure correct bounds.
    /// </summary>
    public void RebuildAndClamp()
    {
        Canvas.ForceUpdateCanvases();
        ClampToViewport();
    }

    private void Move(float dy)
    {
        // Ensure layout is up to date (Preferred Height etc.)
        Canvas.ForceUpdateCanvases();

        Vector2 pos = content.anchoredPosition;
        pos.y += dy;
        content.anchoredPosition = pos;

        ClampToViewport();
    }

    private void ClampToViewport()
    {
        Canvas.ForceUpdateCanvases();

        // Bounds of content in viewport-local space
        Bounds b = RectTransformUtility.CalculateRelativeRectTransformBounds(viewport, content);

        // Viewport limits in its own local space (respects viewport pivot)
        Rect vr = viewport.rect;
        float topLimit = vr.yMax;
        float bottomLimit = vr.yMin;

        Vector2 pos = content.anchoredPosition;

        // If content top is below viewport top => empty space above
        if (b.max.y < topLimit)
            pos.y += (topLimit - b.max.y);

        // If content bottom is above viewport bottom => empty space below
        if (b.min.y > bottomLimit)
            pos.y -= (b.min.y - bottomLimit);

        content.anchoredPosition = pos;
    }
}