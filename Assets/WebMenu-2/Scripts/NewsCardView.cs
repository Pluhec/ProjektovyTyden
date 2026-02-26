 using TMPro;
using UnityEngine;

public class NewsCardView : MonoBehaviour
{
    [SerializeField] private TMP_Text dateText;
    [SerializeField] private TMP_Text headerText;
    [SerializeField] private TMP_Text bodyText;
    [SerializeField] private TMP_Text labelText;

    public void Bind(NewsItem item)
    {
        dateText.text = item.date;
        headerText.text = item.title;
        bodyText.text = item.text;
        labelText.text = string.IsNullOrEmpty(item.label) ? "" : item.label;
    }
}