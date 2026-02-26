using System.Collections.Generic;
using UnityEngine;
using PlayerChoice.DataSets;

public class EventsLoader : MonoBehaviour
{
    [SerializeField] private string EventsFilePath;
    public List<EventInfo> LoadedEvents = new List<EventInfo>();

    private void Awake()
    {
        LoadedEvents = DataFunctions.LoadEventsFromJSON(EventsFilePath);
    }
}