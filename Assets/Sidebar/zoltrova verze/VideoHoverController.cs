using UnityEngine;
using UnityEngine.Video;
using UnityEngine.EventSystems;

// Attach this to the "video" child of a video post prefab.
// It plays the video when the cursor enters and pauses when it leaves.
public class VideoHoverController : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [HideInInspector] public VideoPlayer videoPlayer;
    
    [Tooltip("Délka fade in/out efektu pro hudbu při najetí myší.")]
    public float audioFadeDuration = 0.5f;

    private TimeSoundManager timeSoundManager;

    void Awake()
    {
        timeSoundManager = FindObjectOfType<TimeSoundManager>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (videoPlayer != null) videoPlayer.Play();
        if (timeSoundManager != null) timeSoundManager.StopAllSound();
        
        // Postupně ztlumí a pauzne hudbu z MusicManageru
        if (MusicManager.Instance != null) MusicManager.Instance.FadePause(audioFadeDuration);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (videoPlayer != null) videoPlayer.Pause();
        if (timeSoundManager != null) timeSoundManager.ResumeSound();

        // Odtlumí a plynule zesílí hudbu z MusicManageru
        if (MusicManager.Instance != null) MusicManager.Instance.FadeResume(audioFadeDuration);
    }
}