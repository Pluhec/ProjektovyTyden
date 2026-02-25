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

        // Debug - vypis všechny nody a jejich počet parentů
        if (Application.isPlaying)
            DebugPrintParents();
    }

    private void DebugPrintParents()
    {
        Queue<SkillNode> queue = new Queue<SkillNode>();
        HashSet<SkillNode> enqueued = new HashSet<SkillNode>();

        foreach (var root in rootNodes)
        {
            if (root == null || enqueued.Contains(root)) continue;
            queue.Enqueue(root);
            enqueued.Add(root);
        }

        while (queue.Count > 0)
        {
            SkillNode current = queue.Dequeue();
            if (current?.skillButton == null) continue;

            List<SkillNode> parents = new List<SkillNode>();
            FindParents(rootNodes, current, parents);

            string parentNames = parents.Count == 0 ? "žádný" :
                string.Join(", ", parents.ConvertAll(p => p.skillButton?.name ?? "null"));

            Debug.Log($"Node: {current.skillButton.name} | Počet parentů: {parents.Count} | Parenti: {parentNames}");

            foreach (var child in current.children)
            {
                if (child == null || enqueued.Contains(child)) continue;
                queue.Enqueue(child);
                enqueued.Add(child);
            }
        }
    }

    private void OnDisable()
    {
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
        List<(RectTransform rt, float fullLength, GameObject bgLine)> lineInfos =
            new List<(RectTransform, float, GameObject)>();

        foreach (var skillLine in lines)
        {
            if (skillLine?.lineObject == null) continue;

            RectTransform rt = skillLine.lineObject.GetComponent<RectTransform>();
            Image img = skillLine.lineObject.GetComponent<Image>();
            if (rt == null || img == null) continue;

            float fullLength = rt.sizeDelta.x;

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
            bgRt.sizeDelta = new Vector2(fullLength, lineWidth);
            bgRt.anchoredPosition = rt.anchoredPosition;
            bgRt.localRotation = rt.localRotation;

            rt.sizeDelta = new Vector2(0f, lineWidth);
            lineInfos.Add((rt, fullLength, bgObj));
        }

        float maxLength = 0f;
        foreach (var info in lineInfos)
            if (info.fullLength > maxLength) maxLength = info.fullLength;

        float elapsed = 0f;
        float duration = maxLength / lineAnimationSpeed;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            foreach (var info in lineInfos)
            {
                float len = Mathf.Lerp(0f, info.fullLength, t);
                info.rt.sizeDelta = new Vector2(len, lineWidth);
            }

            yield return null;
        }

        foreach (var info in lineInfos)
        {
            info.rt.sizeDelta = new Vector2(info.fullLength, lineWidth);
            if (info.bgLine != null)
                Destroy(info.bgLine);
        }
    }

    public void RefreshAllAvailability()
    {
        // BFS - každý node se zpracuje jednou ve správném pořadí (parenti před childy)
        Queue<SkillNode> queue = new Queue<SkillNode>();
        HashSet<SkillNode> enqueued = new HashSet<SkillNode>();

        foreach (var root in rootNodes)
        {
            if (root == null || enqueued.Contains(root)) continue;
            queue.Enqueue(root);
            enqueued.Add(root);
        }

        while (queue.Count > 0)
        {
            SkillNode current = queue.Dequeue();
            if (current?.skillButton == null) continue;

            SkillButton btn = current.skillButton.GetComponent<SkillButton>();
            if (btn != null && btn.skillState != SkillButton.SkillState.Unlocked)
            {
                bool allParentsUnlocked = IsAllParentsUnlocked(current);
                btn.RefreshAvailability(allParentsUnlocked);
            }

            foreach (var child in current.children)
            {
                if (child == null || enqueued.Contains(child)) continue;
                queue.Enqueue(child);
                enqueued.Add(child);
            }
        }
    }

    private bool IsAllParentsUnlocked(SkillNode targetNode)
    {
        List<SkillNode> parents = new List<SkillNode>();
        FindParents(rootNodes, targetNode, parents);

        if (parents.Count == 0) return true;

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
        // Projdi každý node ve stromu a zkontroluj jestli má target mezi svými childy
        // Pokud ano, tento node je parentem targetu
        foreach (var node in nodes)
        {
            if (node == null) continue;
        
            foreach (var child in node.children)
            {
                // Porovnáváme podle RectTransform reference (stejný GameObject = stejný button)
                if (child?.skillButton != null && target?.skillButton != null &&
                    child.skillButton == target.skillButton &&
                    !parents.Contains(node))
                {
                    parents.Add(node);
                }
            }
        
            // Rekurzivně projdi childy
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