using UnityEngine;

public class WebsiteNav : MonoBehaviour
{
    [Header("Sections (panely)")]
    [SerializeField] private GameObject heroSection;
    [SerializeField] private GameObject narizeniSection;
    [SerializeField] private GameObject historieSection;

    public void ShowHero()
    {
        SetOnly(heroSection);
    }

    public void ShowNarizeni()
    {
        SetOnly(narizeniSection);
    }

    public void ShowHistorie()
    {
        SetOnly(historieSection);
    }

    private void SetOnly(GameObject active)
    {
        heroSection.SetActive(active == heroSection);
        narizeniSection.SetActive(active == narizeniSection);
        historieSection.SetActive(active == historieSection);
    }
}