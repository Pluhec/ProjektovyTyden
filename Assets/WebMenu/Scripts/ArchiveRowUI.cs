using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ArchiveRowUI : MonoBehaviour
{
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text excerptText;
    [SerializeField] private TMP_Text dateText;
    [SerializeField] private Button readMoreButton;

    private Article bound;

    public void Bind(Article a)
    {
        bound = a;
        if (titleText) titleText.text = a.title;
        if (excerptText) excerptText.text = a.excerpt;
        if (dateText) dateText.text = a.date.ToString("dd. MM. yyyy");

        if (readMoreButton)
        {
            readMoreButton.onClick.RemoveAllListeners();
            readMoreButton.onClick.AddListener(() => Debug.Log($"Read more: {bound.title}"));
        }
    }
}