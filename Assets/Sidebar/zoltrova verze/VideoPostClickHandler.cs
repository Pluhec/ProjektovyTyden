using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Video;

/// <summary>
/// Handles click events on video posts to expand them to full view
/// </summary>
public class VideoPostClickHandler : MonoBehaviour, IPointerClickHandler
{
    public VideoPlayer videoPlayer;
    public string username;

    public void OnPointerClick(PointerEventData eventData)
    {
        // Find the TwittirManager and tell it to expand this video
        TwittirManager manager = FindObjectOfType<TwittirManager>();
        if (manager != null)
        {
            manager.ExpandVideo(videoPlayer, username);
        }
    }
}
