using UnityEngine;
using UnityEngine.Video;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class EventCanvas : MonoBehaviour
{
    [Header("Canvases")]
    [SerializeField] private GameObject videoCanvas;
    [SerializeField] private GameObject decisionCanvas;

    [Header("Video")]
    [SerializeField] private VideoPlayer videoPlayer;

    [Header("Video Buttons")]
    [SerializeField] private Button skipButton;
    [SerializeField] private Button replayButton;

    [Header("Decision Buttons")]
    [SerializeField] private Button buttonFree;
    [SerializeField] private Button buttonMoney;
    [SerializeField] private Button buttonPerk;

    [Header("Decision Button Texts")]
    [SerializeField] private TextMeshProUGUI textFree;
    [SerializeField] private TextMeshProUGUI textMoney;
    [SerializeField] private TextMeshProUGUI textPerk;

    [Header("Tooltips")]
    [Tooltip("Specifické panely pro jednotlivá tlačítka")]
    public GameObject TooltipPanel1; // Pro buttonFree
    public GameObject TooltipPanel2; // Pro buttonMoney
    public GameObject TooltipPanel3; // Pro buttonPerk

    [Header("References")]
    [SerializeField] private EventDataReceiver eventDataReceiver;

    void Start()
    {
        // Nastav listener pro konec videa
        if (videoPlayer != null)
        {
            videoPlayer.loopPointReached += OnVideoEnd;
        }

        // Nastav listenery pro tlačítka
        if (skipButton != null)
        {
            skipButton.onClick.AddListener(SkipVideo);
        }

        if (replayButton != null)
        {
            replayButton.onClick.AddListener(ReplayVideo);
        }

        // Nastav listenery pro decision tlačítka
        if (buttonFree != null)
            buttonFree.onClick.AddListener(() => OnDecisionChosen(0));

        if (buttonMoney != null)
            buttonMoney.onClick.AddListener(() => OnDecisionChosen(1));

        if (buttonPerk != null)
            buttonPerk.onClick.AddListener(() => OnDecisionChosen(2));

        // Registruj se na event pro načtení options
        if (eventDataReceiver != null)
        {
            eventDataReceiver.OnOptionsLoaded += UpdateDecisionButtons;
            // Immediately update buttons in case the receiver already has options loaded
            var opts = eventDataReceiver.GetCurrentOptions();
            UpdateDecisionButtons(opts.free, opts.money, opts.perk);
        }
    }

    void Awake()
    {
        Debug.Log("[EventCanvas] Awake() - Initializing EventCanvas");

        // Skryj všechny tooltipy na začátku
        if (TooltipPanel1 != null) TooltipPanel1.SetActive(false);
        if (TooltipPanel2 != null) TooltipPanel2.SetActive(false);
        if (TooltipPanel3 != null) TooltipPanel3.SetActive(false);

        // Skryj oba sub-canvasy; gameObject zůstává aktivní, aby script běžel
        if (videoCanvas != null)
        {
            videoCanvas.SetActive(false);
            Debug.Log("[EventCanvas] videoCanvas hidden on Awake");
        }
        else
        {
            Debug.LogError("[EventCanvas] videoCanvas is NULL in Awake! Check Inspector!");
        }

        if (decisionCanvas != null)
        {
            decisionCanvas.SetActive(false);
            Debug.Log("[EventCanvas] decisionCanvas hidden on Awake");
        }
        else
        {
            Debug.LogError("[EventCanvas] decisionCanvas is NULL in Awake! Check Inspector!");
        }
    }

    /// <summary>
    /// Voláno schedulerm (EventDataReceiver) pro zobrazení eventu.
    /// </summary>
    public void Show()
    {
        Debug.Log("[EventCanvas] Show() called - attempting to show video canvas");
        ShowVideoCanvas();
    }

    /// <summary>
    /// Aktualizuje texty decision buttonů podle dat z JSON
    /// </summary>
    private void UpdateDecisionButtons(EventDataReceiver.OptionData optionFree, EventDataReceiver.OptionData optionMoney, EventDataReceiver.OptionData optionPerk)
    {
        // Button Free (nejhorší, zdarma)
        if (textFree != null && optionFree != null)
        {
            textFree.text = optionFree.OptionName ?? "Free";
        }

        // Button Money (za peníze)
        if (textMoney != null && optionMoney != null)
        {
            string moneyText = optionMoney.OptionName ?? "Money";
            if (optionMoney.OptionCost > 0)
            {
                moneyText += $" ({optionMoney.OptionCost}$)";
            }
            textMoney.text = moneyText;
        }

        // Button Perk (za perk)
        if (textPerk != null && optionPerk != null)
        {
            textPerk.text = optionPerk.OptionName ?? "Perk";
        }

        Debug.Log($"[EventCanvas] Buttons updated: Free='{optionFree?.OptionName}', Money='{optionMoney?.OptionName}', Perk='{optionPerk?.OptionName}'");

        // Check if buttons should be enabled/disabled based on requirements
        UpdateButtonStates(optionMoney, optionPerk);

        // Navěšení správných tooltip panelů na příslušná tlačítka (předáme i data option)
        SetupButtonTooltip(buttonFree, TooltipPanel1, optionFree, true);
        SetupButtonTooltip(buttonMoney, TooltipPanel2, optionMoney, CanAffordMoney(optionMoney));
        SetupButtonTooltip(buttonPerk, TooltipPanel3, optionPerk, HasRequiredPerk(optionPerk));
    }

    /// <summary>
    /// Přidá EventTrigger pro zobrazení/skrytí konkrétního tooltip panelu při najetí myši.
    /// </summary>
    private void SetupButtonTooltip(Button btn, GameObject targetTooltipPanel, EventDataReceiver.OptionData option, bool isEnabled)
    {
        if (btn == null || targetTooltipPanel == null) return;

        // Najdi TextMeshPro komponentu v panelu (předpokládá se, že tam je)
        TextMeshProUGUI tooltipText = targetTooltipPanel.GetComponentInChildren<TextMeshProUGUI>();

        // Get or create CanvasGroup for opacity control
        CanvasGroup canvasGroup = btn.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = btn.gameObject.AddComponent<CanvasGroup>();
        }

        EventTrigger trigger = btn.gameObject.GetComponent<EventTrigger>();
        if (trigger == null)
            trigger = btn.gameObject.AddComponent<EventTrigger>();
        else
            // Vyčisti předchozí hovery, aby se neduplikovaly
            trigger.triggers.RemoveAll(e => e.eventID == EventTriggerType.PointerEnter || e.eventID == EventTriggerType.PointerExit);

        // Zobrazení tooltipu
        var entryEnter = new EventTrigger.Entry { eventID = EventTriggerType.PointerEnter };
        entryEnter.callback.AddListener((data) => {
            // Brighten disabled button on hover so tooltip is readable
            if (!isEnabled && canvasGroup != null)
            {
                canvasGroup.alpha = 0.8f;
            }

            if (tooltipText != null)
            {
                if (isEnabled)
                {
                    tooltipText.text = FormatOptionEffects(option);
                }
                else
                {
                    tooltipText.text = GetDisabledReason(option);
                }
            }

            // Ensure tooltip panel is always fully visible (not affected by button opacity)
            CanvasGroup tooltipCanvasGroup = targetTooltipPanel.GetComponent<CanvasGroup>();
            if (tooltipCanvasGroup == null)
            {
                tooltipCanvasGroup = targetTooltipPanel.AddComponent<CanvasGroup>();
            }
            tooltipCanvasGroup.alpha = 1f;
            tooltipCanvasGroup.ignoreParentGroups = true; // Ignore parent button's opacity

            targetTooltipPanel.SetActive(true);
        });
        trigger.triggers.Add(entryEnter);

        // Skrytí tooltipu
        var entryExit = new EventTrigger.Entry { eventID = EventTriggerType.PointerExit };
        entryExit.callback.AddListener((data) => {
            // Restore disabled opacity when hover ends
            if (!isEnabled && canvasGroup != null)
            {
                canvasGroup.alpha = 0.5f;
            }
            targetTooltipPanel.SetActive(false);
        });
        trigger.triggers.Add(entryExit);
    }

    /// <summary>
    /// Sestaví text efektů z OptionData pro zobrazení v tooltipu.
    /// </summary>
    private string FormatOptionEffects(EventDataReceiver.OptionData option)
    {
        if (option == null) return "";

        System.Text.StringBuilder sb = new System.Text.StringBuilder();

        // Hlavní popis (pokud existuje) - očistíme strojové názvy
        if (!string.IsNullOrEmpty(option.OptionEffect))
        {
            sb.AppendLine(CleanLabel(option.OptionEffect));
        }

        // Statistiky podle věkových skupin (česky a s plus/minus)
        if (option.OptionEffectYoung != null)
            sb.AppendLine(FormatStatLine(0, option.OptionEffectYoung));
        if (option.OptionEffectAdult != null)
            sb.AppendLine(FormatStatLine(1, option.OptionEffectAdult));
        if (option.OptionEffectSenior != null)
            sb.AppendLine(FormatStatLine(2, option.OptionEffectSenior));

        // Special effect - přeložíme typ a doplníme detaily
        if (option.OptionSpecialEffect != null)
        {
            int t = option.OptionSpecialEffect.EffectsType;
            string typeName = MapSpecialType(t);
            sb.Append(typeName);

            if (option.OptionSpecialEffect.EffectsGroup.HasValue)
            {
                sb.Append($" - skupina: {MapManipulatable(option.OptionSpecialEffect.EffectsGroup.Value)}");
            }

            if (option.OptionSpecialEffect.EffectsEducation.HasValue)
            {
                sb.Append($" - vzdělání: {MapEducation(option.OptionSpecialEffect.EffectsEducation.Value)}");
            }

            if (option.OptionSpecialEffect.EffectAmmount.HasValue)
            {
                sb.Append($" (množství: {option.OptionSpecialEffect.EffectAmmount.Value})");
            }

            sb.AppendLine();
        }

        string outText = sb.ToString().Trim();
        return string.IsNullOrEmpty(outText) ? "(Žádné efekty)" : outText;
    }

    private string CleanLabel(string raw)
    {
        if (string.IsNullOrEmpty(raw)) return "";
        string s = raw.Replace("_", " ");
        if (s.Length == 1) return s.ToUpper();
        s = char.ToUpperInvariant(s[0]) + s.Substring(1).ToLowerInvariant();
        return s;
    }

    private string FormatStatLine(int ageIndex, EventDataReceiver.StatData sd)
    {
        string ageLabel = ageIndex == 0 ? "Mladí" : ageIndex == 1 ? "Dospělí" : "Senioři";
        string v = FormatSigned(sd.Virality);
        string i = FormatSigned(sd.Impact);
        string vi = FormatSigned(sd.Visibility);
        return $"{ageLabel}: viralita {v}, dopad {i}, viditelnost {vi}";
    }

    private string FormatSigned(int val)
    {
        return val > 0 ? $"+{val}" : val.ToString();
    }

    private string MapSpecialType(int t)
    {
        switch (t)
        {
            case 0: return "Speciální efekt: Školství";
            case 1: return "Speciální efekt: Demokracie";
            case 2: return "Speciální efekt: Finance (pop-up)";
            case 3: return "Speciální efekt: Sociální skupina";
            case 4: return "Speciální efekt: Viditelnost";
            default: return $"Speciální efekt: Typ {t}";
        }
    }

    private string MapManipulatable(int g)
    {
        switch (g)
        {
            case 0: return "Imunní";
            case 1: return "Neutrální";
            case 2: return "Sympatizující";
            case 3: return "Spolupracovníci";
            default: return g.ToString();
        }
    }

    private string MapEducation(int e)
    {
        switch (e)
        {
            case 0: return "Základní";
            case 1: return "Střední";
            case 2: return "Vysokoškolské";
            default: return e.ToString();
        }
    }

    /// <summary>
    /// Updates the visual state of buttons (enabled/disabled appearance)
    /// </summary>
    private void UpdateButtonStates(EventDataReceiver.OptionData optionMoney, EventDataReceiver.OptionData optionPerk)
    {
        // Free button is always enabled
        SetButtonVisualState(buttonFree, true);

        // Money button - check if player has enough money
        bool canAffordMoney = CanAffordMoney(optionMoney);
        SetButtonVisualState(buttonMoney, canAffordMoney);

        // Perk button - check if player has the required perk
        bool hasRequiredPerk = HasRequiredPerk(optionPerk);
        SetButtonVisualState(buttonPerk, hasRequiredPerk);

        Debug.Log($"[EventCanvas] Button states - Money: {canAffordMoney}, Perk: {hasRequiredPerk}");
    }

    /// <summary>
    /// Sets the visual state of a button (grayed out when disabled)
    /// </summary>
    private void SetButtonVisualState(Button btn, bool isEnabled)
    {
        if (btn == null) return;

        // Change button interactability
        btn.interactable = isEnabled;

        // Adjust visual opacity
        CanvasGroup canvasGroup = btn.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = btn.gameObject.AddComponent<CanvasGroup>();
        }

        canvasGroup.alpha = isEnabled ? 1f : 0.5f;
    }

    /// <summary>
    /// Checks if the player has enough money for the money option
    /// </summary>
    private bool CanAffordMoney(EventDataReceiver.OptionData optionMoney)
    {
        if (optionMoney == null) return true;
        return PlayerChoice.DataSets.PlayerStats.Money >= optionMoney.OptionCost;
    }

    /// <summary>
    /// Checks if the player has the required perk
    /// </summary>
    private bool HasRequiredPerk(EventDataReceiver.OptionData optionPerk)
    {
        if (optionPerk == null) return true;
        if (string.IsNullOrEmpty(optionPerk.OptionPerk)) return true;

        // Check using the same logic as EventDataReceiver.IsPerkOwned
        System.Reflection.FieldInfo field = typeof(PlayerChoice.DataSets.PerkSet).GetField(
            optionPerk.OptionPerk,
            System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);

        if (field == null)
        {
            // Check owned perks list as fallback
            return PlayerChoice.DataSets.DataFunctions.OwnedPerks.Contains(optionPerk.OptionPerk);
        }

        PlayerChoice.DataSets.PerkInformation perkInfo = field.GetValue(null) as PlayerChoice.DataSets.PerkInformation;
        return perkInfo != null && perkInfo.IsBought;
    }

    /// <summary>
    /// Gets the reason why a button is disabled
    /// </summary>
    private string GetDisabledReason(EventDataReceiver.OptionData option)
    {
        if (option == null) return "NEDOSTUPNÉ";

        // Check money requirement
        if (option.OptionCost > 0)
        {
            int currentMoney = PlayerChoice.DataSets.PlayerStats.Money;
            int required = option.OptionCost;
            return $"NEDOSTATEK PENĚZ\n\nPotřebujete: {required}$\nMáte: {currentMoney}$\nChybí: {required - currentMoney}$";
        }

        // Check perk requirement
        if (!string.IsNullOrEmpty(option.OptionPerk))
        {
            return $"CHYBĚJÍCÍ PERK\n\nPotřebujete perk:\n'{option.OptionPerk}'";
        }

        return "NEDOSTUPNÉ";
    }

    void OnDestroy()
    {
        // Odeber listenery
        if (videoPlayer != null)
        {
            videoPlayer.loopPointReached -= OnVideoEnd;
        }

        if (skipButton != null)
            skipButton.onClick.RemoveListener(SkipVideo);

        if (replayButton != null)
            replayButton.onClick.RemoveListener(ReplayVideo);

        if (buttonFree != null)
            buttonFree.onClick.RemoveAllListeners();

        if (buttonMoney != null)
            buttonMoney.onClick.RemoveAllListeners();

        if (buttonPerk != null)
            buttonPerk.onClick.RemoveAllListeners();

        // Odregistruj se z eventu
        if (eventDataReceiver != null)
        {
            eventDataReceiver.OnOptionsLoaded -= UpdateDecisionButtons;
        }
    }

    /// <summary>
    /// Zavolá se po kliknutí na Free / Money / Perk tlačítko.
    /// </summary>
    private void OnDecisionChosen(int optionIndex)
    {
        if (eventDataReceiver == null) return;

        bool success = eventDataReceiver.OnOptionChosen(optionIndex);

        if (!success)
        {
            // Hráč nemá dost peněz nebo perk — tlačítko vizuálně "zakmitá", ale event zůstane otevřený
            StartCoroutine(ShakeButton(optionIndex));
            return;
        }

        // Volba byla přijata — skryj event canvas
        CloseEventCanvas();
    }

    /// <summary>
    /// Skryje celý event canvas po dokončení rozhodnutí.
    /// </summary>
    private void CloseEventCanvas()
    {
        if (videoCanvas != null) videoCanvas.SetActive(false);
        if (decisionCanvas != null) decisionCanvas.SetActive(false);
        
        // Skryj i tooltipy, kdyby náhodou zůstaly viset při zavření canvasu
        if (TooltipPanel1 != null) TooltipPanel1.SetActive(false);
        if (TooltipPanel2 != null) TooltipPanel2.SetActive(false);
        if (TooltipPanel3 != null) TooltipPanel3.SetActive(false);

        // Gradually restore background music
        if (MusicManager.Instance != null)
        {
            MusicManager.Instance.RestoreMusic(1.0f);
        }
    }

    /// <summary>
    /// Krátká vizuální animace tlačítka při neúspěšné volbě.
    /// </summary>
    private System.Collections.IEnumerator ShakeButton(int optionIndex)
    {
        Button btn = optionIndex == 0 ? buttonFree : optionIndex == 1 ? buttonMoney : buttonPerk;
        if (btn == null) yield break;

        RectTransform rt = btn.GetComponent<RectTransform>();
        if (rt == null) yield break;

        Vector2 originalPos = rt.anchoredPosition;
        float duration = 0.3f;
        float elapsed = 0f;
        float magnitude = 8f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float x = Mathf.Sin(elapsed * 60f) * magnitude * (1f - elapsed / duration);
            rt.anchoredPosition = originalPos + new Vector2(x, 0f);
            yield return null;
        }

        rt.anchoredPosition = originalPos;
    }

    /// <summary>
    /// Zavolá se když video dohraje
    /// </summary>
    private void OnVideoEnd(VideoPlayer vp)
    {
        ShowDecisionCanvas();
    }

    /// <summary>
    /// Přeskočí video a zobrazí Decision Canvas
    /// </summary>
    public void SkipVideo()
    {
        if (videoPlayer != null)
        {
            videoPlayer.Stop();
        }
        ShowDecisionCanvas();
    }

    /// <summary>
    /// Přehraje video znovu - zobrazí Video Canvas
    /// </summary>
    public void ReplayVideo()
    {
        ShowVideoCanvas();
    }

    /// <summary>
    /// Zobrazí Video Canvas a spustí video
    /// </summary>
    private void ShowVideoCanvas()
    {
        Debug.Log("[EventCanvas] ShowVideoCanvas() called");

        if (decisionCanvas != null)
        {
            decisionCanvas.SetActive(false);
            Debug.Log("[EventCanvas] Decision canvas hidden");
        }
        else
        {
            Debug.LogWarning("[EventCanvas] decisionCanvas is NULL!");
        }

        if (videoCanvas != null)
        {
            Debug.Log("[EventCanvas] Setting videoCanvas to active");
            videoCanvas.SetActive(true);
            Debug.Log($"[EventCanvas] videoCanvas.activeSelf = {videoCanvas.activeSelf}");
        }
        else
        {
            Debug.LogError("[EventCanvas] videoCanvas is NULL! Assign it in the Inspector!");
        }

        if (videoPlayer != null)
        {
            videoPlayer.time = 0;
            videoPlayer.Play();
            Debug.Log("[EventCanvas] Video player started");
        }
        else
        {
            Debug.LogWarning("[EventCanvas] videoPlayer is NULL!");
        }

        // Gradually duck background music
        if (MusicManager.Instance != null)
        {
            MusicManager.Instance.DuckMusic(0.1f, 1.0f);
        }
    }

    /// <summary>
    /// Zobrazí Decision Canvas
    /// </summary>
    private void ShowDecisionCanvas()
    {
        if (videoCanvas != null)
        {
            videoCanvas.SetActive(false);
        }

        if (decisionCanvas != null)
        {
            decisionCanvas.SetActive(true);
        }
    }
}