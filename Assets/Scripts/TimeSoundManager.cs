using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

[System.Serializable]
public enum DayTimePeriod
{
    Morning,
    Day,
    Evening,
    Night
}

[System.Serializable]
public class TimeSoundSlot
{
    [Tooltip("Fixed period type.")]
    public DayTimePeriod period = DayTimePeriod.Day;

    [Tooltip("Random clip from this list is played when this slot becomes active.")]
    public AudioClip[] clips;

    [Range(0f, 1f)]
    public float volume = 1f;
}

[RequireComponent(typeof(AudioSource))]
public class TimeSoundManager : MonoBehaviour
{
    [Header("References")]
    public TimeManager timeManager;
    public AudioSource audioSource;
    public Camera mapCamera;

    [Header("Time Sound Slots")]
    public List<TimeSoundSlot> soundSlots = new List<TimeSoundSlot>();

    [Header("Zoom Audio Rules")]
    [Tooltip("Time sounds are audible only at or below this orthographic size (close zoom).")]
    public float closeZoomThreshold = 6f;
    [Tooltip("If true, close/ground detection uses MapCameraMovement min/max zoom range.")]
    public bool useMapCameraZoomRange = true;
    [Tooltip("0 = min zoom (closest), 1 = max zoom (furthest). Ground audio is allowed at or below this normalized zoom.")]
    [Range(0f, 1f)] public float groundZoomCutoff01 = 0.35f;
    [Tooltip("Small buffer to avoid fast toggling around threshold.")]
    public float zoomSwitchHysteresis = 0.2f;
    [Range(0.01f, 8f)] public float zoomBlendSpeed = 3f;

    [Header("High Altitude Wind")]
    [Tooltip("Played continuously and faded in when zooming toward top zoom-out.")]
    public AudioSource highAltitudeWindAudioSource;
    [Range(0f, 1f)] public float highAltitudeWindVolume = 0.35f;
    [Tooltip("Zoom where wind begins to fade in. If <= 0, it is auto-derived from close zoom threshold.")]
    public float windStartsAtZoom = 0f;
    [Tooltip("Zoom where wind reaches full configured volume. If <= 0, uses MapCameraMovement.maxZoom when available.")]
    public float windFullAtZoom = 0f;
    [Range(0.01f, 8f)] public float windFadeSpeed = 4f;

    [Header("Playback")]
    public bool playOnStart = true;
    public bool loopClips = true;
    [Range(0.01f, 8f)] public float timeCrossfadeSpeed = 4f;

    private int activeSlotIndex = -1;
    private SimulationHandler simulationHandler;
    private FieldInfo simulationHandlerField;
    private bool isInCloseZoom;
    private bool isSoundStopped;
    private float closeBlend01;
    private readonly Dictionary<DayTimePeriod, AudioSource> periodSources = new Dictionary<DayTimePeriod, AudioSource>();
    private readonly Dictionary<DayTimePeriod, int> lastClipByPeriod = new Dictionary<DayTimePeriod, int>();
    private float resolvedWindStartZoom;
    private float resolvedWindFullZoom;
    private MapCameraMovement mapCameraMovement;

    void Reset()
    {
        audioSource = GetComponent<AudioSource>();
        SetDefaultTimeSlots();
    }

    void OnValidate()
    {
        EnsureUniquePeriods();
    }

    void Awake()
    {
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
        BuildPeriodSources();

        if (mapCamera == null)
            mapCamera = Camera.main;
        CacheCameraMovement();

        if (timeManager == null)
            timeManager = FindFirstObjectByType<TimeManager>();

        simulationHandlerField = typeof(TimeManager).GetField("simulationHandler", BindingFlags.Instance | BindingFlags.NonPublic);

        if (soundSlots == null || soundSlots.Count == 0)
            SetDefaultTimeSlots();

        EnsureUniquePeriods();
        ResolveWindZoomLimits();
    }

    void Start()
    {
        TryResolveSimulationHandlerFromTimeManager();
        bool closeNow = EvaluateZoomState();
        isInCloseZoom = closeNow;
        closeBlend01 = closeNow ? 1f : 0f;

        if (playOnStart)
            ForceRefreshSound();

        if (highAltitudeWindAudioSource != null && highAltitudeWindAudioSource.clip != null && !highAltitudeWindAudioSource.isPlaying)
            highAltitudeWindAudioSource.Play();
    }

