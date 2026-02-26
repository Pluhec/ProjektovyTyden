using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using UnityEngine.Networking;
using System.Collections;

public class MediaScript : MonoBehaviour 
{
    [Header("UI Settings")]
    public bool _isMediaTrue = false;
    public float openMediaY = 0f;
    public float closeMediaY = 300f;
    public float smoothTime = 0.1f;
    
    [Header("References")]
    public RectTransform mediaPanel;
    public RawImage displayImage;
    public VideoPlayer videoPlayer;

    private Vector2 currentVelocity;

    void Start()
    {
        if (mediaPanel == null) mediaPanel = GetComponent<RectTransform>();
        
        // Default state: Snap to closed position immediately
        Vector2 startPos = mediaPanel.anchoredPosition;
        startPos.y = closeMediaY;
        mediaPanel.anchoredPosition = startPos;
        _isMediaTrue = false;

        // Hide visuals until something is loaded
        ToggleVisuals(false);
    }

    void Update()
    {
        float targetY = _isMediaTrue ? openMediaY : closeMediaY;
        Vector2 targetPosition = new Vector2(mediaPanel.anchoredPosition.x, targetY);

        mediaPanel.anchoredPosition = Vector2.SmoothDamp(
            mediaPanel.anchoredPosition, 
            targetPosition, 
            ref currentVelocity, 
            smoothTime
        );
    }

    public void SetMediaByPath(string fullPath)
    {
        // Reset state
        _isMediaTrue = false; 
        videoPlayer.Stop();
        ToggleVisuals(false);

        if (string.IsNullOrEmpty(fullPath)) return;

        if (fullPath.EndsWith(".mp4", System.StringComparison.OrdinalIgnoreCase))
        {
            PrepareVideo(fullPath);
        }
        else if (fullPath.EndsWith(".png", System.StringComparison.OrdinalIgnoreCase) || 
                 fullPath.EndsWith(".jpg", System.StringComparison.OrdinalIgnoreCase))
        {
            StartCoroutine(LoadImage(fullPath));
        }
    }

    private void PrepareVideo(string path)
    {
        videoPlayer.url = path;
        videoPlayer.isLooping = true;
        
        // Listen for when the video is ready so we don't show a black screen
        videoPlayer.prepareCompleted += (vp) => {
            displayImage.texture = vp.texture;
            vp.Play();
            ToggleVisuals(true);
            _isMediaTrue = true; // Slide the panel up now that it's ready
        };
        videoPlayer.Prepare();
    }

    private IEnumerator LoadImage(string path)
    {
        // Use "file://" prefix for local paths
        string url = path.Contains("://") ? path : "file://" + path;

        using (UnityWebRequest uwr = UnityWebRequestTexture.GetTexture(url))
        {
            yield return uwr.SendWebRequest();

            if (uwr.result == UnityWebRequest.Result.Success)
            {
                Texture2D texture = DownloadHandlerTexture.GetContent(uwr);
                displayImage.texture = texture;
                
                ToggleVisuals(true);
                _isMediaTrue = true; // Slide the panel up
            }
        }
    }

    private void ToggleVisuals(bool show)
    {
        if (displayImage != null) displayImage.enabled = show;
        // We keep the videoPlayer component enabled, but only play when needed
    }
}