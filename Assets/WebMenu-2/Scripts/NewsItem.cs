using System;
using UnityEngine;

[Serializable]
public class NewsItem
{
    public string id;
    public string title;
    public string text;
    public string date;   // ISO "2026-02-26" nebo "2026-02-26T12:00:00Z"
    public string label;  // třeba "NOVINKA"
    public float minStance = 0f;  // Minimum totalitarian percentage required (0-100%). Democratic news = 0, Propaganda = 33, Totalitarian = 66
}

[Serializable]
public class NewsResponse
{
    public NewsItem[] items;
}