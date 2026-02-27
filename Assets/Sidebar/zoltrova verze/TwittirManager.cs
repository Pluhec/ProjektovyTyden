using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using TMPro;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using PlayerChoice.DataSets;
using Newtonsoft.Json;

public class TwittirManager : MonoBehaviour
{
    public GameObject prefab_messagePost;
    public GameObject prefab_imagePost;
    public GameObject prefab_videoPost;
    public Transform contentParent;
    public Transform loadingIndicator;
    public Transform loadingCircle;
    public ScrollRect scrollRect;

    // Expanded video viewer removed — feature disabled. Fields were here previously.

    [Tooltip("Seconds to wait at the bottom before loading more")]
    public float loadMoreDelay = 2f;

    // Images:  Assets/Resources/TwittirImages/dem/   (stance 1)
    //           Assets/Resources/TwittirImages/prop/  (stance 2)
    //           Assets/Resources/TwittirImages/tot/   (stance 3)
    // Videos:   Assets/Resources/TwittirVideos/
    private static readonly string VideosResourcePath = "TwittirVideos";

    [Tooltip("1 = dem, 2 = prop, 3 = tot (automatically updated from simulation stance)")]
    public int stance = 1;

    private static System.Random _random = new System.Random();
    private static HashSet<string> _postedMessages = new HashSet<string>();

    private bool _isLoadingMore = false;
    private SimulationHandler simulationHandler;

    void Start()
    {
        simulationHandler = FindObjectOfType<SimulationHandler>();
        UpdateStanceFromSimulation();

        Generate10();

        // Expanded video UI removed; feature disabled.
    }

    void Update()
    {
        // Keep the loading circle spinning
        if (loadingCircle != null)
            loadingCircle.Rotate(0f, 0f, -180f * Time.deltaTime);

        // If scrolled to the bottom and loading indicator is visible, load more
        if (!_isLoadingMore && scrollRect != null && loadingIndicator != null
            && loadingIndicator.gameObject.activeInHierarchy
            && scrollRect.verticalNormalizedPosition <= 0.01f)
        {
            StartCoroutine(LoadMoreAfterDelay());
        }
    }

    /// <summary>
    /// Updates the stance category based on the current simulation political stance.
    /// Uses percentage scale (0-100%) where higher = more totalitarian:
    /// - stance 1 (dem): 0-33% (democratic)
    /// - stance 2 (prop): 33-66% (moderate/propaganda)
    /// - stance 3 (tot): 66-100% (totalitarian)
    /// </summary>
    private void UpdateStanceFromSimulation()
    {
        if (simulationHandler == null)
        {
            Debug.LogWarning("TwittirManager: SimulationHandler not found! Using default stance (democratic).");
            stance = 1;
            return;
        }

        // Read the same value the progress bar shows (0-100)
        float stancePct = simulationHandler.GetStancePercentage();

        // Map to stance category (1/2/3) based on progress bar thresholds
        if (stancePct < 33f)
        {
            stance = 1; // Democratic (0-33%)
        }
        else if (stancePct < 66f)
        {
            stance = 2; // Propaganda (33-66%)
        }
        else
        {
            stance = 3; // Totalitarian (66-100%)
        }

        Debug.Log($"TwittirManager: Progress bar = {stancePct:F1}% → Category {stance} ({(stance == 1 ? "dem" : stance == 2 ? "prop" : "tot")})");
    }

    // Generate 10 new posts with a random mix of types
    void Generate10()
    {
        // Update stance from simulation before generating new posts
        UpdateStanceFromSimulation();

        for (int i = 0; i < 10; i++)
        {
            int roll = _random.Next(0, 8);
            switch (roll)
            {
                case 0: GenerateRandomMessagePost(); break;
                case 1: GenerateRandomMessagePost(); break;
                case 2: GenerateRandomMessagePost(); break;
                case 3: GenerateRandomMessagePost(); break;
                case 4: GenerateRandomMessagePost(); break;
                case 5: GenerateRandomImagePost();   break;
                case 6: GenerateRandomImagePost();   break;
                case 7: GenerateRandomVideoPost();   break;
            }
        }
    }

