using UnityEngine;
using System.Collections.Generic;
using PlayerChoice.Timing;
using PlayerChoice.DataSets;

public class PostTimer : MonoBehaviour
{
    public GameObject SideBar;
    public float MinPostTime = 5f;
    public float MaxPostTime = 10f;

    private float _timer;
    private float _eventTimer;
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
        DataFunctions.LoadEventsFromJSON(Application.dataPath + "/PleyerDecisions/Events/Events.json");
    }

    // Update is called once per frame
    void Update()
    {
        // only for testing -> _sidebarScript.SpawnPrefab();

        // calling the tweets
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

        // every 3 minutes 60% chance to spawn a event
        _eventTimer += Time.deltaTime;
        if (_eventTimer >= 180f)
        {
            if (Random.Range(0, 101) <= 60)
            {
                CallEvent();
            }
            _eventTimer = 0f;
        }

        // calling the mediascript now and then 

    }

    void CallEvent()
    {
        Debug.Log("Event called");
        
        if (DataFunctions.EventsList != null && DataFunctions.EventsList.Count > 0)
        {
            List<EventInfo> availableEvents = new List<EventInfo>(DataFunctions.EventsList);
            while (availableEvents.Count > 0)
            {
                int randomIndex = Random.Range(0, availableEvents.Count);
                EventInfo randomEvent = availableEvents[randomIndex];
                if (DataFunctions.CheckRequirements(randomEvent))
                {
                    DataFunctions.SendEvent(randomEvent);
                    return;
                }
                availableEvents.RemoveAt(randomIndex);
            }
        }
    }
}
