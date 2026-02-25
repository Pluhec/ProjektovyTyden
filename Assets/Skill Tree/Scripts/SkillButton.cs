using UnityEngine;
using UnityEngine.UI;

public class SkillButton : MonoBehaviour
{
    public enum PrimarySkillTier { Tier1, Tier2 }
    public enum SkillTier { Tier1, Tier2, Tier3 }

    [Header("Skill Type")]
    public bool isPrimarySkill = false;

    [Header("Primary Skill")]
    public PrimarySkillTier primarySkillTier = PrimarySkillTier.Tier1;
    public Sprite primaryTier1Sprite;
    public Sprite primaryTier2Sprite;

    [Header("Secondary Skill")]
    public SkillTier skillTier = SkillTier.Tier1;
    public Sprite secondaryTier1Sprite;
    public Sprite secondaryTier2Sprite;
    public Sprite secondaryTier3Sprite;

    [Header("Skill Icon")]
    public Sprite skillIcon;

    [Header("References")]
    public Image iconImage;

    // Velikosti
    private static readonly Vector2 PrimarySize = new Vector2(90f, 100f);
    private static readonly Vector2 SecondarySize = new Vector2(70f, 70f);

    private static readonly Vector2 PrimaryIconSize = new Vector2(64f, 64f);
    private static readonly Vector2 SecondaryIconSize = new Vector2(44f, 44f);

    private void OnValidate()
    {
        Apply();
    }

    private void Awake()
    {
        Apply();
    }

    public void Apply()
    {
        RectTransform rt = GetComponent<RectTransform>();
        Image bgImage = GetComponent<Image>();

        if (isPrimarySkill)
        {
            rt.sizeDelta = PrimarySize;

            if (bgImage != null)
            {
                Sprite tierSprite = primarySkillTier switch
                {
                    PrimarySkillTier.Tier1 => primaryTier1Sprite,
                    PrimarySkillTier.Tier2 => primaryTier2Sprite,
                    _ => null
                };

                if (tierSprite != null)
                    bgImage.sprite = tierSprite;
            }
        }
        else
        {
            rt.sizeDelta = SecondarySize;

            if (bgImage != null)
            {
                Sprite tierSprite = skillTier switch
                {
                    SkillTier.Tier1 => secondaryTier1Sprite,
                    SkillTier.Tier2 => secondaryTier2Sprite,
                    SkillTier.Tier3 => secondaryTier3Sprite,
                    _ => null
                };

                if (tierSprite != null)
                    bgImage.sprite = tierSprite;
            }
        }

        // Nastav ikonku
        if (iconImage != null)
        {
            if (skillIcon != null)
                iconImage.sprite = skillIcon;

            iconImage.GetComponent<RectTransform>().sizeDelta =
                isPrimarySkill ? PrimaryIconSize : SecondaryIconSize;
        }
    }
}