using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
    [Header("Canvases")]
    public GameObject mainCanvas;       // Tvůj hlavní HUD
    public GameObject stopCanvas;       // StopMenu (Esc menu)
    public GameObject settingsCanvas;   // Nastavení
    public GameObject skillTreeCanvas;  // Strom dovedností
    public GameObject webCanvas;        // Webové rozhraní

    public GameObject WebButton;

    [Header("UI Animations")]
    public List<AnimatedUIElement> uiElementsToAnimate = new List<AnimatedUIElement>();
    public float animationDuration = 0.5f;

    [System.Serializable]
    public class AnimatedUIElement
    {
        public RectTransform element;
        public Vector2 startPosition; // Pozice, odkud prvek vyjíždí
        public Vector2 targetPosition; // Finální pozice (nyní veřejná, nastavíme přes tlačítko)
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Ujistíme se, že na začátku je zapnutý jen MainCanvas a ostatní jsou vypnuté
        ShowMainCanvasOnly();
        
        // Spustíme úvodní animaci pro MainCanvas prvky
        AnimateElementsIn();
    }

    private void AnimateElementsIn()
    {
        foreach (var item in uiElementsToAnimate)
        {
            if (item.element != null)
            {
                // Nastavíme na startovní pozici
                item.element.anchoredPosition = item.startPosition;
                // Spustíme coroutinu pro plynulý přesun
                StartCoroutine(MoveElement(item.element, item.startPosition, item.targetPosition));
            }
        }
    }

    private IEnumerator MoveElement(RectTransform rect, Vector2 from, Vector2 to)
    {
        float elapsed = 0f;
        while (elapsed < animationDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / animationDuration;
            // Použijeme SmoothStep pro hezčí efekt
            t = Mathf.SmoothStep(0f, 1f, t);
            
            rect.anchoredPosition = Vector2.Lerp(from, to, t);
            yield return null;
        }
        rect.anchoredPosition = to;
    }

    // --- Metody pro tlačítka ---

    public void OpenStopMenu()
    {
        // Vypne MainCanvas a zapne StopCanvas
        mainCanvas.SetActive(false); // Pokud chceš vidět animaci MainCanvasu i při zavření, musel bys ho nechat chvíli aktivní
        stopCanvas.SetActive(true);
        if (settingsCanvas != null) settingsCanvas.SetActive(false);
        skillTreeCanvas.SetActive(false);
        webCanvas.SetActive(false);
    }

    public void OpenSettings()
    {
        mainCanvas.SetActive(false);
        stopCanvas.SetActive(false);
        if (settingsCanvas != null) settingsCanvas.SetActive(true);
        skillTreeCanvas.SetActive(false);
        webCanvas.SetActive(false);
    }

    public void OpenSkillTree()
    {
        // Vypne MainCanvas a zapne SkillTreeCanvas
        mainCanvas.SetActive(false);
        stopCanvas.SetActive(false);
        if (settingsCanvas != null) settingsCanvas.SetActive(false);
        skillTreeCanvas.SetActive(true);
        webCanvas.SetActive(false);
    }

    public void OpenWeb()
    {
        // Vypne MainCanvas a zapne WebCanvas
        mainCanvas.SetActive(false);
        stopCanvas.SetActive(false);
        if (settingsCanvas != null) settingsCanvas.SetActive(false);
        skillTreeCanvas.SetActive(false);
        webCanvas.SetActive(true);
    }

    public void BackToGame()
    {
        // Návrat do hry - zobrazí pouze MainCanvas
        ShowMainCanvasOnly();
        // Znovu spustíme animaci příletu, když se vrátíme do hry
        AnimateElementsIn();
    }

    // Pomocná metoda pro reset stavu
    private void ShowMainCanvasOnly()
    {
        if (mainCanvas != null) mainCanvas.SetActive(true);
        if (stopCanvas != null) stopCanvas.SetActive(false);
        if (settingsCanvas != null) settingsCanvas.SetActive(false);
        if (skillTreeCanvas != null) skillTreeCanvas.SetActive(false);
        if (webCanvas != null) webCanvas.SetActive(false);
    }

    public void ShowWebButton()
    {
        WebButton.SetActive(true);
    }
}
