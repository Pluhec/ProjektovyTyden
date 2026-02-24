using UnityEngine;

public class MainMenuController : MonoBehaviour
{
    [Header("Canvases")]
    public GameObject mainCanvas;       // Tvůj hlavní HUD
    public GameObject stopCanvas;       // StopMenu (Esc menu)
    public GameObject skillTreeCanvas;  // Strom dovedností
    public GameObject webCanvas;        // Webové rozhraní

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Ujistíme se, že na začátku je zapnutý jen MainCanvas a ostatní jsou vypnuté
        ShowMainCanvasOnly();
    }



    // --- Metody pro tlačítka ---

    public void OpenStopMenu()
    {
        // Vypne MainCanvas a zapne StopCanvas
        stopCanvas.SetActive(true);
        skillTreeCanvas.SetActive(false);
        webCanvas.SetActive(false);
    }

    public void OpenSkillTree()
    {
        // Vypne MainCanvas a zapne SkillTreeCanvas
        stopCanvas.SetActive(false);
        skillTreeCanvas.SetActive(true);
        webCanvas.SetActive(false);
    }

    public void OpenWeb()
    {
        // Vypne MainCanvas a zapne WebCanvas
        stopCanvas.SetActive(false);
        skillTreeCanvas.SetActive(false);
        webCanvas.SetActive(true);
    }

    public void BackToGame()
    {
        ShowMainCanvasOnly();
    }

    // Pomocná metoda pro reset stavu
    private void ShowMainCanvasOnly()
    {
        if (mainCanvas != null) mainCanvas.SetActive(true);
        if (stopCanvas != null) stopCanvas.SetActive(false);
        if (skillTreeCanvas != null) skillTreeCanvas.SetActive(false);
        if (webCanvas != null) webCanvas.SetActive(false);
    }
}