    void Update()
    {
        if (isSoundStopped)
            return;

        if (mapCamera == null)
            mapCamera = Camera.main;
        CacheCameraMovement();

        bool closeTarget = EvaluateZoomState();
        isInCloseZoom = closeTarget;
        closeBlend01 = Mathf.MoveTowards(closeBlend01, closeTarget ? 1f : 0f, zoomBlendSpeed * Time.deltaTime);

        float windBlend = EvaluateWindBlend();
        FadeAudio(highAltitudeWindAudioSource, highAltitudeWindVolume * windBlend, windFadeSpeed);

        // Hard rule: when camera is not in close/ground zoom, all time sounds must stay silent.
        if (!isInCloseZoom)
        {
            FadeAllPeriodSources(0f, force: true);
            return;
        }

        if (soundSlots.Count == 0)
        {
            FadeAllPeriodSources(0f);
            return;
        }

        if (simulationHandler == null && !TryResolveSimulationHandlerFromTimeManager())
        {
            FadeAllPeriodSources(0f);
            return;
        }

        int minuteInCycle = (int)(simulationHandler.simulationTime % 60);
        int slotIndex = GetSlotIndexForMinute(minuteInCycle);
        if (slotIndex < 0)
        {
            activeSlotIndex = -1;
            FadeAllPeriodSources(0f);
            return;
        }

        if (slotIndex != activeSlotIndex)
            ActivateSlot(slotIndex);

        ApplySlotVolumes(slotIndex, closeBlend01);
    }

    [ContextMenu("Stop All Sound")]
    public void StopAllSound()
    {
        isSoundStopped = true;
        FadeAllPeriodSources(0f, force: true);

        if (highAltitudeWindAudioSource != null)
        {
            highAltitudeWindAudioSource.volume = 0f;
            highAltitudeWindAudioSource.Pause();
        }

        activeSlotIndex = -1;
    }

    [ContextMenu("Resume Sound")]
    public void ResumeSound()
    {
        isSoundStopped = false;
        ForceRefreshSound();

        if (highAltitudeWindAudioSource != null && highAltitudeWindAudioSource.clip != null && !highAltitudeWindAudioSource.isPlaying)
            highAltitudeWindAudioSource.Play();
    }

    [ContextMenu("Force Refresh Time Sound")]
    public void ForceRefreshSound()
    {
        bool closeNow = EvaluateZoomState();
        isInCloseZoom = closeNow;
        closeBlend01 = closeNow ? 1f : 0f;
        ResolveWindZoomLimits();
        FadeAudio(highAltitudeWindAudioSource, highAltitudeWindVolume * EvaluateWindBlend(), windFadeSpeed, force: true);

        if (!isInCloseZoom)
        {
            FadeAllPeriodSources(0f, force: true);
            return;
        }

        if (simulationHandler == null && !TryResolveSimulationHandlerFromTimeManager())
            return;

        int minuteInCycle = (int)(simulationHandler.simulationTime % 60);
        int slotIndex = GetSlotIndexForMinute(minuteInCycle);
        if (slotIndex < 0)
        {
            activeSlotIndex = -1;
            FadeAllPeriodSources(0f, force: true);
            return;
        }

        ActivateSlot(slotIndex);
        ApplySlotVolumes(slotIndex, closeBlend01, force: true);
    }

    private bool TryResolveSimulationHandlerFromTimeManager()
    {
        if (timeManager == null)
            timeManager = FindFirstObjectByType<TimeManager>();

        if (timeManager == null)
            return false;

        if (simulationHandlerField != null)
            simulationHandler = simulationHandlerField.GetValue(timeManager) as SimulationHandler;

        if (simulationHandler == null)
            simulationHandler = FindFirstObjectByType<SimulationHandler>();

        return simulationHandler != null;
    }

    private int GetSlotIndexForMinute(int minute)
    {
        for (int i = 0; i < soundSlots.Count; i++)
        {
            TimeSoundSlot slot = soundSlots[i];
            if (slot == null || slot.clips == null || slot.clips.Length == 0)
                continue;

            if (IsMinuteInPeriod(minute, slot.period))
                return i;
        }
        return -1;
    }

