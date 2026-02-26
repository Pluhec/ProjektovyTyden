using UnityEngine;
using UnityEngine.UI;

public class UIMapInteraction : MonoBehaviour
{
    private Material mapMaterial;
    private RectTransform rectTransform;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        // UI Image používá sdílený materiál, vytvoøíme kopii, aby neovlivnil ostatní UI
        mapMaterial = Instantiate(GetComponent<Image>().material);
        GetComponent<Image>().material = mapMaterial;
    }

    void Update()
    {
        Vector2 localPoint;
        // Pøevod pozice myši na lokální bod v rámci UI elementu
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, Input.mousePosition, null, out localPoint))
        {
            // Pøevod z lokálních souøadnic na 0.0 až 1.0 (UV)
            float uvX = (localPoint.x - rectTransform.rect.x) / rectTransform.rect.width;
            float uvY = (localPoint.y - rectTransform.rect.y) / rectTransform.rect.height;

            mapMaterial.SetVector("_MouseUV", new Vector4(uvX, uvY, 0, 0));
        }
    }
}