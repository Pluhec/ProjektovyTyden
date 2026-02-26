using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Slider))]
public class VolumeSlider : MonoBehaviour
{
    private enum AudioType { Music, SFX }

    [SerializeField] private AudioType audioType;
    private Slider _slider;

    private void Start()
    {
        _slider = GetComponent<Slider>();

        // Najdeme AudioManager (protože je Singleton, je to snadné)
        if (AudioManager.Instance == null)
        {
            Debug.LogWarning("VolumeSlider: AudioManager nenalezen ve scéně!");
            return;
        }

        // Načtení aktuální hodnoty (aby slider neodskočil na default při otevření menu)
        float currentVolume = 0.5f;
        string prefsKey = audioType == AudioType.Music ? "MusicVolume" : "SFXVolume";
        
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
        if (AudioManager.Instance != null)
        {
            if (audioType == AudioType.Music)
            {
                AudioManager.Instance.SetMusicVolume(value);
            }
            else
            {
                AudioManager.Instance.SetSFXVolume(value);
            }
        }
    }
}

