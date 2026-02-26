using UnityEngine;
using UnityEngine.UI;

public class SkillTreeNavbar : MonoBehaviour
{
    [System.Serializable]
    public class NavbarSection
    {
        public Button button;
        public GameObject section;
        public Sprite activeSprite;
        public Sprite inactiveSprite;
    }

    [Header("Sections")]
    public NavbarSection section1;
    public NavbarSection section2;
    public NavbarSection section3;

    private NavbarSection[] _sections;

    private void Awake()
    {
        _sections = new NavbarSection[] { section1, section2, section3 };

        section1.button.onClick.AddListener(() => OpenSection(section1));
        section2.button.onClick.AddListener(() => OpenSection(section2));
        section3.button.onClick.AddListener(() => OpenSection(section3));

        // Defaultně otevři první sekci
        OpenSection(section1);
    }

    private void OpenSection(NavbarSection target)
    {
        foreach (var s in _sections)
        {
            if (s?.button == null || s?.section == null) continue;

            bool isActive = s == target;
            s.section.SetActive(isActive);

            Image btnImage = s.button.GetComponent<Image>();
            if (btnImage != null)
                btnImage.sprite = isActive ? s.activeSprite : s.inactiveSprite;
        }
    }
}