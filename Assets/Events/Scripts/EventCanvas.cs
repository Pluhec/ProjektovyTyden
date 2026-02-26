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
