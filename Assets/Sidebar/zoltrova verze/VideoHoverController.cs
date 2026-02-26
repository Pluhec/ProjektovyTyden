using UnityEngine;
using UnityEngine.Video;
using UnityEngine.EventSystems;

// Attach this to the "video" child of a video post prefab.
// It plays the video when the cursor enters and pauses when it leaves.
public class VideoHoverController : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [HideInInspector] public VideoPlayer videoPlayer;

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (videoPlayer != null) videoPlayer.Play();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (videoPlayer != null) videoPlayer.Pause();
    }
}
