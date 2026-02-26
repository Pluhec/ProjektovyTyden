using UnityEngine;
using TMPro;
using PlayerChoice.DataSets;

/// <summary>
/// Attach this to any UI Text (TMP) object.
/// It will automatically update whenever PlayerStats.Money changes.
/// </summary>
public class MoneyDisplay : MonoBehaviour
{
    [Header("References")]
    [Tooltip("TextMeshPro text that shows the current money amount.")]
    public TextMeshProUGUI moneyText;

    [Header("Display Format")]
    [Tooltip("Format string. Use {0} for the amount. E.g. '{0}$' or 'Peníze: {0}'")]
    public string format = "{0}$";

    private void Awake()
    {
        if (moneyText == null)
            moneyText = GetComponent<TextMeshProUGUI>();
    }

    private void OnEnable()
    {
        PlayerStats.OnMoneyChanged += HandleMoneyChanged;
        Refresh();
    }

    private void OnDisable()
    {
        PlayerStats.OnMoneyChanged -= HandleMoneyChanged;
    }

    private void HandleMoneyChanged(int newAmount)
    {
        UpdateText(newAmount);
    }

    /// <summary>
    /// Forces an immediate refresh from the current PlayerStats.Money value.
    /// </summary>
    [ContextMenu("Refresh")]
    public void Refresh()
    {
        UpdateText(PlayerStats.Money);
    }

    private void UpdateText(int amount)
    {
        if (moneyText == null)
            return;

        moneyText.text = string.Format(format, amount);
    }
}