    void GenerateRandomMessagePost()
    {
        if (prefab_messagePost == null) { Debug.LogWarning("TwittirManager: prefab_messagePost is not assigned."); return; }

        SocialPost_JSON post_data = GetRandomPost();
        if (post_data == null) return;

        GameObject post = Instantiate(prefab_messagePost, contentParent);
        if (loadingIndicator != null) loadingIndicator.SetAsLastSibling();

        Transform usernameChild = post.transform.Find("username");
        Transform messageChild  = post.transform.Find("message");

        if (usernameChild != null) usernameChild.GetComponent<TMP_Text>().text = post_data.UserName;
        if (messageChild  != null) messageChild.GetComponent<TMP_Text>().text  = post_data.Content;
    }

    void GenerateRandomImagePost()
    {
        if (prefab_imagePost == null) { Debug.LogWarning("TwittirManager: prefab_imagePost is not assigned."); return; }

        SocialPost_JSON post_data = GetRandomPost();
        if (post_data == null) return;

        // Pick subfolder based on stance: 1=dem, 2=prop, 3=tot
        string subfolder = stance switch { 2 => "prop", 3 => "tot", _ => "dem" };
        string imagePath = $"TwittirImages/{subfolder}";

        Texture2D[] images = Resources.LoadAll<Texture2D>(imagePath);
        if (images == null || images.Length == 0)
        {
            Debug.LogWarning($"TwittirManager: No textures found in Resources/{imagePath}/");
            return;
        }

        Texture2D randomImage = images[_random.Next(0, images.Length)];

        GameObject post = Instantiate(prefab_imagePost, contentParent);
        if (loadingIndicator != null) loadingIndicator.SetAsLastSibling();

        Transform usernameChild = post.transform.Find("username");
        Transform imageChild    = post.transform.Find("image");

        if (usernameChild != null) usernameChild.GetComponent<TMP_Text>().text = post_data.UserName;
        if (imageChild    != null)
        {
            RawImage rawImage = imageChild.GetComponent<RawImage>();
            rawImage.texture = randomImage;

            // Resize width to match the image aspect ratio, keeping the current height
            RectTransform imageRect = imageChild.GetComponent<RectTransform>();
            imageRect.pivot         = new Vector2(0f, 0.5f); // anchor growth to the left
            float height      = imageRect.sizeDelta.y;
            float aspectRatio = (float)randomImage.width / randomImage.height;
            imageRect.sizeDelta = new Vector2(height * aspectRatio, height);
        }
    }

