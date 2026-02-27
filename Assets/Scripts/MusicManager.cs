using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(AudioSource))]
public class MusicManager : MonoBehaviour
{
    [Header("Playlist")]
    [Tooltip("Tracks that will be played one after another.")]
    public List<AudioClip> tracks = new List<AudioClip>();
    public bool playOnStart = true;
    public bool shuffle = false;
    public bool loopPlaylist = true;
    [Tooltip("When shuffle is enabled, avoids picking the same track twice in a row.")]
    public bool avoidImmediateRepeat = true;

    [Header("Audio")]
    [Range(0f, 1f)] public float volume = 0.8f;
    [Range(0f, 10f)] public float fadeDuration = 1.25f;
    public bool useUnscaledTime = false;

    [Header("Lifetime")]
    public bool persistBetweenScenes = true;
    public bool enforceSingleInstance = true;

    public static MusicManager Instance { get; private set; }

    private AudioSource sourceA;
    private AudioSource sourceB;
    private AudioSource activeSource;
    private AudioSource standbySource;

    private int currentTrackIndex = -1;
    private bool isPausedByUser;
    private bool autoTransitionQueued;
    private Coroutine transitionRoutine;

    void Awake()
    {
        if (enforceSingleInstance)
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        if (persistBetweenScenes)
            DontDestroyOnLoad(gameObject);

        SetupSources();
    }

    void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    void Start()
    {
        if (playOnStart)
            Play();
    }

    void Update()
    {
        if (isPausedByUser || tracks == null || tracks.Count == 0 || activeSource == null || activeSource.clip == null)
            return;

        if (transitionRoutine == null && activeSource.isPlaying && activeSource.clip.length > 0.05f)
        {
            float trigger = Mathf.Max(0f, activeSource.clip.length - fadeDuration);
            if (activeSource.time >= trigger)
            {
                autoTransitionQueued = true;
                PlayNextTrack();
            }
        }

        if (transitionRoutine == null && !activeSource.isPlaying && !autoTransitionQueued)
            PlayNextTrack();
    }

    public void Play()
    {
        if (tracks == null || tracks.Count == 0)
            return;

        if (currentTrackIndex < 0)
        {
            int startIndex = shuffle ? GetRandomIndex(-1) : 0;
            PlayTrack(startIndex);
            return;
        }

        isPausedByUser = false;
        activeSource.UnPause();
    }

    public void Pause()
    {
        isPausedByUser = true;
        if (activeSource != null)
            activeSource.Pause();
        if (standbySource != null)
            standbySource.Pause();
    }

    public void Resume()
    {
        isPausedByUser = false;
        if (activeSource != null)
            activeSource.UnPause();
        if (standbySource != null && standbySource.clip != null)
            standbySource.UnPause();
    }

    public void StopMusic()
    {
        isPausedByUser = false;
        autoTransitionQueued = false;
        currentTrackIndex = -1;

        if (transitionRoutine != null)
        {
            StopCoroutine(transitionRoutine);
            transitionRoutine = null;
        }

        if (sourceA != null)
        {
            sourceA.Stop();
            sourceA.clip = null;
        }
        if (sourceB != null)
        {
            sourceB.Stop();
            sourceB.clip = null;
        }
    }

    public void NextTrack()
    {
        PlayNextTrack();
    }

    public void PreviousTrack()
    {
        if (tracks == null || tracks.Count == 0)
            return;

        int targetIndex;
        if (shuffle)
            targetIndex = GetRandomIndex(currentTrackIndex);
        else
            targetIndex = currentTrackIndex <= 0 ? (loopPlaylist ? tracks.Count - 1 : 0) : currentTrackIndex - 1;

        PlayTrack(targetIndex);
    }

    public void SetVolume(float newVolume)
    {
        volume = Mathf.Clamp01(newVolume);
        if (activeSource != null)
            activeSource.volume = volume;
        if (standbySource != null && transitionRoutine == null)
            standbySource.volume = 0f;
    }

    public void PlayTrack(int index)
    {
        if (tracks == null || tracks.Count == 0 || index < 0 || index >= tracks.Count)
            return;

        if (transitionRoutine != null)
            StopCoroutine(transitionRoutine);

        transitionRoutine = StartCoroutine(TransitionToTrack(index));
    }

    private void PlayNextTrack()
    {
        if (tracks == null || tracks.Count == 0)
            return;

        int nextIndex = GetNextTrackIndex();
        if (nextIndex < 0)
        {
            StopMusic();
            return;
        }

        PlayTrack(nextIndex);
    }

