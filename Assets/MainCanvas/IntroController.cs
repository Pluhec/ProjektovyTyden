using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.Events;
using UnityEngine.InputSystem; // Přidáno pro nový Input System

public class IntroController : MonoBehaviour
{
    [Header("UI Components")]
    [Tooltip("CanvasGroup celého pozadí (černá obrazovka), aby mohlo zmizet na konci.")]
    public CanvasGroup backgroundCanvasGroup;
    
    [Tooltip("TextMeshPro komponenta, kde se budou měnit texty.")]
    public TextMeshProUGUI textComponent;

    [Header("Settings")]
    public float fadeDuration = 1.0f;    // Jak dlouho trvá rozednění/zatmění textu
    public float displayDuration = 2.0f; // Jak dlouho text svítí
    [TextArea(2, 5)]
    public string[] messages;            // Seznam zpráv

    [Header("Events")]
    public UnityEvent onIntroFinished;   // Co se stane po skončení (např. zapnout MainCanvas)

    private bool skipRequested = false;

    private void Update()
    {
        // Kontrola vstupu - podpora pro starý i nový Input System
        bool spacePressed = false;
        bool mousePressed = false;

        // 1. Zkusíme nový Input System (pokud je dostupný)
        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame) spacePressed = true;
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame) mousePressed = true;

        // 2. Fallback na starý Input System (pokud funguje)
        // Toto volání může házet error, pokud je starý systém vypnutý, proto to obalíme nebo použijeme jen když nový nezafungoval
        // Pro jednoduchost a bezpečnost necháme starý systém jen jako doplněk, pokud nový nic nezachytil, 
        // ale v praxi, pokud je "Active Input Handling" nastaven na "Input System Package (New)", staré API nefunguje.
        
        // Jednodušší přístup: Pokud New Input System neexistuje (null), zkusíme starý.
        if (!spacePressed && !mousePressed)
        {
            try 
            {
                if (Input.GetKeyDown(KeyCode.Space)) spacePressed = true;
                if (Input.GetMouseButtonDown(0)) mousePressed = true;
            }
            catch { /* Ignorujeme chyby pokud je starý systém vypnutý */ }
        }

        if (spacePressed || mousePressed)
        {
            skipRequested = true;
        }
    }

    private void Start()
    {
        if (backgroundCanvasGroup == null) backgroundCanvasGroup = GetComponent<CanvasGroup>();
        
        // Spustíme sekvenci
        StartCoroutine(PlayIntroSequence());
    }

    private IEnumerator PlayIntroSequence()
    {
        // Zajistíme, že pozadí je neprůhledné (černé)
        backgroundCanvasGroup.alpha = 1f;
        backgroundCanvasGroup.blocksRaycasts = true; // Blokuje klikání skrz intro

        // Text na začátku neviditelný
        textComponent.alpha = 0f;

        // Projdeme všechny zprávy v poli
        foreach (string msg in messages)
        {
            textComponent.text = msg;
            skipRequested = false; // Reset skipu pro novou zprávu

            // 1. Fade In Text
            yield return StartCoroutine(FadeText(0f, 1f));
            
            // Pokud byl skip během Fade In, zajistíme plnou viditelnost a jdeme dál
            if (skipRequested)
            {
                textComponent.alpha = 1f;
                // Pokud chceš, aby skip přeskočil i čtení a rovnou text zmizel, odkomentuj:
                // goto SkipToFadeOut; 
                // Pokud chceš, aby se skipnutím jen text objevil a čekal na přečtení (nebo další klik), necháme to takto,
                // ale uživatel chtěl "skipne blok", tak pravděpodobně chce jít rovnou na další.
            }
            else
            {
                // 2. Wait - čekáme a zároveň kontrolujeme skip
                float timer = 0f;
                while (timer < displayDuration)
                {
                    if (skipRequested) break; // Přeskočíme čekání
                    timer += Time.deltaTime;
                    yield return null;
                }
            }

            // SkipToFadeOut: (pro případ goto)

            // 3. Fade Out Text (pokud uživatel skipnul zobrazení, chceme aby to zmizelo rychleji? 
            // Podle zadání "blok skipne" -> uděláme rychlý fade out nebo instantní zmizení)
            if (skipRequested)
            {
                // Rychlé zmizení textu při skipu
                textComponent.alpha = 0f; 
            }
            else
            {
                yield return StartCoroutine(FadeText(1f, 0f));
            }
            
            // Krátká pauza mezi texty
            yield return new WaitForSeconds(0.5f);
        }

        // Po posledním textu - Fade Out celého pozadí
        yield return StartCoroutine(FadeBackgroundOut());

        // Hotovo - vypneme interakci a zavoláme event
        backgroundCanvasGroup.blocksRaycasts = false;
        onIntroFinished.Invoke();
        
        // Volitelně můžeme tento objekt deaktivovat
        gameObject.SetActive(false);
    }

    private IEnumerator FadeText(float startAlpha, float endAlpha)
    {
        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            if (skipRequested) break; // Pokud uživatel klikne, přerušíme fade

            elapsed += Time.deltaTime;
            float t = elapsed / fadeDuration;
            textComponent.alpha = Mathf.Lerp(startAlpha, endAlpha, t);
            yield return null;
        }
        if (!skipRequested) textComponent.alpha = endAlpha;
    }

    private IEnumerator FadeBackgroundOut()
    {
        float startAlpha = backgroundCanvasGroup.alpha;
        float elapsed = 0f;
        
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeDuration;
            backgroundCanvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, t);
            yield return null;
        }
        backgroundCanvasGroup.alpha = 0f;
    }
}
