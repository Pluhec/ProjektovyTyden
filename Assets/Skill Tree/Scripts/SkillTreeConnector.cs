using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class SkillNode
{
    public RectTransform skillButton;
    public List<SkillNode> children = new List<SkillNode>();
}

[ExecuteInEditMode]
public class SkillTreeConnector : MonoBehaviour
{
    [Header("Skill Tree Structure")]
    public List<SkillNode> rootNodes = new List<SkillNode>();

    [Header("Line Settings")]
    public float lineWidth = 4f;
    public Color lineColor = Color.white;

    private List<GameObject> _spawnedLines = new List<GameObject>();
    private RectTransform _rectTransform;

    private void OnEnable()
    {
        _rectTransform = GetComponent<RectTransform>();
        GenerateConnections();
    }

    private void OnValidate()
    {
        if (Application.isPlaying) return;
        // OnValidate nesmí měnit RectTransform přímo - odložíme na další frame
        UnityEditor.EditorApplication.delayCall += () =>
        {
            if (this == null) return;
            _rectTransform = GetComponent<RectTransform>();
            GenerateConnections();
        };
    }

    [ContextMenu("Regenerate Connections")]
    public void GenerateConnections()
    {
        ClearLines();

        if (_rectTransform == null)
            _rectTransform = GetComponent<RectTransform>();

        foreach (var root in rootNodes)
        {
            if (root != null)
                DrawNodeConnections(root);
        }
    }

    private void DrawNodeConnections(SkillNode node)
    {
        if (node.skillButton == null) return;

        foreach (var child in node.children)
        {
            if (child == null || child.skillButton == null) continue;

            DrawOrthogonalLine(node.skillButton, child.skillButton);
            DrawNodeConnections(child);
        }
    }

    private void DrawOrthogonalLine(RectTransform from, RectTransform to)
    {
        Vector2 fromPos = WorldToLocal(from);
        Vector2 toPos = WorldToLocal(to);

        float midY = (fromPos.y + toPos.y) / 2f;

        if (Mathf.Approximately(fromPos.x, toPos.x))
        {
            CreateLine(fromPos, toPos);
        }
        else
        {
            Vector2 pointB = new Vector2(fromPos.x, midY);
            Vector2 pointC = new Vector2(toPos.x, midY);

            CreateLine(fromPos, pointB);
            CreateLine(pointB, pointC);
            CreateLine(pointC, toPos);
        }
    }

    private Vector2 WorldToLocal(RectTransform rt)
    {
        Vector3 worldCenter = rt.TransformPoint(Vector3.zero);
        Vector3 local = _rectTransform.InverseTransformPoint(worldCenter);

        // Kompenzace pivotu na ose Y
        float pivotOffsetY = (_rectTransform.pivot.y - 2f) * _rectTransform.rect.height;

        return new Vector2(local.x, local.y - pivotOffsetY);
    }

    private void CreateLine(Vector2 start, Vector2 end)
    {
        if ((end - start).sqrMagnitude < 0.01f) return;

        GameObject lineObj = new GameObject("SkillLine");
        lineObj.transform.SetParent(transform, false);
        lineObj.transform.SetAsFirstSibling();
        _spawnedLines.Add(lineObj);

        Image img = lineObj.AddComponent<Image>();
        img.color = lineColor;
        img.raycastTarget = false;

        RectTransform rt = lineObj.GetComponent<RectTransform>();

        Vector2 dir = end - start;
        float length = dir.magnitude;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

        // Anchor (0,0) = levý dolní roh - konzistentní s tím co vrací ScreenPointToLocalPointInRectangle
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.zero;
        rt.pivot = new Vector2(0f, 0.5f);
        rt.sizeDelta = new Vector2(length, lineWidth);
        rt.anchoredPosition = start;
        rt.localRotation = Quaternion.Euler(0, 0, angle);
    }

    public void ClearLines()
    {
        foreach (var line in _spawnedLines)
        {
            if (line == null) continue;
#if UNITY_EDITOR
            if (!Application.isPlaying)
                DestroyImmediate(line);
            else
#endif
                Destroy(line);
        }
        _spawnedLines.Clear();

#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                if (transform.GetChild(i).name == "SkillLine")
                    DestroyImmediate(transform.GetChild(i).gameObject);
            }
        }
#endif
    }

#if UNITY_EDITOR
    private void Update()
    {
        if (!Application.isPlaying)
        {
            if (_rectTransform == null)
                _rectTransform = GetComponent<RectTransform>();
            GenerateConnections();
        }
    }
#endif
}