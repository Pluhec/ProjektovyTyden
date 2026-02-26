using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class NewsCardView : MonoBehaviour
{
    [SerializeField] private TMP_Text dateText;
    [SerializeField] private TMP_Text headerText;
    [SerializeField] private TMP_Text bodyText;
    [SerializeField] private TMP_Text labelText;

    [Header("Open detail")]
    [SerializeField] private Button openButton;

    private NewsItem bound;
    private NewsDetailUI detailUI;

    public void Bind(NewsItem item)
    {
        bound = item;

        dateText.text = item.date;
        headerText.text = item.title;
        bodyText.text = item.text;
        labelText.text = string.IsNullOrEmpty(item.label) ? "" : item.label;

        if (detailUI == null)
            detailUI = Object.FindAnyObjectByType<NewsDetailUI>(); // Unity 2023+
        // detailUI = FindObjectOfType<NewsDetailUI>();        // fallback pro starší Unity

        if (openButton)
        {
            openButton.onClick.RemoveAllListeners();
            openButton.onClick.AddListener(OpenDetail);
        }
    }

    private void OpenDetail()
    {
        Debug.Log("Card clicked: " + (bound != null ? bound.title : "NULL"));
        if (detailUI == null)
        {
            Debug.LogError("NewsDetailUI not found in scene!");
            return;
        }
        detailUI.Show(bound);
    }
}