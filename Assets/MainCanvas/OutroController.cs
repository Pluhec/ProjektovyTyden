using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.Events;
using UnityEngine.Video;

public class OutroController : MonoBehaviour
{
    [Header("UI Components")]
    [Tooltip("CanvasGroup celého overlaye (nad videem) – kvůli fade-outu celého outro.")]
    public CanvasGroup overlayCanvasGroup;

    [Tooltip("TextMeshProUGUI komponenta pro titulky / texty.")]
    public TextMeshProUGUI textComponent;

    [Header("Video Components")]
    [Tooltip("VideoPlayer, který přehrává outro video na pozadí.")]
    public VideoPlayer videoPlayer;

    [Tooltip("Video, které se přehraje při výhře.")]
    public VideoClip winVideo;

    [Tooltip("Video, které se přehraje při prohře.")]
    public VideoClip loseVideo;

    [Header("Text Settings")]
    public float fadeDuration = 1.0f;    // Jak dlouho trvá fade in/out textu
    public float displayDuration = 2.0f; // Jak dlouho daný text svítí
    [TextArea(2, 5)]
    public string[] messages;            // Seznam zpráv / titulků

    [Header("Events")]
    public UnityEvent onOutroFinished;   // Co se stane po skončení (třeba návrat do main menu)

    private bool outroIsPlaying = false;

    private void Awake()
    {
        if (overlayCanvasGroup == null)
            overlayCanvasGroup = GetComponent<CanvasGroup>();

        if (overlayCanvasGroup != null)
        {
            overlayCanvasGroup.alpha = 0f;    // Na začátku skryté
            overlayCanvasGroup.blocksRaycasts = false;
        }
        
        if (textComponent != null)
            textComponent.alpha = 0f;

        if (videoPlayer != null)
        {
            videoPlayer.isLooping = true;   // Outro video v loopu
            videoPlayer.playOnAwake = false;
        }
    }

    /// <summary>
    /// Spusť outro pro výhru hráče.
    /// </summary>
    public void UserWinPlayOutro()
    {
        if (winVideo == null || videoPlayer == null)
        {
            Debug.LogWarning("OutroController: Win video nebo VideoPlayer není nastaven!");
            return;
        }
        StartOutro(winVideo);
    }

    /// <summary>
    /// Spusť outro pro prohru hráče.
    /// </summary>
    public void UserLosePlayOutro()
    {
        if (loseVideo == null || videoPlayer == null)
        {
            Debug.LogWarning("OutroController: Lose video nebo VideoPlayer není nastaven!");
            return;
        }
        StartOutro(loseVideo);
    }

    private void StartOutro(VideoClip clip)
    {
        if (outroIsPlaying)
            return;

        outroIsPlaying = true;

        // Nastavit video a spustit
        videoPlayer.clip = clip;
        videoPlayer.isLooping = true;
        videoPlayer.Play();

        // Zviditelnit overlay (CanvasGroup) a spustit sekvenci textů
        if (overlayCanvasGroup != null)
        {
            overlayCanvasGroup.alpha = 1f;
            overlayCanvasGroup.blocksRaycasts = true;
        }

        if (textComponent != null)
            StartCoroutine(PlayOutroSequence());
        else
            Debug.LogWarning("OutroController: textComponent není nastaven, titulky se nebudou zobrazovat.");
    }

    private IEnumerator PlayOutroSequence()
    {
        // Text na začátku neviditelný
        textComponent.alpha = 0f;

        // Projdeme všechny zprávy v poli
        foreach (string msg in messages)
        {
            textComponent.text = msg;

            // Fade In textu
            yield return StartCoroutine(FadeText(0f, 1f));

            // Zobrazení po určitou dobu
            yield return new WaitForSeconds(displayDuration);

            // Fade Out textu
            yield return StartCoroutine(FadeText(1f, 0f));

            // Lehké prodlení mezi texty
            yield return new WaitForSeconds(0.5f);
        }

        // Po poslední zprávě můžeme provést fade-out celého overlaye
        if (overlayCanvasGroup != null)
            yield return StartCoroutine(FadeOverlayOut());

        onOutroFinished?.Invoke();

        outroIsPlaying = false;
        
        // Volitelně vypneme objekt celé outro scény/canvasu
        gameObject.SetActive(false);
    }

    private IEnumerator FadeText(float startAlpha, float endAlpha)
    {
        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeDuration;
            textComponent.alpha = Mathf.Lerp(startAlpha, endAlpha, t);
            yield return null;
        }
        textComponent.alpha = endAlpha;
    }

    private IEnumerator FadeOverlayOut()
    {
        float startAlpha = overlayCanvasGroup.alpha;
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeDuration;
            overlayCanvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, t);
            yield return null;
        }

        overlayCanvasGroup.alpha = 0f;
        overlayCanvasGroup.blocksRaycasts = false;
    }
}

