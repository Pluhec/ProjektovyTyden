using System.Collections;
using System.Collections.Generic;
using PlayerChoice.DataSets;
using UnityEngine;

public enum PowerInfluenceMode
{
    FixedFullRed,
    FixedFullBlue,
    SampledRange
}

public class popup_click : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Particle prefab spawned when the popup disappears (clicked or timed out).")]
    public GameObject particles;
    [Tooltip("Reference to the timer indicator object that shrinks over popup lifetime.")]
    public GameObject ukazatel;
    
    [Header("Settings")]
    [Tooltip("Gradient used to color the timer indicator over time.")]
    public Gradient timerGradient;
    [Tooltip("How long (in seconds) the popup stays alive before timing out.")]
    public float lifetime = 15f;

    [Header("Animation & Pivot")]
    [Tooltip("World-space offset from the center to the bottom tip. Used to scale from the bottom. Example: (0, -0.5, 0)")]
    public Vector3 pivotOffset = new Vector3(0f, -0.5f, 0f);
    [Tooltip("Vertical floating amount for idle hover animation.")]
    public float hoverHeight = 0.02f;
    [Tooltip("Speed of idle hover animation.")]
    public float hoverSpeed = 1.5f;

    [Header("Near-End Pulse")]
    [Tooltip("If enabled, the popup body also pulses color near the end of lifetime.")]
    public bool pulsePopupBody = false;
    [Tooltip("How many seconds before timeout the pulse effect starts.")]
    public float pulseStartSeconds = 3f;
    [Tooltip("Pulse animation speed.")]
    public float pulseSpeed = 8f;
    [Range(0f, 1f)]
    [Tooltip("How strongly the pulse blends toward pulse color.")]
    public float pulseRedBlend = 0.55f;
    [Tooltip("Color used for near-end pulsing.")]
    public Color pulseColor = Color.red;
    [Tooltip("If enabled, popup also pulses in scale near end of lifetime.")]
    public bool pulseScale = true;
    [Tooltip("Maximum scale multiplier used by pulse scaling.")]
    public float pulseScaleAmount = 1.1f;

    [Header("Particle Colors")]
    [Tooltip("Particle color when popup is clicked.")]
    public Color particleClickedColor = Color.white;
    [Tooltip("Particle color when popup expires naturally.")]
    public Color particleTimeoutColor = Color.black;

    [Header("Camera Zoom Scaling")]
    [Tooltip("If enabled, popup scales with orthographic camera zoom.")]
    public bool scaleWithZoom = true;
    [Tooltip("The orthographic size where the popup is at its normal (1x) scale.")]
    public float baseOrthographicSize = 5f;
    [Tooltip("How much the popup scales up as you zoom out. 0 = no scaling, 1 = scales exactly with zoom.")]
    [Range(0f, 1f)]
    public float zoomScaleFactor = 0.5f;
    [Tooltip("If enabled, spawned particles also scale with camera zoom.")]
    public bool particlesScaleWithZoom = true;
    [Tooltip("How strongly particles follow camera zoom. 0 = fixed size, 1 = same zoom scaling as popup.")]
    [Range(0f, 1f)]
    public float particleZoomScaleFactor = 1f;

    [Header("Power Effect")]
    [Tooltip("Influence radius (in grid cells) applied when a power popup is clicked.")]
    public float powerInfluenceRadius = 12f;
    [Tooltip("Base additive influence strength applied to simulation stance values.")]
    public float powerInfluenceStrength = 2.0f;
    [Tooltip("Manual falloff profile from center (x=0) to edge (x=1). y=1 means full influence, y=0 means no influence.")]
    public AnimationCurve powerInfluenceFalloffCurve = new AnimationCurve(
        new Keyframe(0f, 1f),
        new Keyframe(1f, 0f)
    );
    [Tooltip("How many curve samples are used to build a smooth influence profile. Higher = smoother.")]
    [Range(4, 64)] public int powerInfluenceCurveSamples = 20;
    [Tooltip("How this popup chooses ideology influence: fixed full red, fixed full blue, or sampled from range.")]
    public PowerInfluenceMode powerInfluenceMode = PowerInfluenceMode.FixedFullRed;
    [Tooltip("Minimum ideology sample for this popup instance (0 = blue, 0.5 = neutral, 1 = red).")]
    [Range(0f, 1f)] public float powerInfluenceMin = 1f;
    [Tooltip("Maximum ideology sample for this popup instance (0 = blue, 0.5 = neutral, 1 = red).")]
    [Range(0f, 1f)] public float powerInfluenceMax = 1f;

    [Header("Power Expand Animation")]
    [Tooltip("Duration (seconds) for the power influence to expand from center to edges after the popup disappears.")]
    public float powerExpandDuration = 0.8f;

    private float sampledPowerInfluenceValue = 1f;

    // Pending animated power influence data (computed on click, applied after death animation)
    private bool hasPendingPowerInfluence = false;
    private SimulationHandler pendingSimHandler;
    private int pendingGridX, pendingGridY;
    private float pendingSignedStrength;
    private float pendingCellAspectXY;
    private float[] pendingRingWeights;
    private int[] pendingRingRadii;
    private float pendingTotalWeight;

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

        InitializePowerInfluenceValue();
        
        currentScale = Vector3.zero;
        transform.localScale = Vector3.zero;
        
        StartCoroutine(SpawnAnimation());
        StartCoroutine(SmoothTimer());
        StartCoroutine(HoverAnimation());
    }

    private void InitializePowerInfluenceValue()
    {
        switch (powerInfluenceMode)
        {
            case PowerInfluenceMode.FixedFullBlue:
                sampledPowerInfluenceValue = 0f;
                Debug.Log("Popup spawned with fixed full BLUE influence value 0.000");
                return;

            case PowerInfluenceMode.FixedFullRed:
                sampledPowerInfluenceValue = 1f;
                Debug.Log("Popup spawned with fixed full RED influence value 1.000");
                return;

            case PowerInfluenceMode.SampledRange:
            default:
                float min = Mathf.Clamp01(powerInfluenceMin);
                float max = Mathf.Clamp01(powerInfluenceMax);

                if (min > max)
                {
                    float temp = min;
                    min = max;
                    max = temp;
                }

                sampledPowerInfluenceValue = Random.Range(min, max);
                Debug.Log($"Popup spawned with sampled power influence value {sampledPowerInfluenceValue:F3} (range {min:F3}-{max:F3}, 0=blue, 0.5=neutral, 1=red)");
                return;
        }
    }

    private float GetCameraZoomMultiplier()
    {
        if (scaleWithZoom && Camera.main != null && Camera.main.orthographic)
        {
            float currentOrthoSize = Camera.main.orthographicSize;
            float ratio = currentOrthoSize / baseOrthographicSize;
            return Mathf.Lerp(1f, ratio, zoomScaleFactor);
        }

        return 1f;
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
        float cameraZoomMultiplier = GetCameraZoomMultiplier();

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
            if (particlesScaleWithZoom)
            {
                float cameraZoomMultiplier = GetCameraZoomMultiplier();
                float particleZoomMultiplier = Mathf.Lerp(1f, cameraZoomMultiplier, particleZoomScaleFactor);
                particl.transform.localScale = particl.transform.localScale * particleZoomMultiplier;
            }
            ParticleSystem ps = particl.GetComponent<ParticleSystem>();
            ParticleSystem.MainModule main = ps.main;
            main.startColor = clicked ? particleClickedColor : particleTimeoutColor;
            particl.GetComponent<ParticleSystemRenderer>().sortingOrder = 32767;
        }

        // Start animated power influence expansion on the SimulationHandler
        // (it survives this object's destruction)
        if (clicked && hasPendingPowerInfluence && pendingSimHandler != null)
        {
            pendingSimHandler.StartCoroutine(AnimatedPowerExpand(
                pendingSimHandler, pendingGridX, pendingGridY,
                pendingSignedStrength, pendingCellAspectXY,
                pendingRingWeights, pendingRingRadii, pendingTotalWeight,
                powerExpandDuration));
        }

        Destroy(gameObject);
    }

    private Vector3 GetBottomTipWorldPosition()
    {
        float cameraZoomMultiplier = GetCameraZoomMultiplier();

        float totalScaleMultiplier = mouseHoverScaleMultiplier * pulseScaleMultiplier * cameraZoomMultiplier;
        Vector3 finalScale = Vector3.Scale(currentScale, new Vector3(totalScaleMultiplier, totalScaleMultiplier, 1f));
        Vector3 scaleRatio = new Vector3(
            initialScale.x != 0 ? finalScale.x / initialScale.x : 0,
            initialScale.y != 0 ? finalScale.y / initialScale.y : 0,
            initialScale.z != 0 ? finalScale.z / initialScale.z : 0
        );

        return transform.position + Vector3.Scale(pivotOffset, scaleRatio);
    }

    private void PrecomputePowerRings(int targetRadius)
    {
        int clampedRadius = Mathf.Max(1, targetRadius);
        int sampleCount = Mathf.Max(4, powerInfluenceCurveSamples);

        pendingRingWeights = new float[sampleCount];
        pendingRingRadii = new int[sampleCount];
        pendingTotalWeight = 0f;

        for (int index = sampleCount - 1; index >= 0; index--)
        {
            float t = sampleCount <= 1 ? 0f : index / (float)(sampleCount - 1);
            float nextT = index >= sampleCount - 1 ? 1f : (index + 1) / (float)(sampleCount - 1);

            float profile = Mathf.Clamp01(powerInfluenceFalloffCurve.Evaluate(t));
            float nextProfile = index >= sampleCount - 1 ? 0f : Mathf.Clamp01(powerInfluenceFalloffCurve.Evaluate(nextT));
            float ringWeight = Mathf.Max(0f, profile - nextProfile);

            int radius = Mathf.Max(1, Mathf.RoundToInt(Mathf.Lerp(1f, clampedRadius, t)));

            pendingRingWeights[index] = ringWeight;
            pendingRingRadii[index] = radius;
            pendingTotalWeight += ringWeight;
        }
    }

    private static IEnumerator AnimatedPowerExpand(
        SimulationHandler simHandler, int gridX, int gridY,
        float signedStrength, float cellAspectXY,
        float[] ringWeights, int[] ringRadii, float totalWeight,
        float expandDuration)
    {
        if (totalWeight <= 0.0001f)
            yield break;

        int sampleCount = ringWeights.Length;
        float elapsed = 0f;
        int lastAppliedIndex = -1;

        while (elapsed < expandDuration)
        {
            elapsed += Time.deltaTime;
            float progress = Mathf.Clamp01(elapsed / expandDuration);

            // EaseOutCubic for a smooth expansion that starts fast and decelerates
            float easedProgress = 1f - Mathf.Pow(1f - progress, 3f);

            // Determine which ring index we should have reached by now
            int targetIndex = Mathf.Min(Mathf.FloorToInt(easedProgress * sampleCount), sampleCount - 1);

            // Apply all rings from lastAppliedIndex+1 up to targetIndex
            while (lastAppliedIndex < targetIndex)
            {
                lastAppliedIndex++;
                float normalizedWeight = ringWeights[lastAppliedIndex] / totalWeight;
                float passStrength = signedStrength * normalizedWeight;
                if (Mathf.Abs(passStrength) > 0.0001f)
                {
                    simHandler.PaintStance(gridX, gridY, passStrength, ringRadii[lastAppliedIndex], cellAspectXY);
                }
            }

            yield return null;
        }

        // Apply any remaining rings that weren't reached
        while (lastAppliedIndex < sampleCount - 1)
        {
            lastAppliedIndex++;
            float normalizedWeight = ringWeights[lastAppliedIndex] / totalWeight;
            float passStrength = signedStrength * normalizedWeight;
            if (Mathf.Abs(passStrength) > 0.0001f)
            {
                simHandler.PaintStance(gridX, gridY, passStrength, ringRadii[lastAppliedIndex], cellAspectXY);
            }
        }
    }

    public void EvaluateOutcome()
    {
        switch (GetComponent<IconShower>().GetCurrentIconType()) {
            case IconType.Money:

            Debug.Log("Money popup clicked!");
            int moneyToAdd = Random.Range(1, 5);
            FloatingTextAnimator.SpawnMultiple(transform.position, moneyToAdd);

                break;
            case IconType.Power:

            Debug.Log("Power popup clicked!");
            SimulationHandler simHandler = FindFirstObjectByType<SimulationHandler>();
            if (simHandler != null)
            {
                // We need to find the grid position manually since we can't modify SimulationHandler
                Camera targetCamera = Camera.main;
                if (targetCamera != null && simHandler.simulationQuadTransform != null)
                {
                    Vector3 tipWorldPos = GetBottomTipWorldPosition();
                    Vector3 localPos = simHandler.simulationQuadTransform.InverseTransformPoint(tipWorldPos);
                    float u = localPos.x + 0.5f;
                    float v = localPos.y + 0.5f;
                    
                    if (u >= 0f && u < 1f && v >= 0f && v < 1f)
                    {
                        int gridX = Mathf.Clamp(Mathf.FloorToInt(u * simHandler.gridSize.x), 0, simHandler.gridSize.x - 1);
                        int gridY = Mathf.Clamp(Mathf.FloorToInt(v * simHandler.gridSize.y), 0, simHandler.gridSize.y - 1);

                        float signedFactor = (0.5f - sampledPowerInfluenceValue) * 2f;
                        float signedStrength = Mathf.Abs(powerInfluenceStrength) * signedFactor;
                        int targetRadius = Mathf.RoundToInt(powerInfluenceRadius);

                        // Compute cell aspect ratio so PaintStance draws a world-space circle
                        // rather than an oval when the simulation quad isn't perfectly square.
                        Vector3 quadScale = simHandler.simulationQuadTransform.lossyScale;
                        float cellWidth  = Mathf.Abs(quadScale.x) / simHandler.gridSize.x;
                        float cellHeight = Mathf.Abs(quadScale.y) / simHandler.gridSize.y;
                        float cellAspectXY = cellHeight > 0.0001f ? cellWidth / cellHeight : 1f;
                        
                        Debug.Log($"Power influence queued from tip {tipWorldPos} at grid ({gridX}, {gridY}) with radius {powerInfluenceRadius}, cellAspect {cellAspectXY:F3}, sampled value {sampledPowerInfluenceValue:F3}, signed factor {signedFactor:F3}, signed strength {signedStrength:F3}, curve samples {powerInfluenceCurveSamples}, expand duration {powerExpandDuration}s");

                        // Pre-compute ring data and store parameters;
                        // the animated expansion starts after the death animation
                        hasPendingPowerInfluence = true;
                        pendingSimHandler = simHandler;
                        pendingGridX = gridX;
                        pendingGridY = gridY;
                        pendingSignedStrength = signedStrength;
                        pendingCellAspectXY = cellAspectXY;
                        PrecomputePowerRings(targetRadius);
                    }
                    else
                    {
                        Debug.LogWarning($"Popup position (u:{u}, v:{v}) is outside the simulation grid bounds.");
                    }
                }
                else
                {
                    Debug.LogWarning("Camera.main or simHandler.simulationQuadTransform is null.");
                }
            }
            else
            {
                Debug.LogWarning("SimulationHandler not found in the scene.");
            }

                break;
        }
    }
}

