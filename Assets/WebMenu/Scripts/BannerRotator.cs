using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class BannerRotator : MonoBehaviour
{
    [SerializeField] private Image image;
    [SerializeField] private Sprite[] banners;
    [SerializeField] private float intervalSeconds = 4f;

    [Header("Optional Fade")]
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private float fadeSeconds = 0.25f;

    private int index;
    private Coroutine routine;

    private void Awake()
    {
        if (!image) image = GetComponent<Image>();
        if (!canvasGroup) canvasGroup = GetComponent<CanvasGroup>();
    }

    private void OnEnable()
    {
        if (banners == null || banners.Length == 0 || !image) return;

        index = 0;
        image.sprite = banners[index];
        routine = StartCoroutine(Loop());
    }

    private void OnDisable()
    {
        if (routine != null) StopCoroutine(routine);
    }

    private IEnumerator Loop()
    {
        while (true)
        {
            yield return new WaitForSecondsRealtime(intervalSeconds);
            index = (index + 1) % banners.Length;

            if (canvasGroup && fadeSeconds > 0.01f)
            {
                yield return Fade(1f, 0f, fadeSeconds);
                image.sprite = banners[index];
                yield return Fade(0f, 1f, fadeSeconds);
            }
            else
            {
                image.sprite = banners[index];
            }
        }
    }

    private IEnumerator Fade(float from, float to, float t)
    {
        float e = 0f;
        canvasGroup.alpha = from;
        while (e < t)
        {
            e += Time.unscaledDeltaTime;
            canvasGroup.alpha = Mathf.Lerp(from, to, e / t);
            yield return null;
        }
        canvasGroup.alpha = to;
    }
}