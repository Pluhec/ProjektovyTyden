using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(GridLayoutGroup))]
public class ResponsiveLatestGrid : MonoBehaviour
{
    [SerializeField] private RectTransform container;
    [SerializeField] private int maxColumns = 3;

    [Header("Breakpoints (width in px)")]
    [SerializeField] private float threeColsMin = 1150f;
    [SerializeField] private float twoColsMin = 800f;

    [Header("Sizing")]
    [SerializeField] private float cardHeight = 230f;

    private GridLayoutGroup grid;
    private int lastCols = -1;

    private void Awake()
    {
        grid = GetComponent<GridLayoutGroup>();
        if (!container) container = GetComponent<RectTransform>();
    }

    private void OnEnable() => Rebuild();
    private void OnRectTransformDimensionsChange() => Rebuild();

    public void Rebuild()
    {
        if (!grid || !container) return;

        float w = container.rect.width;
        int cols = (w >= threeColsMin) ? 3 : (w >= twoColsMin ? 2 : 1);
        cols = Mathf.Clamp(cols, 1, maxColumns);

        if (cols == lastCols && grid.cellSize.y == cardHeight) return;
        lastCols = cols;

        // výpočet šířky karty tak, aby vyšla přesně do řádku
        float totalPadding = grid.padding.left + grid.padding.right;
        float totalSpacing = grid.spacing.x * (cols - 1);
        float cellW = Mathf.Max(240f, (w - totalPadding - totalSpacing) / cols);

        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = cols;
        grid.cellSize = new Vector2(cellW, cardHeight);
    }
}