using UnityEngine;
using UnityEngine.Video;
using TMPro;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using PlayerChoice.DataSets;

/// <summary>
/// Přijímá data od logiky týmu - cestu k videu a text k zobrazení.
/// Načítá eventy z Events.json.
/// Script zatim bete videa od 16-30 pro testování potom udelejte vsechny videa co budou
/// </summary>
public class EventDataReceiver : MonoBehaviour
{
    [Header("=== REFERENCE ===")]
    [SerializeField] private VideoPlayer videoPlayer;
    [SerializeField] private TextMeshProUGUI eventText;

    [Header("=== JSON DATA ===")]
    [Tooltip("Cesta k Events.json (relativní k Assets)")]
    [SerializeField] private string eventsJsonPath = "Assets/PleyerDecisions/Events/Events.json";

    [Header("=== SIMULACE (pro testování) ===")]
    [Tooltip("Zapni pro automatické testování při startu")]
    [SerializeField] private bool simulateOnStart = true;
    
    [Tooltip("ID eventu pro testování (1-30)")]
    [SerializeField] private int testEventId = 1;
    
    // Dostupné eventy pro testování (1-30)
    private int[] availableEventIds = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30 };

    [Header("=== DEBUG ===")]
    [Tooltip("Zobrazí detailní debug informace v Console")]
    [SerializeField] private bool enableDebug = true;

    // Seznam načtených eventů z JSON
    private List<EventData> loadedEvents = new List<EventData>();

    // Wrapper třída pro JSON deserializaci
    [System.Serializable]
    private class EventsWrapper
    {
        public List<EventData> Events;
    }

    // Třída pro option data z JSON
    [System.Serializable]
    public class OptionData
    {
        public int OptionCost;
        public string OptionPerk;
        public string OptionName;
        public string OptionEffect;
    }

    // Třída pro data eventu z JSON
    [System.Serializable]
    private class EventData
    {
        public int EventId;
        public string EventName;
        public string EventDescription;
        public string VideoPath;
        public string RequiredPerk;
        public int? CollaboratorsRequired;
        public OptionData OptionFree;
        public OptionData OptionMoney;
        public OptionData OptionPerk;
    }

    // Aktuálně zobrazený event
    private EventData currentEvent;

    // Event pro notifikaci změny dat (pro UI)
    public System.Action<OptionData, OptionData, OptionData> OnOptionsLoaded;

    void Awake()
    {
        // Registruj tento receiver do DataFunctions
        DataFunctions.SetEventDataReceiver(this);
    }

    void Start()
    {
        // Načti eventy z JSON
        LoadEventsFromJSON();

        if (simulateOnStart)
        {
            Log("========================================");
            Log("SIMULACE AKTIVNÍ - Zobrazuji náhodný event (16-30)");
            Log("========================================");
            
            // Vždy použij náhodný event z 16-30 (ignoruj testEventId kvůli Unity serialization)
            DisplayRandomEvent();
        }
    }

    /// <summary>
    /// Načte eventy z Events.json souboru
    /// </summary>
    public void LoadEventsFromJSON()
    {
        string fullPath = Path.Combine(Application.dataPath, eventsJsonPath.Replace("Assets/", ""));
        
        if (!File.Exists(fullPath))
        {
            LogError($"Events.json nenalezen na cestě: {fullPath}");
            return;
        }

        try
        {
            string jsonContent = File.ReadAllText(fullPath);
            EventsWrapper wrapper = JsonConvert.DeserializeObject<EventsWrapper>(jsonContent);
            
            if (wrapper != null && wrapper.Events != null)
            {
                loadedEvents = wrapper.Events;
                Log($"Načteno {loadedEvents.Count} eventů z JSON.");
            }
            else
            {
                LogError("Nepodařilo se deserializovat Events.json");
            }
        }
        catch (System.Exception ex)
        {
            LogError($"Chyba při načítání JSON: {ex.Message}");
        }
    }

    /// <summary>
    /// Zobrazí event podle jeho ID
    /// </summary>
    /// <param name="eventId">ID eventu (16-30)</param>
    public void DisplayEventById(int eventId)
    {
        EventData eventData = loadedEvents.Find(e => e.EventId == eventId);
        
        if (eventData == null)
        {
            LogError($"Event s ID {eventId} nenalezen!");
            return;
        }

        // Ulož aktuální event
        currentEvent = eventData;

        Log($"--- ZOBRAZUJI EVENT {eventId}: {eventData.EventName} ---");
        Log($"Video path: {eventData.VideoPath}");
        Log($"Description: {eventData.EventDescription}");
        Log($"OptionFree: {eventData.OptionFree?.OptionName}");
        Log($"OptionMoney: {eventData.OptionMoney?.OptionName} (cena: {eventData.OptionMoney?.OptionCost})");
        Log($"OptionPerk: {eventData.OptionPerk?.OptionName} (perk: {eventData.OptionPerk?.OptionPerk})");
        Log("----------------------------------------");

        SetEventData(eventData.VideoPath, eventData.EventDescription);

        // Notifikuj UI o nových options
        OnOptionsLoaded?.Invoke(eventData.OptionFree, eventData.OptionMoney, eventData.OptionPerk);
    }

    /// <summary>
    /// Vrátí aktuální options pro UI
    /// </summary>
    public (OptionData free, OptionData money, OptionData perk) GetCurrentOptions()
    {
        if (currentEvent == null)
            return (null, null, null);
        
        return (currentEvent.OptionFree, currentEvent.OptionMoney, currentEvent.OptionPerk);
    }

    /// <summary>
    /// Vrátí data eventu podle ID (pro použití v jiných skriptech)
    /// </summary>
    public (string videoPath, string description, string name) GetEventDataById(int eventId)
    {
        EventData eventData = loadedEvents.Find(e => e.EventId == eventId);
        
        if (eventData == null)
        {
            return (null, null, null);
        }

        return (eventData.VideoPath, eventData.EventDescription, eventData.EventName);
    }

    /// <summary>
    /// Zobrazí náhodný event z dostupných (16-30)
    /// </summary>
    public void DisplayRandomEvent()
    {
        if (loadedEvents.Count == 0)
        {
            LogError("Žádné eventy nejsou načteny!");
            return;
        }

        // Vyber náhodný event z dostupných (16-30)
        int randomIndex = Random.Range(0, availableEventIds.Length);
        int randomEventId = availableEventIds[randomIndex];
        
        Log($"--- NÁHODNÝ EVENT ID: {randomEventId} ---");
        DisplayEventById(randomEventId);
    }

    /// <summary>
    /// HLAVNÍ METODA: Volej tuto metodu když přijdou data od logiky týmu
    /// </summary>
    /// <param name="videoPath">Cesta k videu (např. "Assets/Video/23.mp4")</param>
    /// <param name="displayText">Text který se má zobrazit</param>
    public void SetEventData(string videoPath, string displayText)
    {
        Log("========================================");
        Log("PŘIJATA DATA OD LOGIKY");
        Log("========================================");

        // Nastav video
        SetVideo(videoPath);

        // Nastav text
        SetText(displayText);

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
    /// Alternativní metoda - zobrazí event podle čísla videa
    /// </summary>
    public void SetVideoByNumber(int videoNumber)
    {
        DisplayEventById(videoNumber);
    }

    /// <summary>
    /// Pro testování - zobrazí nový náhodný event z JSON
    /// </summary>
    [ContextMenu("Zobrazit náhodný event")]
    public void TestNewRandomData()
    {
        DisplayRandomEvent();
    }

    /// <summary>
    /// Pro testování - znovu načti JSON a zobraz testovací event
    /// </summary>
    [ContextMenu("Znovu načíst JSON")]
    public void ReloadJSON()
    {
        LoadEventsFromJSON();
        DisplayEventById(testEventId);
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
