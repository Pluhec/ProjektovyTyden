using System;
using UnityEngine;

public enum IconType
{
    Money,
    Power
}

[Serializable]
public class icon
{
    public Sprite sprite;
    public IconType iconType;
}

public class IconShower : MonoBehaviour
{
    public SpriteRenderer iconRenderer;
    public icon[] icons;

    public void Start()
    {
        if (iconRenderer == null)
        {
            iconRenderer = GetComponent<SpriteRenderer>();
        }
    }

    public bool SetIconType(IconType type)
    {
        if (iconRenderer == null)
        {
            iconRenderer = GetComponent<SpriteRenderer>();
        }

        if (icons == null || icons.Length == 0 || iconRenderer == null)
        {
            return false;
        }

        foreach (icon iconEntry in icons)
        {
            if (iconEntry.iconType == type && iconEntry.sprite != null)
            {
                iconRenderer.sprite = iconEntry.sprite;
                return true;
            }
        }

        return false;
    }

    public IconType GetCurrentIconType()
    {
        foreach (icon icon in icons)
        {
            if (icon.sprite == iconRenderer.sprite)
            {
                return icon.iconType;
            }
        }
        return IconType.Money; 
    }
}