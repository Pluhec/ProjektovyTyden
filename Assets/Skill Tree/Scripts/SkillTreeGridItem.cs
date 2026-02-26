using UnityEngine;

[ExecuteAlways]
[DisallowMultipleComponent]
[RequireComponent(typeof(RectTransform))]
public class SkillTreeGridItem : MonoBehaviour
{
    [Header("Grid Position")]
    public int gridX;
    public int gridY;

    [Header("Optional per-item offset (fine tuning)")]
    public Vector2 localOffset;

    [Header("Optional")]
    public bool ignoreLayout = false;

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

    private void OnValidate()
    {
        // Auto refresh when values change in inspector (editor only)
        if (!Application.isPlaying)
        {
            TryRefreshParentGrid();
        }
    }

    public void TryRefreshParentGrid()
    {
        var grid = GetComponentInParent<SkillTreeGrid>();
        if (grid != null)
            grid.RefreshLayout();
    }
}