using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
[DisallowMultipleComponent]
[RequireComponent(typeof(RectTransform))]
public class SkillTreeGrid : MonoBehaviour
{
    public enum GridOrigin { TopLeft, Center }
    public enum CellSizeMode { Fixed, FitToRect }

    [Header("Grid Size")]
    [Min(1)] public int columns = 14;
    [Min(1)] public int rows = 8;

    [Header("Cell Size")]
    public CellSizeMode cellSizeMode = CellSizeMode.FitToRect;

    [Min(1f)] public float cellWidth = 90f;
    [Min(1f)] public float cellHeight = 90f;

    [Header("Padding")]
    public float leftPadding = 40f;
    public float rightPadding = 40f;
    public float topPadding = 40f;
    public float bottomPadding = 40f;

    [Header("Origin")]
    public GridOrigin origin = GridOrigin.TopLeft;

    [Header("Placement")]
    public bool centerInCell = true;

    [Tooltip("Target parent containing GridItems. If null, this object is used.")]
    public RectTransform itemsContainer;

    [Header("Auto Refresh")]
    public bool refreshOnEnable = true;
    public bool refreshEveryFrameInEditor = false;

    private RectTransform _rectTransform;

    public RectTransform RectTransform
    {
        get
        {
            if (_rectTransform == null)
                _rectTransform = GetComponent<RectTransform>();
            return _rectTransform;
        }
    }

    private void Reset()
    {
        var rt = RectTransform;
        rt.pivot = new Vector2(0f, 1f); // top-left pivot recommended
        if (itemsContainer == null)
            itemsContainer = rt;
    }

    private void OnEnable()
    {
        if (itemsContainer == null)
            itemsContainer = RectTransform;

        if (refreshOnEnable)
            RefreshLayout();
    }

    private void OnValidate()
    {
        if (itemsContainer == null)
            itemsContainer = RectTransform;

        if (!Application.isPlaying)
            RefreshLayout();
    }

    private void Update()
    {
#if UNITY_EDITOR
        if (!Application.isPlaying && refreshEveryFrameInEditor)
            RefreshLayout();
#endif
    }

    [ContextMenu("Refresh Layout")]
    public void RefreshLayout()
    {
        if (itemsContainer == null)
            itemsContainer = RectTransform;

        // 1) Spočítej cell size, pokud má být fit do panelu
        if (cellSizeMode == CellSizeMode.FitToRect)
        {
            var size = RectTransform.rect.size;

            float usableW = Mathf.Max(1f, size.x - leftPadding - rightPadding);
            float usableH = Mathf.Max(1f, size.y - topPadding - bottomPadding);

            cellWidth = usableW / columns;
            cellHeight = usableH / rows;
        }

        // 2) Rozmísti itemy
        var items = itemsContainer.GetComponentsInChildren<SkillTreeGridItem>(true);
        HashSet<Vector2Int> occupied = new HashSet<Vector2Int>();

        foreach (var item in items)
        {
            if (item == null || item.ignoreLayout)
                continue;

            Vector2Int gridPos = new Vector2Int(item.gridX, item.gridY);

            if (!occupied.Add(gridPos))
                Debug.LogWarning($"Duplicate grid cell [{gridPos.x}, {gridPos.y}] used by multiple items.", item);

            Vector2 anchoredPos = GridToAnchoredPosition(gridPos);

            if (centerInCell)
                anchoredPos += new Vector2(cellWidth * 0.5f, -cellHeight * 0.5f);

            anchoredPos += item.localOffset;

            RectTransform itemRT = item.RectTransform;

            // DŮLEŽITÉ: jednotné anchor nastavení pro konzistentní výsledek
            itemRT.anchorMin = new Vector2(0f, 1f);
            itemRT.anchorMax = new Vector2(0f, 1f);

            itemRT.anchoredPosition = anchoredPos;
        }
    }

    public Vector2 GridToAnchoredPosition(Vector2Int gridPos)
    {
        switch (origin)
        {
            case GridOrigin.TopLeft:
                return new Vector2(
                    leftPadding + (gridPos.x * cellWidth),
                    -topPadding - (gridPos.y * cellHeight)
                );

            case GridOrigin.Center:
                float totalWidth = columns * cellWidth;
                float totalHeight = rows * cellHeight;

                float startX = -totalWidth * 0.5f + leftPadding;
                float startY = totalHeight * 0.5f - topPadding;

                return new Vector2(
                    startX + (gridPos.x * cellWidth),
                    startY - (gridPos.y * cellHeight)
                );

            default:
                return Vector2.zero;
        }
    }
}