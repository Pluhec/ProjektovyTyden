using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SkillPopup : MonoBehaviour
{
    [Header("References")]
    public TextMeshProUGUI skillNameText;
    public Button unlockButton;
    public Button closeButton;

    private int _skillId;
    private SkillButton _skillButton;

    public void Init(int skillId, SkillButton skillButton)
    {
        _skillId = skillId;
        _skillButton = skillButton;

        if (skillNameText != null)
            skillNameText.text = $"Skill {skillId}";

        if (unlockButton != null)
        {
            bool alreadyUnlocked = skillButton.skillState == SkillButton.SkillState.Unlocked;
            unlockButton.interactable = !alreadyUnlocked;

            TextMeshProUGUI btnText = unlockButton.GetComponentInChildren<TextMeshProUGUI>();
            if (btnText != null)
                btnText.text = alreadyUnlocked ? "Odemčeno" : "Odemknout";

            unlockButton.onClick.RemoveAllListeners();
            unlockButton.onClick.AddListener(OnUnlockClicked);
        }

        if (closeButton != null)
        {
            closeButton.onClick.RemoveAllListeners();
            closeButton.onClick.AddListener(Close);
        }
    }

    private void OnUnlockClicked()
    {
        _skillButton.SetState(SkillButton.SkillState.Unlocked);

        SkillTreeConnector connector = FindObjectOfType<SkillTreeConnector>();
        if (connector != null)
        {
            connector.RefreshAllAvailability();
            connector.RefreshLineColors();
            connector.AnimateUnlockedLines(_skillButton.GetComponent<RectTransform>());
        }

        Debug.Log("Před Close");
        Close();
        Debug.Log("Po Close - tento log se už neukáže pokud Destroy funguje");
    }

    private void Close()
    {
        Destroy(gameObject);
    }
}