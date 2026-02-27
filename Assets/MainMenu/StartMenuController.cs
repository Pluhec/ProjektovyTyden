using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StartMenuController : MonoBehaviour
{
    [Header("Scenes")]
    public string gameSceneName = "GameScene"; // Jméno scény, která se má načíst (např. "MainCanvas" nebo "Scenes/Level1")

    [Header("Canvases/Panels")]
    public GameObject startMenuCanvas;  // Hlavní panel start menu
    public GameObject optionsCanvas;    // Panel nastavení

    [Header("UI Animations")]
    public float animationDuration = 0.5f;
    public List<AnimatedUIElement> startMenuElements = new List<AnimatedUIElement>();
    public List<AnimatedUIElement> optionsMenuElements = new List<AnimatedUIElement>();

    [System.Serializable]
    public class AnimatedUIElement
    {
        public RectTransform element;
        public Vector2 startPosition; 
        public Vector2 targetPosition;
    }

    private void Start()
    {
        // Ujistíme se, že zobrazujeme správný panel
        ShowStartMenu();
        if (optionsCanvas != null) optionsCanvas.SetActive(false);
    }

    private void AnimateElements(List<AnimatedUIElement> elements)
    {
        foreach (var item in elements)
        {
            if (item.element != null)
            {
                item.element.anchoredPosition = item.startPosition;
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
            t = Mathf.SmoothStep(0f, 1f, t);
            
            rect.anchoredPosition = Vector2.Lerp(from, to, t);
            yield return null;
        }
        rect.anchoredPosition = to;
    }

    // --- Metody pro tlačítka ---

    public void PlayGame()
    {
        // Načte herní scénu
        // Ujisti se, že scéna je přidaná v File -> Build Settings!
           // Load the game scene instead of activating a canvas
           SceneManager.LoadScene(gameSceneName);
    }

    public void OpenOptions()
    {
        if (startMenuCanvas != null) startMenuCanvas.SetActive(false);
        if (optionsCanvas != null) 
        {
            optionsCanvas.SetActive(true);
            AnimateElements(optionsMenuElements);
        }
    }

    public void CloseOptions()
    {
        if (startMenuCanvas != null)
        {
            startMenuCanvas.SetActive(true);
            AnimateElements(startMenuElements);
        }
        if (optionsCanvas != null) optionsCanvas.SetActive(false);
    }

    public void QuitGame()
    {
        Debug.Log("Quit Game requested");
        Application.Quit();
        
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }

    private void ShowStartMenu()
    {
        if (startMenuCanvas != null) 
        {
            startMenuCanvas.SetActive(true);
            AnimateElements(startMenuElements);
        }
        if (optionsCanvas != null) optionsCanvas.SetActive(false);
    }
}
