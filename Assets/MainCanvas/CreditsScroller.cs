using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CreditsScroller : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private RectTransform viewport; // viditelná oblast (ideálně panel s Mask/RectMask2D)
    [SerializeField] private RectTransform content;  // obsah titulků (tvůj "div")

    [Header("Optional end marker (recommended)")]
    [SerializeField] private RectTransform endMarker; // poslední prvek (třeba obrázek dole)

    [Header("Scroll")]
    [SerializeField] private float speed = 100f;      
    [SerializeField] private float startPadding = 50f; 
    [SerializeField] private float endPadding = 50f;   

    [Header("Finish")]
    [SerializeField] private bool loadSceneWhenFinished = true;
    [SerializeField] private string gameSceneName = "MainMenu";

    [Header("Audio")]
    [SerializeField] private AudioSource creditsAudioSource;
    [SerializeField, Range(0f, 1f)] private float targetMusicVolume = 0.8f;
    [SerializeField, Min(0f)] private float fadeInDuration = 1.5f;
    [SerializeField, Min(0f)] private float fadeOutDuration = 1.5f;

    private Coroutine runRoutine;
    private Coroutine audioFadeRoutine;

    public void PlayCredits()
    {
        gameObject.SetActive(true);

        if (runRoutine != null)
            StopCoroutine(runRoutine);

        runRoutine = StartCoroutine(Run());
    }

    public void StopCredits(bool disableObject = false)
    {
        if (runRoutine != null)
        {
            StopCoroutine(runRoutine);
            runRoutine = null;
        }

        StopAudioFadeRoutine();
        if (creditsAudioSource != null)
        {
            creditsAudioSource.Stop();
            creditsAudioSource.volume = 0f;
        }

        if (disableObject)
            gameObject.SetActive(false);
    }

    private IEnumerator Run()
    {
        // přepočet layoutu (TMP + LayoutGroup apod.)
        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(content);
        yield return null; // často pomůže, když se UI dopočítá až další frame
        Canvas.ForceUpdateCanvases();

        // Nastavení startu (tady klidně necháme podle výšky contentu)
        float viewportH = viewport.rect.height;
        float contentH = content.rect.height;

        float startY = -(viewportH * 0.5f) - (contentH * 0.5f) - startPadding;

        var pos = content.anchoredPosition;
        pos.y = startY;
        content.anchoredPosition = pos;

        StartCreditsAudio();

        // --- Scroll dokud endMarker nevyjede nahoru ---
        // Pokud endMarker není nastavený, fallback na původní výpočet endY
        if (endMarker == null)
        {
            float endY = (viewportH * 0.5f) + (contentH * 0.5f) + endPadding;

            while (content.anchoredPosition.y < endY)
            {
                content.anchoredPosition += Vector2.up * (speed * Time.deltaTime);
                yield return null;
            }
        }
        else
        {
            // robustní kontrola přes world corners (nezávislé na pivotech/anchorech)
            Vector3[] vCorners = new Vector3[4];
            Vector3[] mCorners = new Vector3[4];

            while (true)
            {
                content.anchoredPosition += Vector2.up * (speed * Time.deltaTime);

                viewport.GetWorldCorners(vCorners);
                endMarker.GetWorldCorners(mCorners);

                // vCorners[1] = horní levý roh viewportu
                float viewportTopY = vCorners[1].y;

                // mCorners[0] = dolní levý roh markeru (tj. spodní hrana obrázku)
                float markerBottomY = mCorners[0].y;

                // konec, když je celý marker (obrázek) nad horní hranou viewportu + padding
                if (markerBottomY > viewportTopY + endPadding)
                    break;

                yield return null;
            }
        }

        runRoutine = null;

        if (creditsAudioSource != null && creditsAudioSource.isPlaying)
        {
            StopAudioFadeRoutine();
            yield return FadeAudio(creditsAudioSource, 0f, fadeOutDuration);
            creditsAudioSource.Stop();
        }

        if (loadSceneWhenFinished)
            SceneManager.LoadScene(gameSceneName);
        else
            gameObject.SetActive(false);
    }

    private void StartCreditsAudio()
    {
        if (creditsAudioSource == null)
            return;

        StopAudioFadeRoutine();
        creditsAudioSource.volume = 0f;
        if (!creditsAudioSource.isPlaying)
            creditsAudioSource.Play();

        audioFadeRoutine = StartCoroutine(FadeAudio(creditsAudioSource, targetMusicVolume, fadeInDuration));
    }

    private IEnumerator FadeAudio(AudioSource source, float targetVolume, float duration)
    {
        if (source == null)
            yield break;

        float from = source.volume;
        if (duration <= 0f)
        {
            source.volume = targetVolume;
            yield break;
        }

        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            source.volume = Mathf.Lerp(from, targetVolume, Mathf.Clamp01(t / duration));
            yield return null;
        }

        source.volume = targetVolume;
        audioFadeRoutine = null;
    }

    private void StopAudioFadeRoutine()
    {
        if (audioFadeRoutine == null)
            return;

        StopCoroutine(audioFadeRoutine);
        audioFadeRoutine = null;
    }
}
