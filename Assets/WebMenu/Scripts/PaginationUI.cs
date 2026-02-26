using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PaginationUI : MonoBehaviour
{
    [SerializeField] private Button prevButton;
    [SerializeField] private Button nextButton;
    [SerializeField] private Transform pagesContainer;
    [SerializeField] private GameObject pageButtonTemplate; // inactive template with Button+TMP
    [SerializeField] private TMP_Text pageInfoText;

    private NewsManager manager;

    public void Init(NewsManager m)
    {
        manager = m;

        if (prevButton)
        {
            prevButton.onClick.RemoveAllListeners();
            prevButton.onClick.AddListener(() => manager.PrevPage());
        }

        if (nextButton)
        {
            nextButton.onClick.RemoveAllListeners();
            nextButton.onClick.AddListener(() => manager.NextPage());
        }
    }

    public void Rebuild(int pageCount, int currentPage)
    {
        if (!pagesContainer || !pageButtonTemplate) return;

        // clear old buttons (keep template)
        for (int i = pagesContainer.childCount - 1; i >= 0; i--)
        {
            var ch = pagesContainer.GetChild(i);
            if (ch.gameObject == pageButtonTemplate) continue;
            Destroy(ch.gameObject);
        }

        // create 1..N
        for (int i = 0; i < pageCount; i++)
        {
            var go = Instantiate(pageButtonTemplate, pagesContainer);
            go.SetActive(true);

            int pageIndex = i;
            var btn = go.GetComponent<Button>();
            var txt = go.GetComponentInChildren<TMP_Text>();

            if (txt) txt.text = (i + 1).ToString();

            if (btn)
            {
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(() => manager.SetPage(pageIndex));
            }

            // jednoduchý highlight active (změň barvy dle stylu)
            var img = go.GetComponent<Image>();
            if (img) img.enabled = true;
            if (img) img.color = (pageIndex == currentPage) ? new Color(0.2f, 0.55f, 1f, 1f) : new Color(1f, 1f, 1f, 0.9f);
            if (txt) txt.color = (pageIndex == currentPage) ? Color.white : new Color(0.15f, 0.15f, 0.15f, 1f);
        }

        if (pageInfoText) pageInfoText.text = $"{currentPage + 1} / {pageCount}";

        if (prevButton) prevButton.interactable = currentPage > 0;
        if (nextButton) nextButton.interactable = currentPage < pageCount - 1;
    }
}