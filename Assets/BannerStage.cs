using UnityEngine;
using UnityEngine.UI;

public class BannerStage : MonoBehaviour
{
    public enum Stage { Stage1, Stage2, Stage3 }

    [Header("Editor override (přepínáš ručně v Inspectoru)")]
    [SerializeField] private Stage currentStage = Stage.Stage1;

    [Header("UI")]
    [SerializeField] private Image bannerImage;

    [Header("Bannery pro stage")]
    [SerializeField] private Sprite bannerStage1;
    [SerializeField] private Sprite bannerStage2;
    [SerializeField] private Sprite bannerStage3;

    private void OnValidate()
    {
        Apply();
    }

    private void Start()
    {
        Apply();
    }

    public void SetStage(Stage stage)
    {
        currentStage = stage;
        Apply();
    }

    private void Apply()
    {
        if (!bannerImage) return;

        bannerImage.sprite = currentStage switch
        {
            Stage.Stage1 => bannerStage1,
            Stage.Stage2 => bannerStage2,
            Stage.Stage3 => bannerStage3,
            _ => bannerStage1
        };
    }
}