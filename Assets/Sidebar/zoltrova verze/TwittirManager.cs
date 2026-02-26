using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using TMPro;
using System.IO;
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

    // Put your images in Assets/Resources/TwittirImages/
    // Put your videos in Assets/Resources/TwittirVideos/
    private static readonly string ImagesResourcePath = "TwittirImages";
    private static readonly string VideosResourcePath = "TwittirVideos";

    private static System.Random _random = new System.Random();
    private static HashSet<string> _postedMessages = new HashSet<string>();

    void Start()
    {
        Generate10();
    }

    // Generate 10 new posts with a random mix of types
    void Generate10()
    {
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

        // Load all textures from Resources/TwittirImages/
        Texture2D[] images = Resources.LoadAll<Texture2D>(ImagesResourcePath);
        if (images == null || images.Length == 0)
        {
            Debug.LogWarning($"TwittirManager: No textures found in Resources/{ImagesResourcePath}/");
            return;
        }

        Texture2D randomImage = images[_random.Next(0, images.Length)];

        GameObject post = Instantiate(prefab_imagePost, contentParent);

        Transform usernameChild = post.transform.Find("username");
        Transform imageChild    = post.transform.Find("image");

        if (usernameChild != null) usernameChild.GetComponent<TMP_Text>().text = post_data.UserName;
        if (imageChild    != null) imageChild.GetComponent<RawImage>().texture  = randomImage;
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

        Transform usernameChild = post.transform.Find("username");
        Transform videoChild    = post.transform.Find("video");

        if (usernameChild != null) usernameChild.GetComponent<TMP_Text>().text = post_data.UserName;
        if (videoChild    != null)
        {
            VideoPlayer player = videoChild.GetComponent<VideoPlayer>();
            player.clip      = randomVideo;
            player.isLooping = true;
            player.Play();
        }
    }

    // Reuses the same JSON loading logic as UserMessageScript
    private SocialPost_JSON GetRandomPost()
    {
        string socialBasePath = Path.Combine(Application.dataPath, "PleyerDecisions/SocialMessagesJSON/");
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
}
