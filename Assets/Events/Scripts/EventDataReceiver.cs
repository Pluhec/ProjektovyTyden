using UnityEngine;
using UnityEngine.Video;
using TMPro;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using PlayerChoice.DataSets;
using System.Net;

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
    [Tooltip("Přírná reference na EventCanvas komponentu ve scéně")]
    [SerializeField] private EventCanvas eventCanvas;

    [Header("=== JSON DATA ===")]
    [Tooltip("Cesta k Events.json (relativní k Assets)")]
    [SerializeField] private string eventsJsonPath = "Assets/PleyerDecisions/Events/Events.json";

    [Header("=== SIMULACE (pro testování) ===")]
    [Tooltip("Zapni pro automatické testování při startu")]
    [SerializeField] private bool simulateOnStart = true;
    
    [Tooltip("ID eventu pro testování (1-30)")]
    [SerializeField] private int testEventId = 1;

    public TimeManager timeManager;
    public TimeSoundManager timeSoundManager;
    public bool autoStart = false;

    [Header("=== NÁHODNÉ EVENTY ===")]
    [Tooltip("Zapni pro automatické spouštění náhodných eventů v průběhu hry")]
    [SerializeField] private bool enableRandomEvents = true;
    [Tooltip("Minimální čekací doba mezi eventy (v sekundách)")]
    [SerializeField] private float minEventInterval = 30f;
    [Tooltip("Maximální čekací doba mezi eventy (v sekundách)")]
    [SerializeField] private float maxEventInterval = 120f;

    // Interní stav scheduleru
    private bool isEventActive = false;
    private Coroutine randomEventCoroutine;
    
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
        public StatData OptionEffectYoung;
        public StatData OptionEffectAdult;
        public StatData OptionEffectSenior;
        public SpecialEffect OptionSpecialEffect;
    }

    [System.Serializable]
    public class StatData
    {
        public int AgeGroup;
        public int Virality;
        public int Impact;
        public int Visibility;
    }

    [System.Serializable]
    public class SpecialEffect
    {
        public int EffectsType;
        public int? EffectsGroup;
        public int? EffectsEducation;
        public int? EffectAmmount;
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
        if (autoStart || enableRandomEvents)
        {
            LoadEventsFromJSON();
        }

        if (simulateOnStart)
        {
            Log("========================================");
            Log("SIMULACE AKTIVNÍ - Zobrazuji náhodný event (16-30)");
            Log("========================================");
            
            DisplayRandomEvent();

            // Zobraz canvas po načtení dat
            if (eventCanvas != null)
                eventCanvas.Show();
            else
                Debug.LogWarning("[EventDataReceiver] EventCanvas není přiřazen v Inspectoru!");
        }
        else if (enableRandomEvents)
        {
            randomEventCoroutine = StartCoroutine(RandomEventScheduler());
        }
    }

    /// <summary>
    /// Spouští náhodné eventy v pravidelných náhodných intervalech po celou dobu hry.
    /// </summary>
    private System.Collections.IEnumerator RandomEventScheduler()
    {
        // Počáteční čekání před prvním eventem
        float initialWait = Random.Range(minEventInterval, maxEventInterval);
        Log($"[Scheduler] První event za {initialWait:F1}s");
        yield return new WaitForSecondsRealtime(initialWait);

        while (true)
        {
            // Zobraz event
            if (loadedEvents.Count > 0)
            {
                isEventActive = true;

                // Nejdřív nači data eventu (nastaví URL videa a volá Prepare)
                DisplayRandomEvent();

                // Pak zobraz canvas a spusť video (volá Play)
                if (eventCanvas != null)
                    eventCanvas.Show();
                else
                    Debug.LogWarning("[EventDataReceiver] EventCanvas není přiřazen v Inspectoru!");

                // Čekej dokud hráč event nevyřeší
                yield return new WaitUntil(() => !isEventActive);
            }
            else
            {
                LogWarning("[Scheduler] Žádné načtené eventy, scheduler čeká.");
            }

            // Čekej náhodnou dobu před dalším eventem
            float waitTime = Random.Range(minEventInterval, maxEventInterval);
            Log($"[Scheduler] Další event za {waitTime:F1}s");
            yield return new WaitForSecondsRealtime(waitTime);
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

        // Pozastav hru při zobrazení eventu
        if (timeManager != null) timeManager.TogglePause();
        if (timeSoundManager != null) timeSoundManager.StopAllSound();

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

    private void TrySendOptionStatData(OptionData option)
    {
        if (option == null) return;

        var statsList = new System.Collections.Generic.List<PlayerChoice.DataSets.EnumStructs.S_StatData>();

        if (option.OptionEffectYoung != null)
            statsList.Add(ConvertStat(option.OptionEffectYoung));
        if (option.OptionEffectAdult != null)
            statsList.Add(ConvertStat(option.OptionEffectAdult));
        if (option.OptionEffectSenior != null)
            statsList.Add(ConvertStat(option.OptionEffectSenior));

        if (statsList.Count > 0)
            PlayerChoice.DataSets.DataFunctions.SendDataToSimulation(statsList.ToArray());

        // Handle simple special effect for democracy if present
        if (option.OptionSpecialEffect != null && option.OptionSpecialEffect.EffectsType == (int)PlayerChoice.DataSets.EnumStructs.E_PerkSpecialType.Democracy)
        {
            int amt = option.OptionSpecialEffect.EffectAmmount ?? 0;
            PlayerChoice.DataSets.DataFunctions.InformChangeDemocracyMeter(amt);
        }
    }

    private PlayerChoice.DataSets.EnumStructs.S_StatData ConvertStat(StatData sd)
    {
        PlayerChoice.DataSets.EnumStructs.S_StatData outStat = new PlayerChoice.DataSets.EnumStructs.S_StatData();
        int age = Mathf.Clamp(sd.AgeGroup, 0, 2);
        outStat.AgeGroup = (PlayerChoice.DataSets.EnumStructs.E_Age)age;
        outStat.Virality = (sbyte)sd.Virality;
        outStat.Impact = (sbyte)sd.Impact;
        outStat.Visibility = (sbyte)sd.Visibility;
        return outStat;
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
    /// Zavolej tuto metodu když hráč klikne na tlačítko volby (0 = zdarma, 1 = peníze, 2 = perk).
    /// Zkontroluje požadavky, odečte peníze/perk a resumuje hru.
    /// </summary>
    public bool OnOptionChosen(int optionIndex)
    {
        if (currentEvent == null)
        {
            LogError("OnOptionChosen: žádný aktuální event!");
            return false;
        }

        switch (optionIndex)
        {
            case 0: // Zdarma - vždy možné
                Log("Hráč zvolil FREE možnost.");
                TrySendOptionStatData(currentEvent.OptionFree);
                ResumeGame();
                return true;

            case 1: // Za peníze
                OptionData money = currentEvent.OptionMoney;
                if (money == null) { LogError("OptionMoney je null!"); return false; }

                if (PlayerChoice.DataSets.PlayerStats.Money < money.OptionCost)
                {
                    Log($"Hráč nemá dost peněz! Má {PlayerChoice.DataSets.PlayerStats.Money}, potřebuje {money.OptionCost}.");
                    return false;
                }

                PlayerChoice.DataSets.PlayerStats.Money -= (byte)money.OptionCost;
                PlayerChoice.DataSets.PlayerStats.NotifyMoneyChanged();
                Log($"Odečteno {money.OptionCost}$. Zbývá: {PlayerChoice.DataSets.PlayerStats.Money}$.");

                // Apply stat effects if present
                TrySendOptionStatData(money);

                ResumeGame();
                return true;

            case 2: // Za perk
                OptionData perk = currentEvent.OptionPerk;
                if (perk == null) { LogError("OptionPerk je null!"); return false; }

                if (!IsPerkOwned(perk.OptionPerk))
                {
                    Log($"Hráč nemá perk '{perk.OptionPerk}'.");
                    return false;
                }

                Log($"Hráč použil perk '{perk.OptionPerk}'.");

                // Apply stat effects if present
                TrySendOptionStatData(perk);

                ResumeGame();
                return true;

            default:
                LogError($"Neznámý optionIndex: {optionIndex}");
                return false;
        }
    }

    /// <summary>
    /// Resumuje hru po dokončení eventu (unpause čas + zvuk).
    /// </summary>
    public void ResumeGame()
    {
        if (timeManager != null)
            timeManager.TogglePause();

        if (timeSoundManager != null)
            timeSoundManager.ResumeSound();

        // Oznám scheduleru, že event byl vyřešen
        isEventActive = false;

        Log("Hra resumována po eventu.");
    }

    /// <summary>
    /// Zkontroluje jestli hráč vlastní perk podle jeho fieldName v PerkSet.
    /// </summary>
    private bool IsPerkOwned(string perkFieldName)
    {
        if (string.IsNullOrEmpty(perkFieldName)) return false;

        System.Reflection.FieldInfo field = typeof(PerkSet).GetField(
            perkFieldName,
            System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);

        if (field == null)
        {
            // If not a field name, maybe the JSON stores human-readable PerkName; check owned list.
            if (PlayerChoice.DataSets.DataFunctions.OwnedPerks.Contains(perkFieldName))
                return true;

            LogWarning($"IsPerkOwned: Perk field '{perkFieldName}' nenalezen v PerkSet.");
            return false;
        }

        PlayerChoice.DataSets.PerkInformation perkInfo = field.GetValue(null) as PlayerChoice.DataSets.PerkInformation;
        return perkInfo != null && perkInfo.IsBought;
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
