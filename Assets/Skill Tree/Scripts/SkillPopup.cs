using UnityEngine;
using UnityEngine.UI;
using TMPro;
using PlayerChoice.DataSets;
using System.Reflection;

public class SkillPopup : MonoBehaviour
{
    [Header("References")]
    public Image skillIconImageBg;
    public Image skillIconImageFg;
    public TextMeshProUGUI skillNameText;
    public TextMeshProUGUI skillDescriptionText;
    public TextMeshProUGUI skillCostText;
    public Button unlockButton;
    public Button closeButton;

    [Header("Button Sprites")]
    public Sprite unlockButtonSprite;
    public Sprite unlockButtonDisabledSprite;

    private SkillButton _skillButton;
    private PerkInformation _perkInfo;

    public void Init(SkillButton skillButton)
    {
        _skillButton = skillButton;
        _perkInfo = FindPerk(skillButton.perkFieldName);

        if (_perkInfo == null)
        {
            Debug.LogWarning($"SkillPopup: Field '{skillButton.perkFieldName}' nebyl nalezen v PerkSet!");
            Close();
            return;
        }

        if (skillIconImageBg != null)
            skillIconImageBg.sprite = skillButton.GetComponent<Image>().sprite;

        if (skillIconImageFg != null && skillButton.iconImage != null)
            skillIconImageFg.sprite = skillButton.iconImage.sprite;

        if (skillNameText != null)
            skillNameText.text = _perkInfo.PerkName;

        if (skillDescriptionText != null)
            skillDescriptionText.text = _perkInfo.PerkDescription;

        if (skillCostText != null)
            skillCostText.text = $"Cena: {_perkInfo.PerkCost}$";

        RefreshUnlockButton();

        if (unlockButton != null)
        {
            unlockButton.onClick.RemoveAllListeners();
            unlockButton.onClick.AddListener(OnUnlockClicked);
        }

        if (closeButton != null)
        {
            closeButton.onClick.RemoveAllListeners();
            closeButton.onClick.AddListener(Close);
        }
    }

    private void RefreshUnlockButton()
    {
        if (unlockButton == null) return;

        bool alreadyBought = _perkInfo.IsBought;
        bool canAfford = PlayerStats.Money >= _perkInfo.PerkCost;
        bool canUnlock = !alreadyBought && canAfford;

        unlockButton.interactable = canUnlock;

        Image btnImage = unlockButton.GetComponent<Image>();
        if (btnImage != null)
            btnImage.sprite = canUnlock ? unlockButtonSprite : unlockButtonDisabledSprite;
    }

    private void OnUnlockClicked()
    {
        if (_perkInfo == null) return;

        bool success = _perkInfo.PerkPurchase();
        
        if (!success)
        {
            RefreshUnlockButton();
            return;
        }

        _skillButton.SetState(SkillButton.SkillState.Unlocked);

        SkillTreeConnector connector = FindObjectOfType<SkillTreeConnector>();
        if (connector != null)
        {
            connector.RefreshAllAvailability();
            connector.RefreshLineColors();
            connector.AnimateUnlockedLines(_skillButton.GetComponent<RectTransform>());
        }

        Close();
    }

    // Hledá podle názvu fieldu v PerkSet, např. "Coms_TVChannel"
    private PerkInformation FindPerk(string fieldName)
    {
        FieldInfo field = typeof(PerkSet).GetField(fieldName, BindingFlags.Public | BindingFlags.Static);
        if (field == null)
        {
            Debug.LogWarning($"FindPerk: Field '{fieldName}' neexistuje v PerkSet!");
            return null;
        }
        return field.GetValue(null) as PerkInformation;
    }

    private void Close()
    {
        Destroy(gameObject);
    }
}