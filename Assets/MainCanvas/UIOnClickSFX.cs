using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class UIOnClickSFX : MonoBehaviour, IPointerClickHandler
{
    [Header("Optional")]
    [SerializeField] private AudioClip overrideClip;

    [Header("Behavior")]
    [Tooltip("U ToggleGroup/ON-OFF je lepší hrát jen při zapnutí, aby to nehrálo dvakrát.")]
    [SerializeField] private bool togglePlayOnlyWhenTurnedOn = true;

    [Tooltip("Fallback zvuk na pointer click (např. pro Image apod.). Pro Dropdown doporučuji vypnout.")]
    [SerializeField] private bool enablePointerClickFallback = true;

    private Button button;
    private Toggle toggle;
    private Dropdown dropdown;
    private TMP_Dropdown tmpDropdown;

    private int lastDropdownValue;
    private bool hasDropdown;

    private void Awake()
    {
        button = GetComponent<Button>();
        toggle = GetComponent<Toggle>();
        dropdown = GetComponent<Dropdown>();
        tmpDropdown = GetComponent<TMP_Dropdown>();

        if (dropdown != null)
        {
            hasDropdown = true;
            lastDropdownValue = dropdown.value;
        }
        else if (tmpDropdown != null)
        {
            hasDropdown = true;
            lastDropdownValue = tmpDropdown.value;
        }
    }

    private void OnEnable()
    {
        if (button != null) button.onClick.AddListener(Play);

        if (toggle != null) toggle.onValueChanged.AddListener(OnToggleChanged);

        if (dropdown != null) dropdown.onValueChanged.AddListener(OnDropdownChanged);
        if (tmpDropdown != null) tmpDropdown.onValueChanged.AddListener(OnTmpDropdownChanged);

        // reset (když se UI znovu zapíná po scéně/pause menu)
        if (dropdown != null) lastDropdownValue = dropdown.value;
        if (tmpDropdown != null) lastDropdownValue = tmpDropdown.value;
    }

    private void OnDisable()
    {
        if (button != null) button.onClick.RemoveListener(Play);

        if (toggle != null) toggle.onValueChanged.RemoveListener(OnToggleChanged);

        if (dropdown != null) dropdown.onValueChanged.RemoveListener(OnDropdownChanged);
        if (tmpDropdown != null) tmpDropdown.onValueChanged.RemoveListener(OnTmpDropdownChanged);
    }

    // --- Button ---
    private void Play()
    {
        var am = AudioManager.Instance;
        if (am == null) return;

        if (overrideClip != null) am.PlaySFX(overrideClip);
        else am.PlayClickSound();
    }

    // --- Toggle ---
    private void OnToggleChanged(bool isOn)
    {
        if (togglePlayOnlyWhenTurnedOn && !isOn) return;
        Play();
    }

    // --- Dropdown (UGUI) ---
    private void OnDropdownChanged(int value)
    {
        if (value == lastDropdownValue) return;
        lastDropdownValue = value;
        Play();
    }

    // --- Dropdown (TMP) ---
    private void OnTmpDropdownChanged(int value)
    {
        if (value == lastDropdownValue) return;
        lastDropdownValue = value;
        Play();
    }

    // --- Pointer click fallback ---
    public void OnPointerClick(PointerEventData eventData)
    {
        if (!enablePointerClickFallback) return;

        // Když je to dropdown, nechceme hrát při otevření (jen při změně).
        if (hasDropdown) return;

        // Když je to Button, onClick už to vyřeší.
        if (button != null) return;

        // Toggle už řešíme přes onValueChanged (spolehlivější).
        if (toggle != null) return;

        Play();
    }
}