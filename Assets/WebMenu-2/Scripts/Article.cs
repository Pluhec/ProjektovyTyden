using System;
using UnityEngine;

[Serializable]
public class Article
{
    public string title;
    [TextArea(2, 10)] public string excerpt;
    public DateTime date;
    public string category;
    public int views;

    public Article(string title, string excerpt, DateTime date, string category = "Aktualita", int views = 0)
    {
        this.title = title;
        this.excerpt = excerpt;
        this.date = date;
        this.category = category;
        this.views = views;
    }
}