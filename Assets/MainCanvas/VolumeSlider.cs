using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

[RequireComponent(typeof(Slider))]
public class VolumeSlider : MonoBehaviour
{
    private enum AudioType { Music, SFX, Master }

    [SerializeField] private AudioType audioType;
    [Header("Mixer")]
    [Tooltip("Assign the project's AudioMixer used to control Music/SFX levels")]
    [SerializeField] private AudioMixer mainMixer;
    private Slider _slider;

    private void Start()
    {
        _slider = GetComponent<Slider>();

        // Ensure we have a mixer to control
        if (mainMixer == null)
        {
            Debug.LogWarning("VolumeSlider: AudioMixer 'mainMixer' není přiřazen v Inspectoru!");
        }

        // Načtení aktuální hodnoty (aby slider neodskočil na default při otevření menu)
        float currentVolume = 0.5f;
        string prefsKey;
        if (audioType == AudioType.Music) prefsKey = "MusicVolume";
        else if (audioType == AudioType.SFX) prefsKey = "SFXVolume";
        else prefsKey = "MasterVolume";
        
        if (PlayerPrefs.HasKey(prefsKey))
        {
            currentVolume = PlayerPrefs.GetFloat(prefsKey);
        }
        
        _slider.value = currentVolume;

        // Přidání Listeneru - co se stane, když pohnu sliderem
        _slider.onValueChanged.AddListener(OnSliderValueChanged);
    }

    private void OnSliderValueChanged(float value)
    {
        // Apply directly to mixer (no AudioManager dependency)
        if (audioType == AudioType.Music)
        {
            SetMusicVolumeLocal(value);
        }
        else if (audioType == AudioType.SFX)
        {
            SetSFXVolumeLocal(value);
        }
        else // Master: set both
        {
            SetMusicVolumeLocal(value);
            SetSFXVolumeLocal(value);
            PlayerPrefs.SetFloat("MasterVolume", value);
        }
    }

    private void SetMusicVolumeLocal(float value)
    {
        PlayerPrefs.SetFloat("MusicVolume", value);
        if (mainMixer != null)
        {
            float vol = Mathf.Log10(Mathf.Clamp(value, 0.0001f, 1f)) * 20f;
            mainMixer.SetFloat("MusicVolume", vol);
        }
    }

    private void SetSFXVolumeLocal(float value)
    {
        PlayerPrefs.SetFloat("SFXVolume", value);
        if (mainMixer != null)
        {
            float vol = Mathf.Log10(Mathf.Clamp(value, 0.0001f, 1f)) * 20f;
            mainMixer.SetFloat("SFXVolume", vol);
        }
    }
}

