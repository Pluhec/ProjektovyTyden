using TMPro;
using UnityEngine;

public class NewsUI : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private NewsManager manager;

    [Header("Latest")]
    [SerializeField] private Transform latestContainer; // GridLayoutGroup
    [SerializeField] private GameObject latestCardTemplate; // inactive

    [Header("Archive")]
    [SerializeField] private Transform archiveContainer; // VerticalLayoutGroup
    [SerializeField] private GameObject archiveRowTemplate; // inactive

    [Header("Pagination")]
    [SerializeField] private PaginationUI pagination;

    [Header("Optional")]
    [SerializeField] private TMP_Text titleAktuality;
    [SerializeField] private TMP_Text titleArchiv;

    private void Awake()
    {
        if (!manager) manager = FindObjectOfType<NewsManager>();
        if (manager) manager.Changed += Refresh;
        if (pagination && manager) pagination.Init(manager);

        if (titleAktuality) titleAktuality.text = "AKTUALITY";
        if (titleArchiv) titleArchiv.text = "ARCHIV";
    }

    private void OnDestroy()
    {
        if (manager) manager.Changed -= Refresh;
    }

    private void Start()
    {
        Refresh();
    }

    public void Refresh()
    {
        if (!manager) return;

        ClearGenerated(latestContainer, latestCardTemplate);
        ClearGenerated(archiveContainer, archiveRowTemplate);

        // Latest (3)
        foreach (var a in manager.GetLatest())
        {
            var go = Instantiate(latestCardTemplate, latestContainer);
            go.SetActive(true);
            go.GetComponent<LatestCardUI>().Bind(a);
        }

        // Archive page
        var page = manager.CurrentArchivePage;
        foreach (var a in manager.GetArchivePage(page))
        {
            var go = Instantiate(archiveRowTemplate, archiveContainer);
            go.SetActive(true);
            go.GetComponent<ArchiveRowUI>().Bind(a);
        }

        // Pagination
        if (pagination)
            pagination.Rebuild(manager.GetArchivePageCount(), manager.CurrentArchivePage);
    }

    private void ClearGenerated(Transform container, GameObject template)
    {
        if (!container) return;
        for (int i = container.childCount - 1; i >= 0; i--)
        {
            var ch = container.GetChild(i).gameObject;
            if (template && ch == template) continue;
            Destroy(ch);
        }
    }
}