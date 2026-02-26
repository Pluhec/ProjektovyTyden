using UnityEngine;
using UnityEngine.UI;

public class CloseCanvasPopUp : MonoBehaviour
{
    [SerializeField] private Button closeButton;

    void Start()
    {
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(CloseCanvas);
        }
    }

    public void CloseCanvas()
    {
        gameObject.SetActive(false);
    }

    void OnDestroy()
    {
        if (closeButton != null)
        {
            closeButton.onClick.RemoveListener(CloseCanvas);
        }
    }
}
