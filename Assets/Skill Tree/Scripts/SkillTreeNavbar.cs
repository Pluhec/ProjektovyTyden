using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class SkillTreeNavbar : MonoBehaviour
{
    [System.Serializable]
    public class NavbarSection
    {
        public Button button;
        public GameObject section;
        public Sprite activeSprite;
        public Sprite inactiveSprite;
        [HideInInspector] public CanvasGroup canvasGroup;
        [HideInInspector] public RectTransform rectTransform;
        [HideInInspector] public Vector2 originalPosition;
        [HideInInspector] public Vector3 originalScale;
    }

    [Header("Sections")]
    public NavbarSection section1;
    public NavbarSection section2;
    public NavbarSection section3;

    [Header("Animation Settings")]
    public float fadeDuration = 0.4f;
    public float moveOffset = 40f; // How far it moves down
    public float scaleOffset = 0.95f; // How much it scales down

    private NavbarSection[] _sections;
    private NavbarSection _currentSection;
    private Coroutine _transitionCoroutine;

    private void Awake()
    {
        _sections = new NavbarSection[] { section1, section2, section3 };

        foreach (var s in _sections)
        {
            if (s?.section != null)
            {
                s.canvasGroup = s.section.GetComponent<CanvasGroup>();
                if (s.canvasGroup == null)
                {
                    s.canvasGroup = s.section.AddComponent<CanvasGroup>();
                }
                
                s.rectTransform = s.section.GetComponent<RectTransform>();
                if (s.rectTransform != null)
                {
                    s.originalPosition = s.rectTransform.anchoredPosition;
                    s.originalScale = s.rectTransform.localScale;
                }

                // Initialize all to hidden
                s.canvasGroup.alpha = 0f;
                s.section.SetActive(false);
            }
        }

        section1.button.onClick.AddListener(() => OpenSection(section1));
        section2.button.onClick.AddListener(() => OpenSection(section2));
        section3.button.onClick.AddListener(() => OpenSection(section3));

        // Defaultně otevři první sekci bez animace
        if (section1 != null && section1.section != null)
        {
            _currentSection = section1;
            section1.section.SetActive(true);
            section1.canvasGroup.alpha = 1f;
            UpdateButtonSprites();
        }
    }

    private void OpenSection(NavbarSection target)
    {
        if (target == null || target == _currentSection) return;

        if (_transitionCoroutine != null)
        {
            StopCoroutine(_transitionCoroutine);
        }

        _transitionCoroutine = StartCoroutine(TransitionSections(_currentSection, target));
        _currentSection = target;
        UpdateButtonSprites();
    }

    private void UpdateButtonSprites()
    {
        foreach (var s in _sections)
        {
            if (s?.button == null) continue;

            bool isActive = s == _currentSection;
            Image btnImage = s.button.GetComponent<Image>();
            if (btnImage != null)
                btnImage.sprite = isActive ? s.activeSprite : s.inactiveSprite;
        }
    }

    private IEnumerator TransitionSections(NavbarSection from, NavbarSection to)
    {
        // 1. Animate outgoing section first
        if (from != null && from.section != null && from.canvasGroup != null)
        {
            float elapsed = 0f;
            float startAlpha = from.canvasGroup.alpha;
            Vector2 startPos = from.rectTransform != null ? from.rectTransform.anchoredPosition : Vector2.zero;
            Vector3 startScale = from.rectTransform != null ? from.rectTransform.localScale : Vector3.one;

            while (elapsed < fadeDuration / 2f)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / (fadeDuration / 2f));
                
                // EaseIn for going out (starts slow, speeds up)
                float easeT = t * t * t;

                from.canvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, t);
                if (from.rectTransform != null)
                {
                    from.rectTransform.anchoredPosition = Vector2.Lerp(startPos, from.originalPosition + new Vector2(0, -moveOffset), easeT);
                    from.rectTransform.localScale = Vector3.Lerp(startScale, from.originalScale * scaleOffset, easeT);
                }
                yield return null;
            }

            from.canvasGroup.alpha = 0f;
            from.section.SetActive(false);
            if (from.rectTransform != null)
            {
                from.rectTransform.anchoredPosition = from.originalPosition;
                from.rectTransform.localScale = from.originalScale;
            }
        }

        // 2. Animate incoming section after the first one is gone
        if (to != null && to.section != null && to.canvasGroup != null)
        {
            to.section.SetActive(true);
            to.canvasGroup.alpha = 0f;
            if (to.rectTransform != null)
            {
                to.rectTransform.anchoredPosition = to.originalPosition + new Vector2(0, -moveOffset);
                to.rectTransform.localScale = to.originalScale * scaleOffset;
            }

            float elapsed = 0f;
            while (elapsed < fadeDuration / 2f)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / (fadeDuration / 2f));
                
                // EaseOut for coming in (starts fast, slows down)
                float easeT = 1f - Mathf.Pow(1f - t, 3f);

                to.canvasGroup.alpha = Mathf.Lerp(0f, 1f, t);
                if (to.rectTransform != null)
                {
                    to.rectTransform.anchoredPosition = Vector2.Lerp(to.originalPosition + new Vector2(0, -moveOffset), to.originalPosition, easeT);
                    to.rectTransform.localScale = Vector3.Lerp(to.originalScale * scaleOffset, to.originalScale, easeT);
                }
                yield return null;
            }

            to.canvasGroup.alpha = 1f;
            if (to.rectTransform != null)
            {
                to.rectTransform.anchoredPosition = to.originalPosition;
                to.rectTransform.localScale = to.originalScale;
            }
        }
    }
}