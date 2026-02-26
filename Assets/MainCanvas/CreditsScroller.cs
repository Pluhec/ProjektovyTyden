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

    private Coroutine runRoutine;

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

        if (loadSceneWhenFinished)
            SceneManager.LoadScene(gameSceneName);
        else
            gameObject.SetActive(false);
    }
}