    private bool IsMinuteInPeriod(int minute, DayTimePeriod period)
    {
        switch (period)
        {
            case DayTimePeriod.Morning:
                return minute >= 0 && minute < 15;
            case DayTimePeriod.Day:
                return minute >= 15 && minute < 30;
            case DayTimePeriod.Evening:
                return minute >= 30 && minute < 45;
            case DayTimePeriod.Night:
                return minute >= 45 && minute < 60;
            default:
                return false;
        }
    }

    private int PickClipIndex(int length, int avoidIndex)
    {
        if (length <= 1)
            return 0;

        int index = Random.Range(0, length);
        if (index == avoidIndex)
            index = (index + 1) % length;

        return index;
    }

    private bool EvaluateZoomState()
    {
        if (mapCamera == null || !mapCamera.orthographic)
            return false;

        float closeThreshold = closeZoomThreshold;
        if (useMapCameraZoomRange && mapCameraMovement != null)
        {
            float minZoom = mapCameraMovement.minZoom;
            float maxZoom = mapCameraMovement.maxZoom;
            closeThreshold = Mathf.Lerp(minZoom, maxZoom, Mathf.Clamp01(groundZoomCutoff01));
        }

        if (isInCloseZoom)
            return mapCamera.orthographicSize <= closeThreshold + zoomSwitchHysteresis;

        return mapCamera.orthographicSize <= closeThreshold - zoomSwitchHysteresis;
    }

    private void FadeAudio(AudioSource source, float targetVolume, float speed, bool force = false)
    {
        if (source == null)
            return;

        if (!source.isPlaying && source.clip != null && targetVolume > 0f)
            source.Play();

        if (force)
        {
            source.volume = targetVolume;
            return;
        }

        source.volume = Mathf.MoveTowards(source.volume, targetVolume, speed * Time.deltaTime);

        if (targetVolume <= 0f && source.volume <= 0.001f && source.isPlaying)
            source.Pause();
    }

    private void BuildPeriodSources()
    {
        if (audioSource == null)
            return;

        periodSources.Clear();
        lastClipByPeriod.Clear();
        AudioSource[] allSources = GetComponents<AudioSource>();
        for (int i = 0; i < allSources.Length; i++)
        {
            if (allSources[i] == null || allSources[i] == highAltitudeWindAudioSource)
                continue;

            allSources[i].volume = 0f;
            allSources[i].Stop();
        }

        int sourceCursor = 0;
        for (int i = 0; i < soundSlots.Count; i++)
        {
            TimeSoundSlot slot = soundSlots[i];
            if (slot == null)
                continue;

            if (periodSources.ContainsKey(slot.period))
                continue;

            AudioSource source = null;
            while (sourceCursor < allSources.Length && source == null)
            {
                if (allSources[sourceCursor] != highAltitudeWindAudioSource)
                    source = allSources[sourceCursor];

                sourceCursor++;
            }

            if (source == null)
                source = gameObject.AddComponent<AudioSource>();

            CopyAudioSettings(audioSource, source);
            source.playOnAwake = false;
            source.loop = loopClips;
            source.volume = 0f;
            source.clip = null;
            source.Stop();

            periodSources[slot.period] = source;
            lastClipByPeriod[slot.period] = -1;
        }
    }

    private void ResolveWindZoomLimits()
    {
        float closeThreshold = closeZoomThreshold;
        if (useMapCameraZoomRange && mapCameraMovement != null)
        {
            float minZoom = mapCameraMovement.minZoom;
            float maxZoom = mapCameraMovement.maxZoom;
            closeThreshold = Mathf.Lerp(minZoom, maxZoom, Mathf.Clamp01(groundZoomCutoff01));
        }

        float autoStart = closeThreshold + zoomSwitchHysteresis;
        float autoFull = autoStart + 3f;

        if (mapCameraMovement != null)
            autoFull = Mathf.Max(autoStart + 0.01f, mapCameraMovement.maxZoom);

        resolvedWindStartZoom = windStartsAtZoom > 0f ? windStartsAtZoom : autoStart;
        resolvedWindFullZoom = windFullAtZoom > 0f ? windFullAtZoom : autoFull;

        if (resolvedWindFullZoom <= resolvedWindStartZoom)
            resolvedWindFullZoom = resolvedWindStartZoom + 0.01f;
    }

    private void CacheCameraMovement()
    {
        if (mapCamera == null)
        {
            mapCameraMovement = null;
            return;
        }

        if (mapCameraMovement != null && mapCameraMovement.gameObject == mapCamera.gameObject)
            return;

        mapCameraMovement = mapCamera.GetComponent<MapCameraMovement>();
    }

