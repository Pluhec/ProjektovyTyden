using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Audio Mixer")]
    public AudioMixer mainMixer;

    [Header("Audio Sources")]
    public AudioSource musicSource;
    public AudioSource sfxSource;

    [Header("UI Sliders (Optional references)")]
    public Slider musicSlider;
    public Slider sfxSlider;

    [Header("Clips")]
    public AudioClip backgroundMusic; // Sem přetáhneš svou hudbu
    public AudioClip clickSound;

    private void Awake()
    {
        // Singleton pattern - aby existoval jen jeden AudioManager
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        // Spuštění hudby na pozadí, pokud je nastavena a ještě nehraje
        if (backgroundMusic != null && musicSource != null)
        {
            if (musicSource.clip != backgroundMusic)
            {
                musicSource.clip = backgroundMusic;
                musicSource.loop = true;
                musicSource.Play();
            }
        }

        // Načteme uložené hodnoty hlasitosti, pokud existují, jinak default
        if (PlayerPrefs.HasKey("MusicVolume"))
            LoadVolume("MusicVolume", musicSlider);
        
        if (PlayerPrefs.HasKey("SFXVolume"))
            LoadVolume("SFXVolume", sfxSlider);
        
        // Nastavíme slidery, aby reagovaly na změnu (pokud jsou přiřazené)
        if (musicSlider != null)
            musicSlider.onValueChanged.AddListener(SetMusicVolume);
        
        if (sfxSlider != null)
            sfxSlider.onValueChanged.AddListener(SetSFXVolume);
    }

    // --- Metody pro Slidery ---

    public void SetMusicVolume(float value)
    {
        // Slider je 0 až 1, Mixer je -80dB až 0dB
        // Použijeme logaritmickou škálu pro přirozenější zeslabování
        float volume = Mathf.Log10(Mathf.Clamp(value, 0.0001f, 1f)) * 20;
        
        mainMixer.SetFloat("MusicVolume", volume);
        PlayerPrefs.SetFloat("MusicVolume", value); // Uložíme 0-1 hodnotu slideru
    }

    public void SetSFXVolume(float value)
    {
        float volume = Mathf.Log10(Mathf.Clamp(value, 0.0001f, 1f)) * 20;
        
        mainMixer.SetFloat("SFXVolume", volume);
        PlayerPrefs.SetFloat("SFXVolume", value);
    }

    private void LoadVolume(string parameterName, Slider slider)
    {
        float value = PlayerPrefs.GetFloat(parameterName, 0.75f);
        if (slider != null) slider.value = value;
        
        // Aplikujeme hlasitost
        float volume = Mathf.Log10(Mathf.Clamp(value, 0.0001f, 1f)) * 20;
        mainMixer.SetFloat(parameterName, volume);
    }

    // --- Metody pro hraní zvuků ---

    public void PlayClickSound()
    {
        if (clickSound != null)
        {
            sfxSource.PlayOneShot(clickSound);
        }
    }

    public void PlaySFX(AudioClip clip)
    {
        if (clip != null)
        {
            sfxSource.PlayOneShot(clip);
        }
    }

}
