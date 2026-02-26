using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class NewsManager : MonoBehaviour
{
    public event Action Changed;

    [Header("Rules")]
    [SerializeField] private int latestCount = 3;
    [SerializeField] private int archivePageSize = 5;

    [Header("Demo")]
    [SerializeField] private bool seedDemoOnStart = true;

    public readonly List<Article> Articles = new();
    public int CurrentArchivePage { get; private set; }

    private void Start()
    {
        if (seedDemoOnStart && Articles.Count == 0)
            SeedDemo();
    }

    public void AddArticle(string title, string excerpt, string category = "Aktualita", int views = 0)
    {
        Articles.Insert(0, new Article(title, excerpt, DateTime.Now, category, views));
        CurrentArchivePage = 0;
        Changed?.Invoke();
    }

    public List<Article> GetLatest() => Articles.Take(latestCount).ToList();

    public int GetArchiveCount() => Mathf.Max(0, Articles.Count - latestCount);

    public int GetArchivePageCount()
    {
        int count = GetArchiveCount();
        return Mathf.Max(1, Mathf.CeilToInt(count / (float)archivePageSize));
    }

    public List<Article> GetArchivePage(int page)
    {
        int pageCount = GetArchivePageCount();
        page = Mathf.Clamp(page, 0, pageCount - 1);

        int skip = latestCount + page * archivePageSize;
        return Articles.Skip(skip).Take(archivePageSize).ToList();
    }

    public void SetPage(int page)
    {
        CurrentArchivePage = Mathf.Clamp(page, 0, GetArchivePageCount() - 1);
        Changed?.Invoke();
    }

    public void NextPage()
    {
        SetPage(CurrentArchivePage + 1);
    }

    public void PrevPage()
    {
        SetPage(CurrentArchivePage - 1);
    }

    [ContextMenu("Seed Demo")]
    public void SeedDemo()
    {
        Articles.Clear();

        // 3 aktuality
        Articles.Add(new Article("HNS zahajuje sérii debat o bezpečnosti regionů", "Strana zahájila sérii debat napříč regiony…", DateTime.Now.AddDays(-1), "Aktualita", 20));
        Articles.Add(new Article("Úspěšná kampaň za bezpečnější města", "Kampaň přinesla konkrétní kroky a výsledky…", DateTime.Now.AddDays(-3), "Aktualita", 12));
        Articles.Add(new Article("Emanuel Verdi představil vizi dlouhodobé stability", "Představení priorit a dlouhodobých opatření…", DateTime.Now.AddDays(-7), "Aktualita", 9));

        // archiv (přidej víc než 5, ať vidíš stránkování)
        for (int i = 0; i < 11; i++)
        {
            Articles.Add(new Article(
                $"Archivní článek {i + 1}",
                "Krátký popis archivního článku…",
                DateTime.Now.AddDays(-(14 + i * 3)),
                "Aktualita",
                0
            ));
        }

        // seřaď od nejnovějšího
        Articles.Sort((a, b) => b.date.CompareTo(a.date));
        CurrentArchivePage = 0;
        Changed?.Invoke();
    }
}