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

    [Header("Time Sound Slots")]
    public List<TimeSoundSlot> soundSlots = new List<TimeSoundSlot>();

    [Header("Playback")]
    public bool playOnStart = true;
    public bool loopClips = true;

    private int activeSlotIndex = -1;
    private int lastClipIndex = -1;
    private SimulationHandler simulationHandler;
    private FieldInfo simulationHandlerField;

    void Reset()
    {
        audioSource = GetComponent<AudioSource>();
        SetDefaultTimeSlots();
    }

    void Awake()
    {
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        if (timeManager == null)
            timeManager = FindFirstObjectByType<TimeManager>();

        simulationHandlerField = typeof(TimeManager).GetField("simulationHandler", BindingFlags.Instance | BindingFlags.NonPublic);

        if (soundSlots == null || soundSlots.Count == 0)
            SetDefaultTimeSlots();
    }

    void Start()
    {
        TryResolveSimulationHandlerFromTimeManager();

        if (playOnStart)
            ForceRefreshSound();
    }

    void Update()
    {
        if (audioSource == null || soundSlots.Count == 0)
            return;

        if (simulationHandler == null && !TryResolveSimulationHandlerFromTimeManager())
            return;

        int minuteInCycle = (int)(simulationHandler.simulationTime % 60);
        int slotIndex = GetSlotIndexForMinute(minuteInCycle);
        if (slotIndex < 0)
            return;

        if (slotIndex != activeSlotIndex)
        {
            activeSlotIndex = slotIndex;
            PlayRandomClipForActiveSlot();
            return;
        }

        if (!audioSource.isPlaying)
            PlayRandomClipForActiveSlot();
    }

    [ContextMenu("Force Refresh Time Sound")]
    public void ForceRefreshSound()
    {
        if (simulationHandler == null && !TryResolveSimulationHandlerFromTimeManager())
            return;

        int minuteInCycle = (int)(simulationHandler.simulationTime % 60);
        activeSlotIndex = GetSlotIndexForMinute(minuteInCycle);
        PlayRandomClipForActiveSlot();
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
        audioSource.volume = slot.volume;
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
