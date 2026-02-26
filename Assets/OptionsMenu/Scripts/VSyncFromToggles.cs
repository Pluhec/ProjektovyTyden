using UnityEngine;
using UnityEngine.UI;

public class VSyncFromToggles : MonoBehaviour
{
    [SerializeField] private Toggle vsyncOnToggle; // Toggle "ON"

    private const string PrefKey = "VSyncEnabled";

    private void Awake()
    {
        // 1) Načti uloženou hodnotu (default ON)
        bool saved = PlayerPrefs.GetInt(PrefKey, 1) == 1;

        // 2) Nastav UI podle uložené hodnoty (ToggleGroup se přepne sám)
        vsyncOnToggle.isOn = saved;

        // 3) Aplikuj na Unity
        ApplyVSync(saved);

        // 4) Poslouchej změny
        vsyncOnToggle.onValueChanged.AddListener(OnVSyncToggleChanged);
    }

    private void OnDestroy()
    {
        vsyncOnToggle.onValueChanged.RemoveListener(OnVSyncToggleChanged);
    }

    private void OnVSyncToggleChanged(bool enabled)
    {
        ApplyVSync(enabled);

        // Ulož volbu
        PlayerPrefs.SetInt(PrefKey, enabled ? 1 : 0);
        PlayerPrefs.Save();
    }

    private void ApplyVSync(bool enabled)
    {
        // 0 = vyp, 1 = zap
        QualitySettings.vSyncCount = enabled ? 1 : 0;
    }
}