public class FloatingTextAnimator : MonoBehaviour
{
    public static void SpawnMultiple(Vector3 position, int amount)
    {
        GameObject managerGo = new GameObject("FloatingTextManager");
        managerGo.transform.position = position;
        var manager = managerGo.AddComponent<FloatingTextAnimator>();
        manager.StartCoroutine(manager.SpawnSequence(position, amount));
    }

    private IEnumerator SpawnSequence(Vector3 position, int amount)
    {
        for (int i = 0; i < amount; i++)
        {
            SpawnSingle(position, 1);
            PlayerChoice.DataSets.PlayerStats.AddMoney((byte)1);
            yield return new WaitForSeconds(0.15f); // 0.15s delay between each coin
        }
        Destroy(gameObject); // Destroy manager when done
    }

    private void SpawnSingle(Vector3 position, int amount)
    {
        GameObject go = new GameObject("FloatingText");
        go.transform.position = position;
        
        var tmp = go.AddComponent<TMPro.TextMeshPro>();
        tmp.text = $"+{amount}";
        tmp.color = new Color(1f, 0.84f, 0f); // Gold color
        tmp.fontSize = 1.5f;
        tmp.alignment = TMPro.TextAlignmentOptions.Center;
        tmp.fontStyle = TMPro.FontStyles.Bold;
        tmp.sortingOrder = 32767;
        
        tmp.outlineWidth = 0.2f;
        tmp.outlineColor = new Color(0, 0, 0, 0.8f);

        var animator = go.AddComponent<FloatingTextAnimator>();
        animator.StartCoroutine(animator.AnimateRoutine(tmp));
    }

