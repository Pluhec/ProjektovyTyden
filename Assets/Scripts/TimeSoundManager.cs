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

    [Tooltip("Inclusive start minute in the 0-59 cycle.")]
    [Range(0, 59)]
    public int startMinute = 0;

    [Tooltip("Exclusive end minute in the 0-59 cycle. Can wrap (e.g. 45 -> 10).")]
    [Range(0, 59)]
    public int endMinute = 15;

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
    [Tooltip("Small buffer to avoid fast toggling around threshold.")]
    public float zoomSwitchHysteresis = 0.2f;

    [Header("High Altitude Wind")]
    [Tooltip("Played continuously while zoomed out (above close threshold).")]
    public AudioSource highAltitudeWindAudioSource;
    [Range(0f, 1f)] public float highAltitudeWindVolume = 0.35f;
    [Range(0.01f, 2f)] public float audioFadeSpeed = 4f;

    [Header("Playback")]
    public bool playOnStart = true;
    public bool loopClips = true;

    private int activeSlotIndex = -1;
    private int lastClipIndex = -1;
    private SimulationHandler simulationHandler;
    private FieldInfo simulationHandlerField;
    private bool isInCloseZoom = true;
    private float currentTimeAudioTargetVolume;

    void Reset()
    {
        audioSource = GetComponent<AudioSource>();
        SetDefaultTimeSlots();
    }

    void Awake()
    {
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        if (mapCamera == null)
            mapCamera = Camera.main;

        if (timeManager == null)
            timeManager = FindFirstObjectByType<TimeManager>();

        simulationHandlerField = typeof(TimeManager).GetField("simulationHandler", BindingFlags.Instance | BindingFlags.NonPublic);

        if (soundSlots == null || soundSlots.Count == 0)
            SetDefaultTimeSlots();
    }

    void Start()
    {
        TryResolveSimulationHandlerFromTimeManager();
        EvaluateZoomState(forceImmediate: true);

        if (playOnStart)
            ForceRefreshSound();

        if (highAltitudeWindAudioSource != null && highAltitudeWindAudioSource.clip != null && !highAltitudeWindAudioSource.isPlaying)
            highAltitudeWindAudioSource.Play();
    }

    void Update()
    {
        if (mapCamera == null)
            mapCamera = Camera.main;

        EvaluateZoomState(forceImmediate: false);

        if (!isInCloseZoom)
        {
            FadeAudio(audioSource, 0f);
            FadeAudio(highAltitudeWindAudioSource, highAltitudeWindVolume);
            return;
        }

        FadeAudio(highAltitudeWindAudioSource, 0f);

        if (audioSource == null || soundSlots.Count == 0)
            return;

        if (simulationHandler == null && !TryResolveSimulationHandlerFromTimeManager())
            return;

        int minuteInCycle = (int)(simulationHandler.simulationTime % 60);
        int slotIndex = GetSlotIndexForMinute(minuteInCycle);
        if (slotIndex < 0)
        {
            FadeAudio(audioSource, 0f);
            return;
        }

        if (slotIndex != activeSlotIndex)
        {
            activeSlotIndex = slotIndex;
            PlayRandomClipForActiveSlot();
        }
        else if (!audioSource.isPlaying)
            PlayRandomClipForActiveSlot();

        FadeAudio(audioSource, currentTimeAudioTargetVolume);
    }

    [ContextMenu("Force Refresh Time Sound")]
    public void ForceRefreshSound()
    {
        EvaluateZoomState(forceImmediate: true);
        if (!isInCloseZoom)
        {
            FadeAudio(audioSource, 0f, force: true);
            FadeAudio(highAltitudeWindAudioSource, highAltitudeWindVolume, force: true);
            return;
        }

        if (simulationHandler == null && !TryResolveSimulationHandlerFromTimeManager())
            return;

        int minuteInCycle = (int)(simulationHandler.simulationTime % 60);
        activeSlotIndex = GetSlotIndexForMinute(minuteInCycle);
        PlayRandomClipForActiveSlot();
        FadeAudio(highAltitudeWindAudioSource, 0f, force: true);
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

            if (IsMinuteInSlot(minute, slot.startMinute, slot.endMinute))
                return i;
        }
        return -1;
    }

    private bool IsMinuteInSlot(int minute, int startMinute, int endMinute)
    {
        if (startMinute == endMinute)
            return true;

        if (startMinute < endMinute)
            return minute >= startMinute && minute < endMinute;

        return minute >= startMinute || minute < endMinute;
    }

    private void PlayRandomClipForActiveSlot()
    {
        if (activeSlotIndex < 0 || activeSlotIndex >= soundSlots.Count)
            return;

        TimeSoundSlot slot = soundSlots[activeSlotIndex];
        if (slot == null || slot.clips == null || slot.clips.Length == 0)
            return;

        int clipIndex = PickClipIndex(slot.clips.Length, lastClipIndex);
        lastClipIndex = clipIndex;

        AudioClip selectedClip = slot.clips[clipIndex];
        if (selectedClip == null)
            return;

        audioSource.clip = selectedClip;
        currentTimeAudioTargetVolume = slot.volume;
        audioSource.volume = 0f;
        audioSource.loop = loopClips;
        audioSource.Play();
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

    private void EvaluateZoomState(bool forceImmediate)
    {
        bool nextCloseState = isInCloseZoom;

        if (mapCamera == null || !mapCamera.orthographic)
        {
            nextCloseState = true;
        }
        else if (isInCloseZoom)
        {
            nextCloseState = mapCamera.orthographicSize <= closeZoomThreshold + zoomSwitchHysteresis;
        }
        else
        {
            nextCloseState = mapCamera.orthographicSize <= closeZoomThreshold - zoomSwitchHysteresis;
        }

        if (forceImmediate || nextCloseState != isInCloseZoom)
        {
            isInCloseZoom = nextCloseState;
            if (!isInCloseZoom && audioSource != null)
            {
                audioSource.Stop();
            }

            if (highAltitudeWindAudioSource != null && highAltitudeWindAudioSource.clip != null && !highAltitudeWindAudioSource.isPlaying)
            {
                highAltitudeWindAudioSource.Play();
            }
        }
    }

    private void FadeAudio(AudioSource source, float targetVolume, bool force = false)
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

        source.volume = Mathf.MoveTowards(source.volume, targetVolume, audioFadeSpeed * Time.deltaTime);
    }

    private void SetDefaultTimeSlots()
    {
        soundSlots = new List<TimeSoundSlot>
        {
            new TimeSoundSlot { period = DayTimePeriod.Morning, startMinute = 0, endMinute = 15, volume = 1f },
            new TimeSoundSlot { period = DayTimePeriod.Day, startMinute = 15, endMinute = 30, volume = 1f },
            new TimeSoundSlot { period = DayTimePeriod.Evening, startMinute = 30, endMinute = 45, volume = 1f },
            new TimeSoundSlot { period = DayTimePeriod.Night, startMinute = 45, endMinute = 0, volume = 1f }
        };
    }
}