    void GenerateRandomVideoPost()
    {
        if (prefab_videoPost == null) { Debug.LogWarning("TwittirManager: prefab_videoPost is not assigned."); return; }

        SocialPost_JSON post_data = GetRandomPost();
        if (post_data == null) return;

        // Load all video clips from Resources/TwittirVideos/
        VideoClip[] videos = Resources.LoadAll<VideoClip>(VideosResourcePath);
        if (videos == null || videos.Length == 0)
        {
            Debug.LogWarning($"TwittirManager: No video clips found in Resources/{VideosResourcePath}/");
            return;
        }

        VideoClip randomVideo = videos[_random.Next(0, videos.Length)];

        GameObject post = Instantiate(prefab_videoPost, contentParent);
        if (loadingIndicator != null) loadingIndicator.SetAsLastSibling();

        Transform usernameChild = post.transform.Find("username");
        Transform videoChild    = post.transform.Find("video");

        if (usernameChild != null) usernameChild.GetComponent<TMP_Text>().text = post_data.UserName;
        if (videoChild    != null)
        {
            VideoPlayer player = videoChild.GetComponent<VideoPlayer>();

            // Create a RenderTexture sized to the clip and pipe it to a RawImage
            // so the video is actually visible inside the UI.
            RenderTexture rt = new RenderTexture((int)randomVideo.width, (int)randomVideo.height, 0);
            player.renderMode   = VideoRenderMode.RenderTexture;
            player.targetTexture = rt;

            RawImage display = videoChild.GetComponentInChildren<RawImage>();
            if (display != null) display.texture = rt;

            // Resize the VideoPlayer width to match the video's aspect ratio, keeping height
            RectTransform videoRect = videoChild.GetComponent<RectTransform>();
            videoRect.pivot         = new Vector2(0f, 0.5f); // anchor growth to the left
            float videoHeight      = videoRect.sizeDelta.y;
            float videoAspect      = (float)randomVideo.width / randomVideo.height;
            videoRect.sizeDelta    = new Vector2(videoHeight * videoAspect, videoHeight);

            player.clip        = randomVideo;
            player.isLooping   = true;
            player.playOnAwake = false; // hover controller handles play/pause

            // Add hover controller so video plays only when the cursor is over it
            VideoHoverController hover = videoChild.gameObject.AddComponent<VideoHoverController>();
            hover.videoPlayer = player;

            // Add click handler to expand video on click
            VideoPostClickHandler clickHandler = videoChild.gameObject.AddComponent<VideoPostClickHandler>();
            clickHandler.videoPlayer = player;
            clickHandler.username = post_data.UserName;

            // Prepare the video and show the first frame so it isn't transparent at rest
            player.prepareCompleted += _ => StartCoroutine(ShowFirstFrame(player));
            player.Prepare();
        }
    }

    // Plays the video for one frame to bake the first frame into the RenderTexture, then pauses.
    private IEnumerator ShowFirstFrame(VideoPlayer player)
    {
        player.Play();
        yield return null; // wait one engine frame for the texture to be written
        player.Pause();
        player.frame = 0;
    }

    private IEnumerator LoadMoreAfterDelay()
    {
        _isLoadingMore = true;
        yield return new WaitForSeconds(loadMoreDelay);
        // Only fire if still at the bottom (user didn't scroll away)
        if (scrollRect.verticalNormalizedPosition <= 0.01f)
            Generate10();
        _isLoadingMore = false;
    }

    // Reuses the same JSON loading logic as UserMessageScript
    private SocialPost_JSON GetRandomPost()
    {
        string socialBasePath = Path.Combine(Application.streamingAssetsPath, "SocialMessagesJSON");
        string independentFile = Path.Combine(socialBasePath, "IndependentMessages.json");

        if (!File.Exists(independentFile))
        {
            Debug.LogError($"TwittirManager: JSON not found at {independentFile}");
            return null;
        }

        try
        {
            string json = File.ReadAllText(independentFile);
            var data = JsonConvert.DeserializeObject<IndependentMessageList>(json);

            if (data?.IndependentZpravy != null && data.IndependentZpravy.Count > 0)
            {
                var available = data.IndependentZpravy
                    .Where(m => !_postedMessages.Contains(m.Content))
                    .ToList();

                // Reset deduplication pool if all messages have been used
                if (available.Count == 0)
                {
                    _postedMessages.Clear();
                    available = data.IndependentZpravy;
                }

                var selected = available[_random.Next(0, available.Count)];
                _postedMessages.Add(selected.Content);
                return selected;
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"TwittirManager: Failed to load posts — {ex.Message}");
        }

        return null;
    }

    /// <summary>
    /// Restore no-op ExpandVideo to satisfy callers; feature remains disabled.
    /// </summary>
    public void ExpandVideo(VideoPlayer sourceVideoPlayer, string username)
    {
        Debug.Log("TwittirManager: ExpandVideo called but expanded video feature is disabled.");
    }

    /// <summary>
    /// Restore no-op CloseExpandedVideo to satisfy callers; feature remains disabled.
    /// </summary>
    public void CloseExpandedVideo()
    {
        Debug.Log("TwittirManager: CloseExpandedVideo called but expanded video feature is disabled.");
    }

}