    private int GetNextTrackIndex()
    {
        if (tracks == null || tracks.Count == 0)
            return -1;

        if (currentTrackIndex < 0)
            return shuffle ? GetRandomIndex(-1) : 0;

        if (shuffle)
            return GetRandomIndex(currentTrackIndex);

        int next = currentTrackIndex + 1;
        if (next >= tracks.Count)
            return loopPlaylist ? 0 : -1;

        return next;
    }

    private int GetRandomIndex(int excludedIndex)
    {
        if (tracks.Count == 1)
            return 0;

        int index = Random.Range(0, tracks.Count);
        if (!avoidImmediateRepeat)
            return index;

        int guard = 0;
        while (index == excludedIndex && guard < 10)
        {
            index = Random.Range(0, tracks.Count);
            guard++;
        }

        return index;
    }

    private IEnumerator TransitionToTrack(int newIndex)
    {
        autoTransitionQueued = false;
        isPausedByUser = false;

        AudioClip nextClip = tracks[newIndex];
        if (nextClip == null)
        {
            transitionRoutine = null;
            yield break;
        }

        standbySource.clip = nextClip;
        standbySource.volume = 0f;
        standbySource.Play();

        if (activeSource.clip == null || !activeSource.isPlaying || fadeDuration <= 0f)
        {
            activeSource.Stop();
            activeSource.clip = null;
            standbySource.volume = volume;

            SwapSources();
            currentTrackIndex = newIndex;
            transitionRoutine = null;
            yield break;
        }

        float t = 0f;
        float startVolume = activeSource.volume;

        while (t < fadeDuration)
        {
            t += GetDeltaTime();
            float k = Mathf.Clamp01(t / fadeDuration);

            activeSource.volume = Mathf.Lerp(startVolume, 0f, k);
            standbySource.volume = Mathf.Lerp(0f, volume, k);
            yield return null;
        }

        activeSource.Stop();
        activeSource.clip = null;
        activeSource.volume = 0f;
        standbySource.volume = volume;

        SwapSources();
        currentTrackIndex = newIndex;
        transitionRoutine = null;
    }

    private void SwapSources()
    {
        AudioSource temp = activeSource;
        activeSource = standbySource;
        standbySource = temp;
    }

    private float GetDeltaTime()
    {
        return useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
    }

    // --- Music Ducking (for event videos) ---
    private Coroutine duckRoutine;
    private float duckTargetVolume;
    private bool isDucked = false;

    /// <summary>
    /// Gradually lowers music volume to a fraction of the current volume.
    /// Call this when an event video starts.
    /// </summary>
    public void DuckMusic(float targetFraction = 0.1f, float duration = 1.0f)
    {
        if (isDucked) return;
        isDucked = true;
        duckTargetVolume = volume * targetFraction;
        if (duckRoutine != null) StopCoroutine(duckRoutine);
        duckRoutine = StartCoroutine(FadeToVolume(duckTargetVolume, duration));
    }

    /// <summary>
    /// Gradually restores music volume back to the original level.
    /// Call this when the event is dismissed.
    /// </summary>
    public void RestoreMusic(float duration = 1.0f)
    {
        if (!isDucked) return;
        isDucked = false;
        if (duckRoutine != null) StopCoroutine(duckRoutine);
        duckRoutine = StartCoroutine(FadeToVolume(volume, duration));
    }

    private IEnumerator FadeToVolume(float target, float duration)
    {
        float startVol = activeSource != null ? activeSource.volume : volume;
        float t = 0f;
        while (t < duration)
        {
            t += GetDeltaTime();
            float k = Mathf.Clamp01(t / duration);
            float newVol = Mathf.Lerp(startVol, target, k);
            if (activeSource != null) activeSource.volume = newVol;
            yield return null;
        }
        if (activeSource != null) activeSource.volume = target;
        duckRoutine = null;
    }

    private void SetupSources()
    {
        AudioSource[] allSources = GetComponents<AudioSource>();
        sourceA = allSources.Length > 0 ? allSources[0] : gameObject.AddComponent<AudioSource>();
        sourceB = allSources.Length > 1 ? allSources[1] : gameObject.AddComponent<AudioSource>();

        ConfigureSource(sourceA);
        ConfigureSource(sourceB);

        sourceB.outputAudioMixerGroup = sourceA.outputAudioMixerGroup;

        sourceA.volume = volume;
        sourceB.volume = 0f;
        activeSource = sourceA;
        standbySource = sourceB;
    }

    private void ConfigureSource(AudioSource source)
    {
        source.playOnAwake = false;
        source.loop = false;
        source.spatialBlend = 0f;
        source.volume = volume;
    }
}