    private float EvaluateWindBlend()
    {
        if (mapCamera == null || !mapCamera.orthographic)
            return 0f;

        if (resolvedWindFullZoom <= resolvedWindStartZoom)
            ResolveWindZoomLimits();

        float zoom = mapCamera.orthographicSize;
        return Mathf.InverseLerp(resolvedWindStartZoom, resolvedWindFullZoom, zoom);
    }

    private void FadeAllPeriodSources(float targetVolume, bool force = false)
    {
        foreach (var kv in periodSources)
            FadeAudio(kv.Value, targetVolume, timeCrossfadeSpeed, force);
    }

    private void ApplySlotVolumes(int activeIndex, float groundBlend, bool force = false)
    {
        if (activeIndex < 0 || activeIndex >= soundSlots.Count)
        {
            FadeAllPeriodSources(0f, force);
            return;
        }

        TimeSoundSlot activeSlot = soundSlots[activeIndex];
        DayTimePeriod activePeriod = activeSlot.period;

        foreach (var kv in periodSources)
        {
            float target = 0f;
            if (kv.Key == activePeriod)
            {
                target = activeSlot.volume * groundBlend;
            }

            FadeAudio(kv.Value, target, timeCrossfadeSpeed, force);
        }
    }

    private void CopyAudioSettings(AudioSource from, AudioSource to)
    {
        if (from == null || to == null)
            return;

        to.outputAudioMixerGroup = from.outputAudioMixerGroup;
        to.mute = from.mute;
        to.bypassEffects = from.bypassEffects;
        to.bypassListenerEffects = from.bypassListenerEffects;
        to.bypassReverbZones = from.bypassReverbZones;
        to.priority = from.priority;
        to.panStereo = from.panStereo;
        to.spatialBlend = from.spatialBlend;
        to.reverbZoneMix = from.reverbZoneMix;
        to.dopplerLevel = from.dopplerLevel;
        to.spread = from.spread;
        to.rolloffMode = from.rolloffMode;
        to.minDistance = from.minDistance;
        to.maxDistance = from.maxDistance;
    }

    private void ActivateSlot(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= soundSlots.Count)
            return;

        TimeSoundSlot slot = soundSlots[slotIndex];
        if (slot == null || slot.clips == null || slot.clips.Length == 0)
            return;

        if (!periodSources.TryGetValue(slot.period, out AudioSource source) || source == null)
            return;

        int previousClipIndex = -1;
        if (lastClipByPeriod.TryGetValue(slot.period, out int lastIndex))
            previousClipIndex = lastIndex;

        int clipIndex = PickClipIndex(slot.clips.Length, previousClipIndex);
        lastClipByPeriod[slot.period] = clipIndex;
        AudioClip nextClip = slot.clips[clipIndex];
        if (nextClip == null)
            return;

        source.loop = loopClips;
        source.volume = Mathf.Min(source.volume, slot.volume);
        if (source.clip != nextClip)
            source.clip = nextClip;

        if (!source.isPlaying)
            source.Play();

        activeSlotIndex = slotIndex;
    }

    private void SetDefaultTimeSlots()
    {
        soundSlots = new List<TimeSoundSlot>
        {
            new TimeSoundSlot { period = DayTimePeriod.Morning, volume = 1f },
            new TimeSoundSlot { period = DayTimePeriod.Day, volume = 1f },
            new TimeSoundSlot { period = DayTimePeriod.Evening, volume = 1f },
            new TimeSoundSlot { period = DayTimePeriod.Night, volume = 1f }
        };
    }

    private void EnsureUniquePeriods()
    {
        if (soundSlots == null)
        {
            soundSlots = new List<TimeSoundSlot>();
            return;
        }

        HashSet<DayTimePeriod> seen = new HashSet<DayTimePeriod>();
        for (int i = soundSlots.Count - 1; i >= 0; i--)
        {
            TimeSoundSlot slot = soundSlots[i];
            if (slot == null)
            {
                soundSlots.RemoveAt(i);
                continue;
            }

            if (seen.Contains(slot.period))
            {
                soundSlots.RemoveAt(i);
                continue;
            }

            seen.Add(slot.period);
        }

        BuildPeriodSources();
        ResolveWindZoomLimits();
    }
}
