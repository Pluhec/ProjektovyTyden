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
}

[Serializable]
public class NewsResponse
{
    public NewsItem[] items;
}