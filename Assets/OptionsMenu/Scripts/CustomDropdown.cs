using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;

public class ResolutionDropdown : MonoBehaviour
{
    [Header("UI Elements")]
    public TMP_Dropdown resolutionDropdown;  // Přetáhni tady svůj Dropdown
    private const string ResolutionWidthPrefKey = "ResolutionWidth";
    private const string ResolutionHeightPrefKey = "ResolutionHeight";

    private Resolution[] resolutions;

    void Start()
    {
        if (resolutionDropdown == null)
        {
            return;
        }

        BuildSupportedResolutions();
        PopulateDropdownOptions();

        // Připoj metodu pro změnu rozlišení
        resolutionDropdown.onValueChanged.AddListener(ChangeResolution);

        // Načti uložené rozlišení (default aktuální = index 0)
        int savedWidth = PlayerPrefs.GetInt(ResolutionWidthPrefKey, -1);
        int savedHeight = PlayerPrefs.GetInt(ResolutionHeightPrefKey, -1);
        int selectedIndex = GetIndexBySize(savedWidth, savedHeight);
        int clampedIndex = Mathf.Clamp(selectedIndex, 0, resolutions.Length - 1);

        // Nastav UI bez triggeru callbacku
        resolutionDropdown.SetValueWithoutNotify(clampedIndex);
        resolutionDropdown.RefreshShownValue();

        // Aplikuj uložené rozlišení při startu
        ApplyResolution(clampedIndex);
    }

    private void OnDestroy()
    {
        if (resolutionDropdown != null)
        {
            resolutionDropdown.onValueChanged.RemoveListener(ChangeResolution);
        }
    }

    // Funkce pro změnu rozlišení při výběru
    void ChangeResolution(int index)
    {
        ApplyResolution(index);
    }

    private void BuildSupportedResolutions()
    {
        var uniqueBySize = new Dictionary<string, Resolution>();

        foreach (Resolution resolution in Screen.resolutions)
        {
            string key = $"{resolution.width}x{resolution.height}";
            if (!uniqueBySize.ContainsKey(key))
            {
                uniqueBySize.Add(key, resolution);
            }
        }

        List<Resolution> ordered = uniqueBySize.Values
            .OrderByDescending(resolution => resolution.width)
            .ThenByDescending(resolution => resolution.height)
            .ToList();

        int currentWidth = Screen.width;
        int currentHeight = Screen.height;

        int currentIndex = ordered.FindIndex(resolution =>
            resolution.width == currentWidth && resolution.height == currentHeight);

        if (currentIndex >= 0)
        {
            Resolution currentResolution = ordered[currentIndex];
            ordered.RemoveAt(currentIndex);
            ordered.Insert(0, currentResolution);
        }
        else
        {
            ordered.Insert(0, new Resolution { width = currentWidth, height = currentHeight });
        }

        resolutions = ordered.ToArray();
    }

    private void PopulateDropdownOptions()
    {
        var options = new List<TMP_Dropdown.OptionData>();

        foreach (Resolution resolution in resolutions)
        {
            options.Add(new TMP_Dropdown.OptionData($"{resolution.width} x {resolution.height}"));
        }

        resolutionDropdown.ClearOptions();
        resolutionDropdown.AddOptions(options);
    }

    private int GetIndexBySize(int width, int height)
    {
        if (width <= 0 || height <= 0)
        {
            return 0;
        }

        int index = System.Array.FindIndex(resolutions, resolution =>
            resolution.width == width && resolution.height == height);

        return index >= 0 ? index : 0;
    }

    private void ApplyResolution(int index)
    {
        if (index < 0 || index >= resolutions.Length)
        {
            return;
        }

        Resolution selectedResolution = resolutions[index];
        Screen.SetResolution(selectedResolution.width, selectedResolution.height, Screen.fullScreen);

        PlayerPrefs.SetInt(ResolutionWidthPrefKey, selectedResolution.width);
        PlayerPrefs.SetInt(ResolutionHeightPrefKey, selectedResolution.height);
        PlayerPrefs.Save();
    }
}