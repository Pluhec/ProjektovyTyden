using UnityEngine;

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
        if (_sidebarScript == null) return;

        _timer += Time.deltaTime;
        if (_timer >= _nextWaitTime)
        {
            _sidebarScript.SpawnPrefab();
            _timer = 0f;
            _nextWaitTime = Random.Range(MinPostTime, MaxPostTime);
        }
    }
}
