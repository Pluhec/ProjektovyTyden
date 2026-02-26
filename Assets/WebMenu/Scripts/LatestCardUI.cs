using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LatestCardUI : MonoBehaviour
{
    [SerializeField] private TMP_Text badgeText;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text excerptText;
    [SerializeField] private TMP_Text viewsText;
    [SerializeField] private Button readMoreButton;

    private Article bound;

    public void Bind(Article a)
    {
        bound = a;

        if (badgeText) badgeText.text = (a.category ?? "Aktualita").ToUpper();
        if (titleText) titleText.text = a.title;
        if (excerptText) excerptText.text = a.excerpt;
        if (viewsText) viewsText.text = a.views > 0 ? a.views.ToString() : "";

        if (readMoreButton)
        {
            readMoreButton.onClick.RemoveAllListeners();
            readMoreButton.onClick.AddListener(() =>
            {
                Debug.Log($"Read more: {bound.title}");
                // sem si napojíš otevření detailu (panel, webview, atd.)
            });
        }
    }
}