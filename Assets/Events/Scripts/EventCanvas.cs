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

        // Registruj se na event pro načtení options
        if (eventDataReceiver != null)
        {
            eventDataReceiver.OnOptionsLoaded += UpdateDecisionButtons;
        }

        // Na začátku zobraz Video Canvas
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
    }

    void OnDestroy()
    {
        // Odeber listenery
        if (videoPlayer != null)
        {
            videoPlayer.loopPointReached -= OnVideoEnd;
        }

        if (skipButton != null)
        {
            skipButton.onClick.RemoveListener(SkipVideo);
        }

        if (replayButton != null)
        {
            replayButton.onClick.RemoveListener(ReplayVideo);
        }

        // Odregistruj se z eventu
        if (eventDataReceiver != null)
        {
            eventDataReceiver.OnOptionsLoaded -= UpdateDecisionButtons;
        }
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
