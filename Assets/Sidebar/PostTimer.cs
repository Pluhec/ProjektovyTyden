using UnityEngine;
using PlayerChoice.Timing;

public class PostTimer : MonoBehaviour
{
    public GameObject SideBar;
    public float MinPostTime = 5f;
    public float MaxPostTime = 10f;

    private float _timer;
    private float _nextWaitTime;
    private UserMessageScript _sidebarScript;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (SideBar != null)
        {
            _sidebarScript = SideBar.GetComponent<UserMessageScript>();
        }
        _nextWaitTime = Random.Range(MinPostTime, MaxPostTime);
    }

    // Update is called once per frame
    void Update()
    {
        // only for testing -> _sidebarScript.SpawnPrefab();

        if (_sidebarScript == null) return;

        _timer += Time.deltaTime;
        if (_timer >= _nextWaitTime)
        {
            // Use probabilities from TimerBase to determine if a post should spawn
            if (Random.Range(0, 101) <= TimerBase.V_SocialPostSpawnProb)
            {
                _sidebarScript.SpawnPrefab();
            }
            _timer = 0f;
            _nextWaitTime = Random.Range(MinPostTime, MaxPostTime);
        }
    }
}
