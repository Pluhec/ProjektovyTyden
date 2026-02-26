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

    private GameObject currentCanvas;
    private Coroutine transitionCoroutine;
    private Dictionary<RectTransform, Coroutine> activeCoroutines = new Dictionary<RectTransform, Coroutine>();

    void Start()
    {
        // Initialize all elements to their start positions
        InitializeElements(mainCanvasElements);
        InitializeElements(stopCanvasElements);
        InitializeElements(settingsCanvasElements);
        InitializeElements(skillTreeCanvasElements);
        InitializeElements(webCanvasElements);

        // Disable all canvases except main
        if (stopCanvas != null) stopCanvas.SetActive(false);
        if (settingsCanvas != null) settingsCanvas.SetActive(false);
        if (skillTreeCanvas != null) skillTreeCanvas.SetActive(false);
        if (webCanvas != null) webCanvas.SetActive(false);

        if (mainCanvas != null) mainCanvas.SetActive(true);
        currentCanvas = mainCanvas;

        // Animate main canvas in
        foreach (var item in mainCanvasElements)
        {
            if (item.element != null)
            {
                StartMoveRoutine(item.element, item.targetPosition);
            }
        }
    }

    private void InitializeElements(List<AnimatedUIElement> elements)
    {
        if (elements == null) return;
        foreach (var item in elements)
        {
            if (item.element != null)
            {
                item.element.anchoredPosition = item.startPosition;
            }
        }
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
        if (currentCanvas == activeCanvas) return;

        if (transitionCoroutine != null)
        {
            StopCoroutine(transitionCoroutine);
        }
        
        transitionCoroutine = StartCoroutine(TransitionCanvas(activeCanvas, animationsToPlay));
    }

    private IEnumerator TransitionCanvas(GameObject newCanvas, List<AnimatedUIElement> newElements)
    {
        GameObject previousCanvas = currentCanvas;
        List<AnimatedUIElement> previousElements = GetElementsForCanvas(previousCanvas);

        currentCanvas = newCanvas;

        // 1. Animate out previous elements and fade out background
        if (previousElements != null)
        {
            foreach (var item in previousElements)
            {
                if (item.element != null)
                {
                    StartMoveRoutine(item.element, item.startPosition);
                }
            }
        }
        
        if (previousCanvas != null)
        {
            StartFadeRoutine(previousCanvas, 0f);
        }

        // 2. Enable new canvas, fade in background, and animate in elements
        if (newCanvas != null)
        {
            newCanvas.SetActive(true);
            StartFadeRoutine(newCanvas, 1f);
            
            if (newElements != null)
            {
                foreach (var item in newElements)
                {
                    if (item.element != null)
                    {
                        StartMoveRoutine(item.element, item.targetPosition);
                    }
                }
            }
        }

        // 3. Wait for animations to finish (using unscaled time in case game is paused)
        yield return new WaitForSecondsRealtime(animationDuration);

        // 4. Disable previous canvas if it's not the new one
        // IMPORTANT: Never disable the canvas this script is attached to (usually mainCanvas), 
        // otherwise all coroutines will stop and the script will break!
        if (previousCanvas != null && previousCanvas != newCanvas)
        {
            if (previousCanvas != this.gameObject && previousCanvas != mainCanvas)
            {
                previousCanvas.SetActive(false);
            }
        }
    }

    private List<AnimatedUIElement> GetElementsForCanvas(GameObject canvas)
    {
        if (canvas == mainCanvas) return mainCanvasElements;
        if (canvas == stopCanvas) return stopCanvasElements;
        if (canvas == settingsCanvas) return settingsCanvasElements;
        if (canvas == skillTreeCanvas) return skillTreeCanvasElements;
        if (canvas == webCanvas) return webCanvasElements;
        return null;
    }

    private void StartMoveRoutine(RectTransform rect, Vector2 to)
    {
        if (rect == null) return;
        
        if (activeCoroutines.ContainsKey(rect) && activeCoroutines[rect] != null)
        {
            StopCoroutine(activeCoroutines[rect]);
        }
        
        activeCoroutines[rect] = StartCoroutine(MoveElement(rect, rect.anchoredPosition, to));
    }

    private Dictionary<GameObject, Coroutine> activeFadeCoroutines = new Dictionary<GameObject, Coroutine>();

    private void StartFadeRoutine(GameObject canvas, float targetAlpha)
    {
        if (canvas == null) return;
        
        CanvasGroup canvasGroup = canvas.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = canvas.AddComponent<CanvasGroup>();
        }

        if (activeFadeCoroutines.ContainsKey(canvas) && activeFadeCoroutines[canvas] != null)
        {
            StopCoroutine(activeFadeCoroutines[canvas]);
        }

        activeFadeCoroutines[canvas] = StartCoroutine(FadeCanvasGroup(canvasGroup, targetAlpha));
    }

    private IEnumerator FadeCanvasGroup(CanvasGroup cg, float targetAlpha)
    {
        float startAlpha = cg.alpha;
        float elapsed = 0f;

        while (elapsed < animationDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / animationDuration;
            
            // Smooth EaseOutCubic for fading
            float easeT = 1f - Mathf.Pow(1f - t, 3f);
            
            cg.alpha = Mathf.Lerp(startAlpha, targetAlpha, easeT);
            yield return null;
        }
        
        cg.alpha = targetAlpha;
    }

    private IEnumerator MoveElement(RectTransform rect, Vector2 from, Vector2 to)
    {
        float elapsed = 0f;
        while (elapsed < animationDuration)
        {
            // Use unscaledDeltaTime so animations work even if Time.timeScale is 0 (paused)
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / animationDuration;
            
            // Smooth EaseOutCubic for a very nice, premium feel
            float easeT = 1f - Mathf.Pow(1f - t, 3f);
            
            rect.anchoredPosition = Vector2.Lerp(from, to, easeT);
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
