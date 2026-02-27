using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

public class NewsLoader : MonoBehaviour
{
    [Header("Data source")]
    [SerializeField] private bool useTestData = true;
    [SerializeField] private string url;
    [SerializeField] private TextAsset localJson;

    [Header("UI targets")]
    [SerializeField] private Transform aktualityContent;
    [SerializeField] private Transform archivContent;

    [Header("Prefabs")]
    [SerializeField] private GameObject aktualitaPrefab;
    [SerializeField] private GameObject archivPrefab;

    [Header("Settings")]
    [SerializeField] private int aktualityCount = 3;

    /// <summary>
    /// Loads news once when the web page opens.
    /// News is filtered based on the current political stance at this moment.
    /// The news will NOT update while the page is open - only when closed and reopened.
    /// </summary>
    private void Start()
    {
        if (useTestData)
        {
            LoadFromList(NewsTestData.Get10());
            return;
        }

        if (localJson != null)
        {
            LoadFromJson(localJson.text);
        }
        else if (!string.IsNullOrEmpty(url))
        {
            StartCoroutine(LoadFromUrl());
        }
        else
        {
            Debug.LogWarning("NewsLoader: Není nastavený žádný zdroj dat.");
        }
    }

    private System.Collections.IEnumerator LoadFromUrl()
    {
        using UnityWebRequest req = UnityWebRequest.Get(url);
        yield return req.SendWebRequest();

        if (req.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("News load failed: " + req.error);
            yield break;
        }

        LoadFromJson(req.downloadHandler.text);
    }

    private void LoadFromJson(string json)
    {
        NewsResponse response = JsonUtility.FromJson<NewsResponse>(json);

        if (response == null || response.items == null)
        {
            Debug.LogError("JSON je prázdný nebo špatně naformátovaný.");
            return;
        }

        LoadFromList(response.items.ToList());
    }

    private void LoadFromList(List<NewsItem> items)
    {
        if (aktualityContent == null || archivContent == null)
        {
            Debug.LogError("NewsLoader: Není nastavený AktualityContent nebo ArchivContent.");
            return;
        }

        if (aktualitaPrefab == null || archivPrefab == null)
        {
            Debug.LogError("NewsLoader: Není nastavený prefab.");
            return;
        }

        Clear(aktualityContent);
        Clear(archivContent);

        // Get current political stance from simulation (0-100% where higher = more totalitarian)
        float currentStance = GetCurrentStance();
        Debug.Log($"NewsLoader: Totalitarian percentage = {currentStance:F1}% (showing news with minStance <= {currentStance:F1}%)");

        // Filter news based on current stance - only show news appropriate for current political climate
        items = items
            .Where(item => item.minStance <= currentStance)
            .OrderByDescending(ParseDateSafe)
            .ToList();

        Debug.Log($"NewsLoader: {items.Count} news items available for current stance");

        for (int i = 0; i < items.Count; i++)
        {
            bool isAktualita = i < aktualityCount;

            Transform parent = isAktualita ? aktualityContent : archivContent;
            GameObject prefab = isAktualita ? aktualitaPrefab : archivPrefab;

            GameObject go = Instantiate(prefab, parent, false);

            NewsCardView view = go.GetComponent<NewsCardView>();
            if (view == null)
            {
                Debug.LogError(prefab.name + " nemá komponentu NewsCardView.");
                continue;
            }

            view.Bind(items[i]);
        }
    }

    /// <summary>
    /// Gets the current political stance from the simulation.
    /// Returns a percentage (0-100%) where higher = more totalitarian.
    /// </summary>
    private float GetCurrentStance()
    {
        SimulationHandler simHandler = FindObjectOfType<SimulationHandler>();
        if (simHandler == null)
        {
            Debug.LogWarning("NewsLoader: SimulationHandler not found! Defaulting to democratic stance (0%).");
            return 0f;
        }

        // Read the same value the progress bar shows (0-100)
        float totalitarianPercentage = simHandler.GetStancePercentage();

        return totalitarianPercentage;
    }

    private static DateTime ParseDateSafe(NewsItem item)
    {
        if (item == null || string.IsNullOrEmpty(item.date))
            return DateTime.MinValue;

        if (DateTime.TryParse(
                item.date,
                CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeUniversal,
                out DateTime dt))
        {
            return dt;
        }

        return DateTime.MinValue;
    }

    private static void Clear(Transform parent)
    {
        for (int i = parent.childCount - 1; i >= 0; i--)
        {
            Destroy(parent.GetChild(i).gameObject);
        }
    }
}