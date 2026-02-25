using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class SkillNode
{
    public RectTransform skillButton;
    public List<SkillNode> children = new List<SkillNode>();
}

[System.Serializable]
public class SkillLine
{
    public GameObject lineObject;
    public RectTransform from;
    public RectTransform to;
    public Vector2 start;
    public Vector2 end;
    public string connectionId;
}

[ExecuteInEditMode]
public class SkillTreeConnector : MonoBehaviour
{
    [Header("Skill Tree Structure")]
    public List<SkillNode> rootNodes = new List<SkillNode>();

    [Header("Line Settings")]
    public float lineWidth = 4f;
    public Color lineColor = Color.white;
    public Color unlockedLineColor = Color.red;

    [Header("Animation")]
    public float lineAnimationSpeed = 500f;

    [Header("Popup")]
    public GameObject popupPrefab;

    private List<SkillLine> _spawnedLines = new List<SkillLine>();
    private RectTransform _rectTransform;

#if UNITY_EDITOR
    private bool _needsRegenerate = false;
#endif

    private void OnEnable()
    {
        _rectTransform = GetComponent<RectTransform>();

        // Vždy smaž všechny existující SkillLine objekty při startu
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            GameObject child = transform.GetChild(i).gameObject;
            if (child.name == "SkillLine")
            {
                if (Application.isPlaying)
                    Destroy(child);
                else
                    DestroyImmediate(child);
            }
        }
        _spawnedLines.Clear();

        foreach (var root in rootNodes)
        {
            if (root?.skillButton == null) continue;
            SkillButton sb = root.skillButton.GetComponent<SkillButton>();
            if (sb != null && sb.skillState == SkillButton.SkillState.Locked)
                sb.SetState(SkillButton.SkillState.Available);
        }