    private IEnumerator AnimateRoutine(TMPro.TextMeshPro tmp)
    {
        float duration = 1.5f;
        float elapsed = 0f;
        
        Vector3 startPos = transform.position;
        // Random trajectory for each coin
        float randomX = Random.Range(-1.5f, 1.5f);
        float randomY = Random.Range(1.5f, 2.5f);
        Vector3 endPos = startPos + new Vector3(randomX, randomY, 0f);
        
        // Control points for Bezier curve (arc)
        Vector3 controlPoint = startPos + new Vector3(randomX * 0.5f, randomY + 1f, 0f);
        
        Color startColor = tmp.color;
        Color outlineStartColor = tmp.outlineColor;
        
        Vector3 startScale = Vector3.zero;
        Vector3 midScale = Vector3.one * 1.3f;
        Vector3 endScale = Vector3.one;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            
            // Position: Quadratic Bezier curve for a nice arc
            float easeT = 1f - Mathf.Pow(1f - t, 3f); // Ease out cubic for movement
            Vector3 m1 = Vector3.Lerp(startPos, controlPoint, easeT);
            Vector3 m2 = Vector3.Lerp(controlPoint, endPos, easeT);
            transform.position = Vector3.Lerp(m1, m2, easeT);
            
            // Scale: Bouncy pop in
            if (t < 0.2f)
            {
                float scaleT = t / 0.2f;
                float c1 = 1.70158f;
                float c3 = c1 + 1f;
                float scaleEase = 1f + c3 * Mathf.Pow(scaleT - 1f, 3f) + c1 * Mathf.Pow(scaleT - 1f, 2f);
                transform.localScale = Vector3.LerpUnclamped(startScale, midScale, scaleEase);
            }
            else if (t < 0.4f)
            {
                float scaleT = (t - 0.2f) / 0.2f;
                float scaleEase = scaleT < 0.5f ? 2f * scaleT * scaleT : 1f - Mathf.Pow(-2f * scaleT + 2f, 2f) / 2f;
                transform.localScale = Vector3.Lerp(midScale, endScale, scaleEase);
            }
            else
            {
                transform.localScale = endScale;
            }
            
            // Fade out: Start fading after 50% of duration
            if (t > 0.5f)
            {
                float fadeT = (t - 0.5f) / 0.5f;
                float fadeEase = fadeT * fadeT; // Ease in quad
                
                Color newColor = startColor;
                newColor.a = Mathf.Lerp(1f, 0f, fadeEase);
                tmp.color = newColor;
                
                Color newOutlineColor = outlineStartColor;
                newOutlineColor.a = Mathf.Lerp(outlineStartColor.a, 0f, fadeEase);
                tmp.outlineColor = newOutlineColor;
            }
            
            yield return null;
        }
        
        Destroy(gameObject);
    }
}
