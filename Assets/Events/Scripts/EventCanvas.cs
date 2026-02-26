using UnityEngine;
using UnityEngine.Video;
using UnityEngine.UI;
using TMPro;

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

    [Header("Tooltip")]
    [Tooltip("Panel GameObject used as tooltip (set inactive by default)")]
    [SerializeField] private GameObject tooltipPanel;
    [SerializeField] private TextMeshProUGUI tooltipLabel;

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

        // Na začátku zobraz Video Canvas
        ShowVideoCanvas();
    }

    void Awake()
    {
        // Ensure tooltip is hidden at start
        if (tooltipPanel != null)
            tooltipPanel.SetActive(false);
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

        // Wire hover handlers with option data so tooltip can show effect descriptions
        AttachHover(buttonFree, optionFree);
        AttachHover(buttonMoney, optionMoney);
        AttachHover(buttonPerk, optionPerk);
    }

    private void AttachHover(Button btn, EventDataReceiver.OptionData option)
    {
        if (btn == null) return;

        var hover = btn.gameObject.GetComponent<EventDecisionHover>();
        if (hover == null)
            hover = btn.gameObject.AddComponent<EventDecisionHover>();

        hover.parentCanvas = this;
        hover.option = option;
    }

    /// <summary>
    /// Show tooltip with given text at screen position.
    /// </summary>
    public void ShowTooltip(string text, Vector2 screenPosition)
    {
        if (tooltipPanel == null || tooltipLabel == null) return;
        // Activate first so RectTransform sizes are valid
        tooltipPanel.SetActive(true);
        tooltipLabel.text = text;

        Canvas parent = GetComponentInParent<Canvas>();
        RectTransform tooltipRect = tooltipPanel.GetComponent<RectTransform>();

        if (parent == null || tooltipRect == null) return;

        // Position and clamp tooltip inside parent canvas
        UpdateTooltipPosition(screenPosition, parent, tooltipRect);
    }

    // Helper that calculates and applies tooltip anchored position inside parent canvas
    private void UpdateTooltipPosition(Vector2 screenPosition, Canvas parent, RectTransform tooltipRect)
    {
        if (parent == null || tooltipRect == null) return;

        RectTransform canvasRect = parent.transform as RectTransform;
        Vector2 localPoint;
        Camera cam = parent.renderMode == RenderMode.ScreenSpaceOverlay ? null : parent.worldCamera;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenPosition, cam, out localPoint);

        // Small offset from cursor
        Vector2 offset = new Vector2(12f, -12f);
        Vector2 desired = localPoint + offset;

        // Clamp tooltip inside canvas bounds
        Vector2 canvasSize = canvasRect.rect.size;
        Vector2 tooltipSize = tooltipRect.rect.size;

        float minX = -canvasSize.x * 0.5f + tooltipSize.x * tooltipRect.pivot.x;
        float maxX = canvasSize.x * 0.5f - tooltipSize.x * (1f - tooltipRect.pivot.x);
        float minY = -canvasSize.y * 0.5f + tooltipSize.y * tooltipRect.pivot.y;
        float maxY = canvasSize.y * 0.5f - tooltipSize.y * (1f - tooltipRect.pivot.y);

        desired.x = Mathf.Clamp(desired.x, minX, maxX);
        desired.y = Mathf.Clamp(desired.y, minY, maxY);

        tooltipRect.anchoredPosition = desired;
    }

    // Make active tooltip follow the mouse while visible
    void Update()
    {
        if (tooltipPanel == null || !tooltipPanel.activeSelf) return;
        Canvas parent = GetComponentInParent<Canvas>();
        RectTransform tooltipRect = tooltipPanel.GetComponent<RectTransform>();
        if (parent == null || tooltipRect == null) return;
        UpdateTooltipPosition(Input.mousePosition, parent, tooltipRect);
    }

    /// <summary>
    /// Hide tooltip.
    /// </summary>
    public void HideTooltip()
    {
        if (tooltipPanel == null) return;
        tooltipPanel.SetActive(false);
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
        gameObject.SetActive(false);
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
        if (decisionCanvas != null)
        {
            decisionCanvas.SetActive(false);
        }

        if (videoCanvas != null)
        {
            videoCanvas.SetActive(true);
        }

        if (videoPlayer != null)
        {
            videoPlayer.time = 0;
            videoPlayer.Play();
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
