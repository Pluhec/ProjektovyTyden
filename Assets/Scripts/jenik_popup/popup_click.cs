using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class popup_click : MonoBehaviour
{
    [Header("References")]
    public GameObject particles;
    public GameObject ukazatel;
    
    [Header("Settings")]
    public Gradient timerGradient;
    public float lifetime = 15f;

    [Header("Animation & Pivot")]
    [Tooltip("World-space offset from the center to the bottom tip. Used to scale from the bottom. Example: (0, -0.5, 0)")]
    public Vector3 pivotOffset = new Vector3(0f, -0.5f, 0f);
    public float hoverHeight = 0.02f;
    public float hoverSpeed = 1.5f;

    [Header("Near-End Pulse")]
    public bool pulsePopupBody = false;
    public float pulseStartSeconds = 3f;
    public float pulseSpeed = 8f;
    [Range(0f, 1f)]
    public float pulseRedBlend = 0.55f;
    public Color pulseColor = Color.red;
    public bool pulseScale = true;
    public float pulseScaleAmount = 1.1f;

    [Header("Particle Colors")]
    public Color particleClickedColor = Color.white;
    public Color particleTimeoutColor = Color.black;

    [Header("Camera Zoom Scaling")]
    public bool scaleWithZoom = true;
    [Tooltip("The orthographic size where the popup is at its normal (1x) scale.")]
    public float baseOrthographicSize = 5f;
    [Tooltip("How much the popup scales up as you zoom out. 0 = no scaling, 1 = scales exactly with zoom.")]
    [Range(0f, 1f)]
    public float zoomScaleFactor = 0.5f;

    private Vector2 mousePos;
    private bool isInteractable = false;
    private bool isDying = false;
    private bool isMouseOver = false;
    
    private Vector3 initialScale;
    private Vector3 ukazatelInitialScale;
    private Vector3 initialPosition;

    // Animation states
    private Vector3 currentScale;
    private Vector3 currentHoverOffset;
    private float mouseHoverScaleMultiplier = 1f;
    private float pulseScaleMultiplier = 1f;

    private SpriteRenderer ukazatelRenderer;
    private SpriteRenderer bodyRenderer;
    private IconShower iconShower;
    private SpriteRenderer iconRenderer;
    private Color bodyBaseColor;
    private Color iconBaseColor;

    void Start()
    {
        initialScale = transform.localScale;
        initialPosition = transform.position;
        if (ukazatel != null) 
        {
            ukazatelInitialScale = ukazatel.transform.localScale;
            ukazatelRenderer = ukazatel.GetComponent<SpriteRenderer>();
        }

        bodyRenderer = GetComponent<SpriteRenderer>();
        iconShower = GetComponent<IconShower>();
        if (iconShower != null)
        {
            iconRenderer = iconShower.iconRenderer;
        }

        if (bodyRenderer != null)
        {
            bodyBaseColor = bodyRenderer.color;
        }
        if (iconRenderer != null)
        {
            iconBaseColor = iconRenderer.color;
        }
        
        currentScale = Vector3.zero;
        transform.localScale = Vector3.zero;
        
        StartCoroutine(SpawnAnimation());
        StartCoroutine(SmoothTimer());
        StartCoroutine(HoverAnimation());
    }

    void Update()
    {
        if (!isInteractable || isDying) return;

        mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        
        // Check hover
        RaycastHit2D[] hits = Physics2D.RaycastAll(mousePos, Vector2.zero);
        isMouseOver = false;
        foreach (var hit in hits)
        {
            if (hit.collider != null && hit.collider.gameObject == gameObject)
            {
                isMouseOver = true;
                break;
            }
        }

        // Smoothly adjust hover multiplier for a nice pop when mouse is over
        float targetHoverMult = isMouseOver ? 1.02f : 1f;
        mouseHoverScaleMultiplier = Mathf.Lerp(mouseHoverScaleMultiplier, targetHoverMult, Time.deltaTime * 10f);

        if(isMouseOver && Input.GetMouseButtonDown(0))
        {
            EvaluateOutcome();
            StartCoroutine(DeathAnimation(true));
        }
    }

    void LateUpdate()
    {
        // Calculate camera zoom scale multiplier
        float cameraZoomMultiplier = 1f;
        if (scaleWithZoom && Camera.main != null && Camera.main.orthographic)
        {
            float currentOrthoSize = Camera.main.orthographicSize;
            // If current size is larger than base, we are zoomed out.
            // We lerp between 1 (no scale change) and the actual ratio based on zoomScaleFactor.
            float ratio = currentOrthoSize / baseOrthographicSize;
            cameraZoomMultiplier = Mathf.Lerp(1f, ratio, zoomScaleFactor);
        }

        // 1. Apply scale with hover, pulse, and camera zoom multipliers
        float totalScaleMultiplier = mouseHoverScaleMultiplier * pulseScaleMultiplier * cameraZoomMultiplier;
        Vector3 finalScale = Vector3.Scale(currentScale, new Vector3(totalScaleMultiplier, totalScaleMultiplier, 1f));
        transform.localScale = finalScale;

        // 2. Apply pivot offset math (keeps the bottom point stationary)
        Vector3 anchorWorldPos = initialPosition + pivotOffset;
        Vector3 scaleRatio = new Vector3(
            initialScale.x != 0 ? finalScale.x / initialScale.x : 0,
            initialScale.y != 0 ? finalScale.y / initialScale.y : 0,
            initialScale.z != 0 ? finalScale.z / initialScale.z : 0
        );
        Vector3 newCenter = anchorWorldPos - Vector3.Scale(pivotOffset, scaleRatio);
        
        // 3. Add hover offset
        transform.position = newCenter + currentHoverOffset;
    }

    IEnumerator SpawnAnimation()
    {
        float duration = 0.4f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            
            // EaseOutCubic for a smooth, professional slide/scale in
            float ease = 1f - Mathf.Pow(1f - t, 3f);
            
            currentScale = initialScale * ease;

            yield return null;
        }
        
        currentScale = initialScale;
        isInteractable = true;
    }

    IEnumerator HoverAnimation()
    {
        float randomOffset = Random.Range(0f, 100f);
        
        while (!isDying)
        {
            float time = Time.time + randomOffset;
            // Very gentle floating
            currentHoverOffset = new Vector3(0, Mathf.Sin(time * hoverSpeed) * hoverHeight, 0);
            yield return null;
        }
    }

    IEnumerator SmoothTimer()
    {
        float elapsed = 0f;

        while (elapsed < lifetime && !isDying)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / lifetime;

            if (ukazatel != null)
            {
                float targetY = Mathf.Max(0f, ukazatelInitialScale.y - 0.3f);
                ukazatel.transform.localScale = new Vector3(ukazatelInitialScale.x, Mathf.Lerp(ukazatelInitialScale.y, targetY, t), ukazatelInitialScale.z);
                
                if (ukazatelRenderer != null)
                {
                    ukazatelRenderer.color = timerGradient.Evaluate(t);
                }
            }

            float timeLeft = lifetime - elapsed;
            UpdateNearEndPulse(timeLeft);
            yield return null;
        }

        if (!isDying)
        {
            StartCoroutine(DeathAnimation(false));
        }

        ResetPulseColors();
    }

    private void UpdateNearEndPulse(float timeLeft)
    {
        if (timeLeft > pulseStartSeconds)
        {
            ResetPulseColors();
            pulseScaleMultiplier = 1f;
            return;
        }

        float pulse = (Mathf.Sin(Time.time * pulseSpeed) + 1f) * 0.5f;
        float nearEndFactor = 1f - Mathf.Clamp01(timeLeft / Mathf.Max(0.01f, pulseStartSeconds));
        float blend = pulse * pulseRedBlend * Mathf.Lerp(0.6f, 1f, nearEndFactor);

        if (pulseScale)
        {
            // Scale pulses between 1.0 and pulseScaleAmount based on the pulse wave and how close to the end it is
            float currentPulseScale = Mathf.Lerp(1f, pulseScaleAmount, pulse * nearEndFactor);
            pulseScaleMultiplier = currentPulseScale;
        }
        else
        {
            pulseScaleMultiplier = 1f;
        }

        if (iconRenderer != null)
        {
            iconRenderer.color = Color.Lerp(iconBaseColor, pulseColor, blend);
        }

        if (pulsePopupBody && bodyRenderer != null)
        {
            bodyRenderer.color = Color.Lerp(bodyBaseColor, pulseColor, blend * 0.75f);
        }
    }

    private void ResetPulseColors()
    {
        if (iconRenderer != null)
        {
            iconRenderer.color = iconBaseColor;
        }

        if (bodyRenderer != null)
        {
            bodyRenderer.color = bodyBaseColor;
        }
    }

    IEnumerator DeathAnimation(bool clicked)
    {
        isDying = true;
        isInteractable = false;
        ResetPulseColors();

        float duration = 0.25f;
        float elapsed = 0f;
        Vector3 startScale = currentScale;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            
            // EaseInCubic for a smooth shrink out
            float ease = t * t * t;

            currentScale = Vector3.Lerp(startScale, Vector3.zero, ease);

            yield return null;
        }

        if (particles != null)
        {
            Vector3 spawnPos = transform.position;
            spawnPos.z = -10f; 
            
            GameObject particl = Instantiate(particles, spawnPos, Quaternion.identity);
            ParticleSystem ps = particl.GetComponent<ParticleSystem>();
            ParticleSystem.MainModule main = ps.main;
            main.startColor = clicked ? particleClickedColor : particleTimeoutColor;
            particl.GetComponent<ParticleSystemRenderer>().sortingOrder = 32767;
        }

        Destroy(gameObject);
    }

    public void EvaluateOutcome()
    {
        switch (GetComponent<IconShower>().GetCurrentIconType()) {
            case IconType.Money:

            Debug.Log("Money popup clicked!");
            // TODO: Implement money effect, show popup text, etc.

                break;
            case IconType.Power:

            Debug.Log("Power popup clicked!");
            // TODO: Implement power-up effect, show popup text, etc.

                break;
        }
    }
}
