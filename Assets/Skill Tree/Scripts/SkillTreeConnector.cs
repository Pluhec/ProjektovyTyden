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

#if UNITY_EDITOR
    private bool _needsRegenerate = false;
#endif

    private void OnEnable()
    {
        _rectTransform = GetComponent<RectTransform>();

        foreach (var root in rootNodes)
        {
            if (root?.skillButton == null) continue;
            SkillButton sb = root.skillButton.GetComponent<SkillButton>();
            if (sb != null && sb.skillState == SkillButton.SkillState.Locked)
                sb.SetState(SkillButton.SkillState.Available);
        }

        GenerateConnections();
    }

    private void OnValidate()
    {
        if (Application.isPlaying) return;
#if UNITY_EDITOR
        _needsRegenerate = true;
#endif
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

        SkillButton fromSkill = from.GetComponent<SkillButton>();
        SkillButton toSkill = to.GetComponent<SkillButton>();

        // Čára je červená pouze pokud jsou OBA konce unlocked
        bool isUnlocked = fromSkill != null && toSkill != null &&
                          fromSkill.skillState == SkillButton.SkillState.Unlocked &&
                          toSkill.skillState == SkillButton.SkillState.Unlocked;

        Color currentColor = isUnlocked ? Color.red : lineColor;

        float midY = (fromPos.y + toPos.y) / 2f;

        if (Mathf.Approximately(fromPos.x, toPos.x))
        {
            CreateLine(fromPos, toPos, currentColor);
        }
        else
        {
            Vector2 pointB = new Vector2(fromPos.x, midY);
            Vector2 pointC = new Vector2(toPos.x, midY);
            CreateLine(fromPos, pointB, currentColor);
            CreateLine(pointB, pointC, currentColor);
            CreateLine(pointC, toPos, currentColor);
        }
    }

    private Vector2 WorldToLocal(RectTransform rt)
    {
        Vector3 worldCenter = rt.TransformPoint(Vector3.zero);
        Vector3 local = _rectTransform.InverseTransformPoint(worldCenter);

        float pivotOffsetY = (_rectTransform.pivot.y - 2f) * _rectTransform.rect.height;

        return new Vector2(local.x, local.y - pivotOffsetY);
    }

    private void CreateLine(Vector2 start, Vector2 end, Color color)
{
    if ((end - start).sqrMagnitude < 0.01f) return;

    GameObject lineObj = new GameObject("SkillLine");
    lineObj.transform.SetParent(transform, false);
    lineObj.transform.SetAsFirstSibling();
    _spawnedLines.Add(lineObj);

    Image img = lineObj.AddComponent<Image>();
    img.color = color;
    img.raycastTarget = false;

    RectTransform rt = lineObj.GetComponent<RectTransform>();

    Vector2 dir = (end - start).normalized;
    float length = Vector2.Distance(start, end);
    float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

    Vector2 adjustedStart = start - dir * (lineWidth * 0.5f);
    float adjustedLength = length + lineWidth;

    rt.anchorMin = Vector2.zero;
    rt.anchorMax = Vector2.zero;
    rt.pivot = new Vector2(0f, 0.5f);
    rt.sizeDelta = new Vector2(adjustedLength, lineWidth);
    rt.anchoredPosition = adjustedStart;
    rt.localRotation = Quaternion.Euler(0, 0, angle);
}

// Přidej tuto novou metodu - projde celý strom a aktualizuje Available stavy
public void RefreshAllAvailability()
{
    // Projdi všechny nody a pro každý child zjisti jestli jsou všichni jeho parenti unlocked
    foreach (var root in rootNodes)
    {
        if (root != null)
            RefreshNodeAvailability(root);
    }
    GenerateConnections();
}

private void RefreshNodeAvailability(SkillNode node)
{
    if (node.skillButton == null) return;

    SkillButton skillBtn = node.skillButton.GetComponent<SkillButton>();
    if (skillBtn == null) return;

    foreach (var child in node.children)
    {
        if (child?.skillButton == null) continue;

        SkillButton childBtn = child.skillButton.GetComponent<SkillButton>();
        if (childBtn == null) continue;

        // Zkontroluj jestli jsou VŠICHNI parenti tohoto childu unlocked
        bool allParentsUnlocked = IsAllParentsUnlocked(child);
        childBtn.RefreshAvailability(allParentsUnlocked);

        RefreshNodeAvailability(child);
    }
}

// Projde celý strom a najde všechny parenty daného nodu
private bool IsAllParentsUnlocked(SkillNode targetNode)
{
    foreach (var root in rootNodes)
    {
        if (!CheckParentsUnlocked(root, targetNode))
            return false;
    }
    return true;
}

private bool CheckParentsUnlocked(SkillNode current, SkillNode target)
{
    foreach (var child in current.children)
    {
        if (child == target)
        {
            // Tento current je parent targetu - musí být unlocked
            SkillButton btn = current.skillButton?.GetComponent<SkillButton>();
            return btn != null && btn.skillState == SkillButton.SkillState.Unlocked;
        }

        if (!CheckParentsUnlocked(child, target))
            return false;
    }
    return true;
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
        if (!Application.isPlaying && _needsRegenerate)
        {
            _needsRegenerate = false;
            if (_rectTransform == null)
                _rectTransform = GetComponent<RectTransform>();
            GenerateConnections();
        }
    }
#endif
}