        GenerateConnections();
    }

    private void OnDisable()
    {
        // Při vypnutí hry smaž všechny runtime čáry
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            GameObject child = transform.GetChild(i).gameObject;
            if (child.name == "SkillLine")
            {
                if (Application.isPlaying)
                    Destroy(child);
                else
                    DestroyImmediate(child);
            }
        }
        _spawnedLines.Clear();
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

        string connId = $"{from.name}->{to.name}";
        float midY = (fromPos.y + toPos.y) / 2f;

        if (Mathf.Approximately(fromPos.x, toPos.x))
        {
            CreateLine(fromPos, toPos, lineColor, from, to, connId);
        }
        else
        {
            Vector2 pointB = new Vector2(fromPos.x, midY);
            Vector2 pointC = new Vector2(toPos.x, midY);
            CreateLine(fromPos, pointB, lineColor, from, to, connId);
            CreateLine(pointB, pointC, lineColor, from, to, connId);
            CreateLine(pointC, toPos, lineColor, from, to, connId);
        }
    }

    private Vector2 WorldToLocal(RectTransform rt)
    {
        Vector3 worldCenter = rt.TransformPoint(Vector3.zero);
        Vector3 local = _rectTransform.InverseTransformPoint(worldCenter);
        float pivotOffsetY = (_rectTransform.pivot.y - 2f) * _rectTransform.rect.height;
        return new Vector2(local.x, local.y - pivotOffsetY);
    }

    private void CreateLine(Vector2 start, Vector2 end, Color color, RectTransform from, RectTransform to, string connId)
    {
        if ((end - start).sqrMagnitude < 0.01f) return;

        GameObject lineObj = new GameObject("SkillLine");
        lineObj.transform.SetParent(transform, false);
        lineObj.transform.SetAsFirstSibling();

        SkillLine skillLine = new SkillLine
        {
            lineObject = lineObj,
            from = from,
            to = to,
            start = start,
            end = end,
            connectionId = connId
        };
        _spawnedLines.Add(skillLine);

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

    public void RefreshLineColors()
{
    HashSet<string> unlockedConnections = new HashSet<string>();

    foreach (var line in _spawnedLines)
    {
        if (line?.lineObject == null) continue;

        SkillButton fromSkill = line.from?.GetComponent<SkillButton>();
        SkillButton toSkill = line.to?.GetComponent<SkillButton>();

        bool shouldBeRed = fromSkill != null && toSkill != null &&
                           fromSkill.skillState == SkillButton.SkillState.Unlocked &&
                           toSkill.skillState == SkillButton.SkillState.Unlocked;

        if (shouldBeRed)
            unlockedConnections.Add(line.connectionId);
    }

    foreach (string connId in unlockedConnections)
    {
        List<SkillLine> connLines = _spawnedLines.FindAll(l => l.connectionId == connId);

        List<(Vector2 start, Vector2 end, RectTransform from, RectTransform to)> lineData =
            new List<(Vector2, Vector2, RectTransform, RectTransform)>();

        foreach (var line in connLines)
            lineData.Add((line.start, line.end, line.from, line.to));

        foreach (var line in connLines)
        {
            _spawnedLines.Remove(line);
            if (Application.isPlaying)
                Destroy(line.lineObject);
            else
                DestroyImmediate(line.lineObject);
        }

        foreach (var data in lineData)
            CreateLine(data.start, data.end, unlockedLineColor, data.from, data.to, connId);
    }
}

public void AnimateUnlockedLines(RectTransform unlockedSkill)
{
    // Najdi všechna unikátní spojení která patří tomuto skillu
    HashSet<string> connIds = new HashSet<string>();

    foreach (var line in _spawnedLines)
    {
        if (line?.lineObject == null) continue;
        if (line.from != unlockedSkill && line.to != unlockedSkill) continue;

        Image img = line.lineObject.GetComponent<Image>();
        if (img != null && img.color == unlockedLineColor)
            connIds.Add(line.connectionId);
    }

    foreach (string connId in connIds)
    {
        List<SkillLine> connLines = _spawnedLines.FindAll(l => l.connectionId == connId);
        if (connLines.Count > 0)
            StartCoroutine(AnimateConnectionCoroutine(connLines));
    }
}

private IEnumerator AnimateConnectionCoroutine(List<SkillLine> lines)
{
    // Ulož plné délky a nastav šedé pozadí + červenou délku na 0
    List<(RectTransform rt, Image img, float fullLength, GameObject bgLine)> lineInfos =
        new List<(RectTransform, Image, float, GameObject)>();

    foreach (var skillLine in lines)
    {
        if (skillLine?.lineObject == null) continue;

        RectTransform rt = skillLine.lineObject.GetComponent<RectTransform>();
        Image img = skillLine.lineObject.GetComponent<Image>();
        if (rt == null || img == null) continue;

        float fullLength = rt.sizeDelta.x;

        // Vytvoř šedé pozadí pod červenou čárou
        GameObject bgObj = new GameObject("SkillLineBG");
        bgObj.transform.SetParent(skillLine.lineObject.transform.parent, false);
        bgObj.transform.SetSiblingIndex(skillLine.lineObject.transform.GetSiblingIndex());

        Image bgImg = bgObj.AddComponent<Image>();
        bgImg.color = lineColor;
        bgImg.raycastTarget = false;

        RectTransform bgRt = bgObj.GetComponent<RectTransform>();
        bgRt.anchorMin = rt.anchorMin;
        bgRt.anchorMax = rt.anchorMax;
        bgRt.pivot = rt.pivot;
        bgRt.sizeDelta = rt.sizeDelta;
        bgRt.anchoredPosition = rt.anchoredPosition;
        bgRt.localRotation = rt.localRotation;

        // Červená čára začíná na délce 0
        rt.sizeDelta = new Vector2(0f, lineWidth);

        lineInfos.Add((rt, img, fullLength, bgObj));
    }

    // Animuj všechny čáry spojení NAJEDNOU
    float currentLength = 0f;
    float maxLength = 0f;
    foreach (var info in lineInfos)
        if (info.fullLength > maxLength) maxLength = info.fullLength;

    while (currentLength < maxLength)
    {
        currentLength += lineAnimationSpeed * Time.deltaTime;
        currentLength = Mathf.Min(currentLength, maxLength);

        foreach (var info in lineInfos)
        {
            float len = Mathf.Min(currentLength, info.fullLength);
            info.rt.sizeDelta = new Vector2(len, lineWidth);
        }

        yield return null;
    }

    // Ujisti se že všechny čáry mají plnou délku a smaž pozadí
    foreach (var info in lineInfos)
    {
        info.rt.sizeDelta = new Vector2(info.fullLength, lineWidth);
        if (info.bgLine != null)
            Destroy(info.bgLine);
    }
}

    public void RefreshAllAvailability()
    {
        foreach (var root in rootNodes)
        {
            if (root != null)
                RefreshNodeAvailability(root);
        }
    }

    private void RefreshNodeAvailability(SkillNode node)
    {
        if (node.skillButton == null) return;

        foreach (var child in node.children)
        {
            if (child?.skillButton == null) continue;

            SkillButton childBtn = child.skillButton.GetComponent<SkillButton>();
            if (childBtn == null) continue;

            bool allParentsUnlocked = IsAllParentsUnlocked(child);
            childBtn.RefreshAvailability(allParentsUnlocked);

            RefreshNodeAvailability(child);
        }
    }

    private bool IsAllParentsUnlocked(SkillNode targetNode)
    {
        List<SkillNode> parents = new List<SkillNode>();
        FindParents(rootNodes, targetNode, parents);

        // Pokud nemá žádného parenta je to root - vždy dostupný
        if (parents.Count == 0) return true;

        // VŠICHNI parenti musí být Unlocked
        foreach (var parent in parents)
        {
            SkillButton btn = parent.skillButton?.GetComponent<SkillButton>();
            if (btn == null || btn.skillState != SkillButton.SkillState.Unlocked)
                return false;
        }
        return true;
    }

    private void FindParents(List<SkillNode> nodes, SkillNode target, List<SkillNode> parents)
    {
        foreach (var node in nodes)
        {
            if (node == null) continue;
            foreach (var child in node.children)
            {
                if (child == target)
                    parents.Add(node);
            }
            FindParents(node.children, target, parents);
        }
    }

    public void ClearLines()
    {
        foreach (var line in _spawnedLines)
        {
            if (line?.lineObject == null) continue;
#if UNITY_EDITOR
            if (!Application.isPlaying)
                DestroyImmediate(line.lineObject);
            else
#endif
                Destroy(line.lineObject);
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