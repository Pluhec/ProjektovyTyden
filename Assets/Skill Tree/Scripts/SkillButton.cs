using UnityEngine;
using UnityEngine.UI;

public class SkillButton : MonoBehaviour
{
    public enum SkillState { Locked, Available, Unlocked }
    public enum PrimarySkillTier { Tier1, Tier2 }
    public enum SkillTier { Tier1, Tier2, Tier3 }

    [Header("Skill Identity")]
    public int skillId;

    [Header("Skill Type")]
    public bool isPrimarySkill = false;

    [Header("Primary Skill Sprites")]
    public PrimarySkillTier primarySkillTier = PrimarySkillTier.Tier1;
    public Sprite primaryTier1Sprite;
    public Sprite primaryTier2Sprite;
    public Sprite primaryTier1UnlockedSprite;
    public Sprite primaryTier2UnlockedSprite;

    [Header("Secondary Skill Sprites")]
    public SkillTier skillTier = SkillTier.Tier1;
    public Sprite secondaryTier1Sprite;
    public Sprite secondaryTier2Sprite;
    public Sprite secondaryTier3Sprite;
    public Sprite secondaryTier1UnlockedSprite;
    public Sprite secondaryTier2UnlockedSprite;
    public Sprite secondaryTier3UnlockedSprite;

    [Header("Skill Icon")]
    public Sprite skillIcon;

    [Header("References")]
    public Image iconImage;
    public GameObject popupPrefab;

    [Header("State")]
    public SkillState skillState = SkillState.Locked;

    private static readonly Vector2 PrimarySize = new Vector2(90f, 100f);
    private static readonly Vector2 SecondarySize = new Vector2(70f, 70f);
    private static readonly Vector2 PrimaryIconSize = new Vector2(78f, 78f);
    private static readonly Vector2 SecondaryIconSize = new Vector2(58f, 58f);

    private static readonly Color LockedColor = new Color(0.4f, 0.4f, 0.4f, 1f);
    private static readonly Color AvailableColor = Color.white;

    private Button _button;

    private void Awake()
    {
        _button = GetComponent<Button>();
        if (_button != null)
            _button.onClick.AddListener(OnSkillClicked);

        Apply();
    }

    private void OnValidate()
    {
        Apply();
    }

    public void SetState(SkillState newState)
    {
        skillState = newState;
        Apply();

        // Po změně stavu aktualizuj čáry
        SkillTreeConnector connector = GetComponentInParent<SkillTreeConnector>();
        if (connector == null)
            connector = FindObjectOfType<SkillTreeConnector>();
        if (connector != null)
            connector.GenerateConnections();
    }

    private void OnSkillClicked()
    {
        if (skillState == SkillState.Locked) return;

        if (popupPrefab == null)
        {
            Debug.LogWarning($"SkillButton [{skillId}]: popupPrefab není přiřazen!");
            return;
        }

        Canvas canvas = GetComponentInParent<Canvas>();
        GameObject popup = Instantiate(popupPrefab, canvas.transform);
        SkillPopup skillPopup = popup.GetComponent<SkillPopup>();
        if (skillPopup != null)
            skillPopup.Init(skillId, this);
    }

    public void Apply()
    {
        RectTransform rt = GetComponent<RectTransform>();
        Image bgImage = GetComponent<Image>();

        if (rt != null)
            rt.sizeDelta = isPrimarySkill ? PrimarySize : SecondarySize;

        if (bgImage != null)
        {
            bgImage.sprite = GetCurrentSprite();
            bgImage.color = skillState == SkillState.Locked ? LockedColor : AvailableColor;
        }

        if (iconImage != null)
        {
            if (skillIcon != null)
                iconImage.sprite = skillIcon;

            iconImage.color = skillState == SkillState.Locked ? LockedColor : AvailableColor;
            iconImage.GetComponent<RectTransform>().sizeDelta =
                isPrimarySkill ? PrimaryIconSize : SecondaryIconSize;
        }

        if (_button != null)
            _button.interactable = skillState != SkillState.Locked;
    }

    private Sprite GetCurrentSprite()
    {
        bool unlocked = skillState == SkillState.Unlocked;

        if (isPrimarySkill)
        {
            return primarySkillTier switch
            {
                PrimarySkillTier.Tier1 => unlocked ? primaryTier1UnlockedSprite : primaryTier1Sprite,
                PrimarySkillTier.Tier2 => unlocked ? primaryTier2UnlockedSprite : primaryTier2Sprite,
                _ => null
            };
        }
        else
        {
            return skillTier switch
            {
                SkillTier.Tier1 => unlocked ? secondaryTier1UnlockedSprite : secondaryTier1Sprite,
                SkillTier.Tier2 => unlocked ? secondaryTier2UnlockedSprite : secondaryTier2Sprite,
                SkillTier.Tier3 => unlocked ? secondaryTier3UnlockedSprite : secondaryTier3Sprite,
                _ => null
            };
        }
    }

    /// <summary>
    /// Volá se z SkillTreeConnectoru po odemčení parenta.
    /// Zkontroluje jestli jsou všichni parenti unlocked a pokud ano, nastaví Available.
    /// </summary>
    public void RefreshAvailability(bool allParentsUnlocked)
    {
        if (skillState == SkillState.Unlocked) return;
        SetState(allParentsUnlocked ? SkillState.Available : SkillState.Locked);
    }
}