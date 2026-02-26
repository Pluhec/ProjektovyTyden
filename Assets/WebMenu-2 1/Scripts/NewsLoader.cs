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

        // Seřadíme podle data od nejnovějšího
        items = items
            .OrderByDescending(ParseDateSafe)
            .ToList();

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