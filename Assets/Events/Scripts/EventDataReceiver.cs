using UnityEngine;
using UnityEngine.Video;
using TMPro;
using PlayerChoice.DataSets;

/// <summary>
/// Přijímá data od logiky týmu - cestu k videu a text k zobrazení.
/// Obsahuje simulaci pro testování.
/// </summary>
public class EventDataReceiver : MonoBehaviour
{
    [Header("=== REFERENCE ===")]
    [SerializeField] private VideoPlayer videoPlayer;
    [SerializeField] private TextMeshProUGUI eventText;

    [Header("=== SIMULACE (pro testování) ===")]
    [Tooltip("Zapni pro automatické testování při startu")]
    [SerializeField] private bool simulateOnStart = true;

    [Header("=== DEBUG ===")]
    [Tooltip("Zobrazí detailní debug informace v Console")]
    [SerializeField] private bool enableDebug = true;

    // Základní cesta k videím
    private const string VIDEO_BASE_PATH = "Assets/Video/";

    // Dostupná videa (16-30, kromě 26)
    private int[] availableVideos = { 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 27, 28, 29, 30 };

    // Ukázkové texty pro simulaci (4 věty každý)
    private string[] sampleTexts = {
        "Vaše firma stojí před důležitým rozhodnutím. Nový investor nabízí velkou částku, ale požaduje kontrolu nad projektem. Tým je rozdělený a čas tlačí. Jak se rozhodnete?",
        "Nastala neočekávaná krize v dodavatelském řetězci. Klíčový partner oznámil ukončení spolupráce. Zákazníci čekají na dodávky a reputace firmy je v sázce. Co uděláte jako první?",
        "Konkurence právě představila revoluční produkt. Vaše pozice na trhu je ohrožena a tým potřebuje jasné vedení. Máte tři možnosti jak reagovat. Která cesta je ta správná?",
        "Jeden z vašich klíčových zaměstnanců dostal nabídku od konkurence. Jeho odchod by znamenal ztrátu důležitých znalostí. Nabízí se několik řešení situace. Jak si ho udržíte?"
    };

    void Start()
    {
        if (simulateOnStart)
        {
            Log("========================================");
            Log("SIMULACE AKTIVNÍ - Generuji testovací data");
            Log("========================================");
            
            SimulateLogicTeamData();
        }

		DataFunctions.SetEventDataReceiver(this);
    }

    /// <summary>
    /// SIMULACE: Vygeneruje náhodná data jako by přišla od logiky
    /// </summary>
    public void SimulateLogicTeamData()
    {
        // Náhodné video
        int randomIndex = Random.Range(0, availableVideos.Length);
        int videoNumber = availableVideos[randomIndex];
        string videoPath = $"{VIDEO_BASE_PATH}{videoNumber}.mp4";

        // Náhodný text
        string randomText = sampleTexts[Random.Range(0, sampleTexts.Length)];

        Log("--- SIMULACE: Data od logiky ---");
        Log($"Video path: {videoPath}");
        Log($"Text: {randomText}");
        Log("--------------------------------");

        // Nastav data
		//MARK: No longer valid vay of display
        //SetEventData(videoPath, randomText);
    }

    /// <summary>
    /// HLAVNÍ METODA: Volej tuto metodu když přijdou data od logiky týmu
    /// </summary>
    /// <param name="videoPath">Cesta k videu (např. "Assets/Video/23.mp4")</param>
    /// <param name="displayText">Text který se má zobrazit</param>
    public void SetEventData(EventInfo eventInfo)
    {
        Log("========================================");
        Log("PŘIJATA DATA OD LOGIKY");
        Log("========================================");

        // Nastav video
		if(eventInfo.VideoPath.Length > 1)
		{
			SetVideo(eventInfo.VideoPath);
		}

        // Nastav text
		if(eventInfo.EventDescription.Length > 1)
		{
			SetText(eventInfo.EventDescription);
		}

        Log("========================================");
    }

    /// <summary>
    /// Nastaví video podle cesty
    /// </summary>
    private void SetVideo(string videoPath)
    {
        if (videoPlayer == null)
        {
            LogError("VideoPlayer není přiřazen v Inspectoru!");
            return;
        }

        if (string.IsNullOrEmpty(videoPath))
        {
            LogError("Cesta k videu je prázdná!");
            return;
        }

        // Převeď na absolutní cestu
        string absolutePath = System.IO.Path.Combine(
            Application.dataPath, 
            videoPath.Replace("Assets/", "")
        );
        absolutePath = absolutePath.Replace("/", System.IO.Path.DirectorySeparatorChar.ToString());

        // Zkontroluj jestli soubor existuje
        if (!System.IO.File.Exists(absolutePath))
        {
            LogError($"Video soubor neexistuje: {absolutePath}");
            return;
        }

        // Extrahuj název videa
        string videoName = System.IO.Path.GetFileName(videoPath);

        Log($"VIDEO: {videoName}");
        Log($"Cesta: {absolutePath}");

        // Nastav VideoPlayer
        videoPlayer.source = VideoSource.Url;
        videoPlayer.url = absolutePath;
        videoPlayer.Prepare();

        Log("Video připraveno k přehrání");
    }

    /// <summary>
    /// Nastaví text k zobrazení
    /// </summary>
    private void SetText(string text)
    {
        if (eventText == null)
        {
            LogError("EventText (TextMeshProUGUI) není přiřazen v Inspectoru!");
            return;
        }

        if (string.IsNullOrEmpty(text))
        {
            LogWarning("Text je prázdný");
            text = "";
        }

        eventText.text = text;
        Log($"TEXT: {text}");
    }

    /// <summary>
    /// Alternativní metoda - nastaví jen video podle čísla
    /// </summary>
    public void SetVideoByNumber(int videoNumber)
    {
        string videoPath = $"{VIDEO_BASE_PATH}{videoNumber}.mp4";
        SetVideo(videoPath);
    }

    /// <summary>
    /// Pro testování - vygeneruj nová náhodná data
    /// </summary>
    [ContextMenu("Simulovat nová data")]
    public void TestNewRandomData()
    {
        SimulateLogicTeamData();
    }

    // === DEBUG METODY ===

    private void Log(string message)
    {
        if (enableDebug)
        {
            Debug.Log($"[EventDataReceiver] {message}");
        }
    }

    private void LogWarning(string message)
    {
        Debug.LogWarning($"[EventDataReceiver] {message}");
    }

    private void LogError(string message)
    {
        Debug.LogError($"[EventDataReceiver] {message}");
    }
}
