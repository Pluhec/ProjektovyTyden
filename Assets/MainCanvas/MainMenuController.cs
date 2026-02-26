using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    [Header("Canvases")]
    public GameObject mainCanvas;       // Tvůj hlavní HUD
    public GameObject stopCanvas;       // StopMenu (Esc menu)
    public GameObject settingsCanvas;   // Nastavení
    public GameObject skillTreeCanvas;  // Strom dovedností
    public GameObject webCanvas;        // Webové rozhraní


    public GameObject webButton; // CamelCase pro field

    [Header("Animation Settings")]
    public float animationDuration = 0.5f;
    
    
    public string gameSceneName = "MainMenu";

    [System.Serializable]
    public class AnimatedUIElement
    {
        public RectTransform element;
        public Vector2 startPosition; 
        public Vector2 targetPosition; 
    }

    [Header("Animations per Canvas")]
    public List<AnimatedUIElement> mainCanvasElements = new List<AnimatedUIElement>();
    public List<AnimatedUIElement> stopCanvasElements = new List<AnimatedUIElement>();
    public List<AnimatedUIElement> settingsCanvasElements = new List<AnimatedUIElement>();
    public List<AnimatedUIElement> skillTreeCanvasElements = new List<AnimatedUIElement>();
    public List<AnimatedUIElement> webCanvasElements = new List<AnimatedUIElement>();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Ujistíme se, že na začátku je zapnutý jen MainCanvas a ostatní jsou vypnuté
        ShowMainCanvasOnly();
    }

    // --- Metody pro tlačítka ---

    public void OpenStopMenu()
    {
        ConsoleOpenCanvas(stopCanvas, stopCanvasElements);
    }

    public void OpenSettings()
    {
        ConsoleOpenCanvas(settingsCanvas, settingsCanvasElements);
    }

    public void OpenSkillTree()
    {
        ConsoleOpenCanvas(skillTreeCanvas, skillTreeCanvasElements);
    }

    public void OpenWeb()
    {
        ConsoleOpenCanvas(webCanvas, webCanvasElements);
    }

    public void BackToGame()
    {
        ConsoleOpenCanvas(mainCanvas, mainCanvasElements);
    }

    public void ShowWebButton()
    {
        if (webButton != null) webButton.SetActive(true);
    }
    
    // Hlavní metoda pro přepínání
    private void ConsoleOpenCanvas(GameObject activeCanvas, List<AnimatedUIElement> animationsToPlay)
    {
        // 1. Vypnout vše
        if (stopCanvas != null) stopCanvas.SetActive(false);
        if (settingsCanvas != null) settingsCanvas.SetActive(false);
        if (skillTreeCanvas != null) skillTreeCanvas.SetActive(false);
        if (webCanvas != null) webCanvas.SetActive(false);

        // 2. Zapnout ten správný
        if (activeCanvas != null)
        {
            activeCanvas.SetActive(true);
            AnimateElements(animationsToPlay);
        }
    }

    private void ShowMainCanvasOnly()
    {
         ConsoleOpenCanvas(mainCanvas, mainCanvasElements);
    }

    private void AnimateElements(List<AnimatedUIElement> elements)
    {
        foreach (var item in elements)
        {
            if (item.element != null)
            {
                // Reset na startovní pozici
                item.element.anchoredPosition = item.startPosition;
                // Spuštění animace
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
    
    public void BackToMenu()
    {
        // Načte herní scénu
        // Ujisti se, že scéna je přidaná v File -> Build Settings!
        SceneManager.LoadScene(gameSceneName);
    }
}
