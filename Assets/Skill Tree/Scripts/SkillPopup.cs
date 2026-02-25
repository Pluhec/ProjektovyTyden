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

            unlockButton.onClick.AddListener(OnUnlockClicked);
        }

        if (closeButton != null)
            closeButton.onClick.AddListener(Close);
    }

    private void OnUnlockClicked()
    {
        // TODO: Tady později přidáš volání na logiku peněz
        // bool success = SkillManager.Instance.TryUnlockSkill(_skillId);

        _skillButton.SetState(SkillButton.SkillState.Unlocked);

        // Aktualizuj dostupnost childů
        SkillTreeConnector connector = FindObjectOfType<SkillTreeConnector>();
        if (connector != null)
            connector.RefreshAllAvailability();

        Close();
    }

    private void Close()
    {
        Destroy(gameObject);
    }
}