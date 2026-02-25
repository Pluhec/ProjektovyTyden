using UnityEngine;
using UnityEngine.SceneManagement;

public class StartMenuController : MonoBehaviour
{
    [Header("Scenes")]
    public string gameSceneName = "GameScene"; // Jméno scény, která se má načíst (např. "MainCanvas" nebo "Scenes/Level1")

    [Header("Canvases/Panels")]
    public GameObject startMenuCanvas;  // Hlavní panel start menu
    public GameObject optionsCanvas;    // Panel nastavení

    private void Start()
    {
        // Ujistíme se, že zobrazujeme správný panel
        ShowStartMenu();
        if (optionsCanvas != null) optionsCanvas.SetActive(false);
    }

    // --- Metody pro tlačítka ---

    public void PlayGame()
    {
        // Načte herní scénu
        // Ujisti se, že scéna je přidaná v File -> Build Settings!
        SceneManager.LoadScene(gameSceneName);
    }

    public void OpenOptions()
    {
        if (startMenuCanvas != null) startMenuCanvas.SetActive(false);
        if (optionsCanvas != null) optionsCanvas.SetActive(true);
    }

    public void CloseOptions()
    {
        if (startMenuCanvas != null) startMenuCanvas.SetActive(true);
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
        if (startMenuCanvas != null) startMenuCanvas.SetActive(true);
        if (optionsCanvas != null) optionsCanvas.SetActive(false);
    }
}

