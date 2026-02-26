using UnityEngine;
using UnityEngine.UI;

public class SkillButton : MonoBehaviour
{
    public enum SkillState { Locked, Available, Unlocked }
    public enum PrimarySkillTier { Tier1, Tier2 }
    public enum SkillTier { Tier1, Tier2, Tier3 }

    [Header("Skill Identity")]
    public string perkFieldName;

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
    public bool tweakIcon = false; // když true, velikost ikony se nebere ze skriptu ale z Unity

    [Header("References")]
    public Image iconImage;

    [Header("State")]
    public SkillState skillState = SkillState.Locked;

    private static readonly Vector2 PrimarySize = new Vector2(90f, 100f);
    private static readonly Vector2 SecondarySize = new Vector2(70f, 70f);
    private static readonly Vector2 PrimaryIconSize = new Vector2(64f, 64f);
    private static readonly Vector2 SecondaryIconSize = new Vector2(44f, 44f);

    private static readonly Color LockedColor = Color.gray;
    private static readonly Color AvailableColor = Color.white;

    private Button _button;

    private void Awake()
    {
        _button = GetComponent<Button>();
        if (_button != null)
            _button.onClick.AddListener(OnSkillClicked);
    }

    private void Start()
    {
        Apply();
    }

    private void OnValidate()
    {
        if (Application.isPlaying) return;
        Apply();
    }

    public void SetState(SkillState newState)
    {
        skillState = newState;
        Apply();
    }

    private void OnSkillClicked()
    {
        if (skillState == SkillState.Locked) return;

        SkillTreeConnector connector = FindObjectOfType<SkillTreeConnector>();
        if (connector == null || connector.popupPrefab == null)
        {
            return;
        }

        Canvas canvas = GetComponentInParent<Canvas>();
        GameObject popup = Instantiate(connector.popupPrefab, canvas.transform);
        SkillPopup skillPopup = popup.GetComponent<SkillPopup>();
        if (skillPopup != null)
            skillPopup.Init(this);
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

            // Pouze pokud tweakIcon je false nastavíme velikost ze skriptu
            if (!tweakIcon)
            {
                iconImage.GetComponent<RectTransform>().sizeDelta =
                    isPrimarySkill ? PrimaryIconSize : SecondaryIconSize;
            }
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

    public void RefreshAvailability(bool allParentsUnlocked)
    {
        if (skillState == SkillState.Unlocked) return;

        SkillState newState = allParentsUnlocked ? SkillState.Available : SkillState.Locked;
        if (skillState == newState) return;

        skillState = newState;
        Apply();
    }
}