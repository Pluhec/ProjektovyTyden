using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NewsDetailUI : MonoBehaviour
{
    [Header("Root")]
    [SerializeField] private GameObject root;   // sem dej třeba Card nebo celý DetailCanvas content

    [Header("Texts")]
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text dateText;
    [SerializeField] private TMP_Text labelText;
    [SerializeField] private TMP_Text bodyText;

    [Header("Buttons")]
    [SerializeField] private Button closeButton;
    [SerializeField] private Button backgroundButton; // volitelné: klik na pozadí zavře

    private void Awake()
    {
        if (closeButton) closeButton.onClick.AddListener(Hide);
        if (backgroundButton) backgroundButton.onClick.AddListener(Hide);

        Hide(); // ať je to po startu vypnuté
    }

    public void Show(NewsItem item)
    {
        if (item == null) return;

        if (titleText) titleText.text = item.title;
        if (dateText) dateText.text = item.date;
        if (labelText) labelText.text = string.IsNullOrEmpty(item.label) ? "" : item.label;
        if (bodyText) bodyText.text = item.text;

        if (root) root.SetActive(true);
        else gameObject.SetActive(true);
    }

    public void Hide()
    {
        if (root) root.SetActive(false);
        else gameObject.SetActive(false);
    }
}