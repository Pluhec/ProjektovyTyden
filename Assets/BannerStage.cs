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

    private SimulationHandler simulationHandler;

    private void OnValidate()
    {
        Apply();
    }

    private void Start()
    {
        simulationHandler = FindObjectOfType<SimulationHandler>();
        UpdateStageFromSimulation();
        Apply();
    }

    private void Update()
    {
        UpdateStageFromSimulation();
    }

    /// <summary>
    /// Updates banner stage based on simulation totalitarian percentage.
    /// Only counts past neutral (byte 128). Same formula as TwittirManager / NewsLoader:
    ///   0-33% = Stage1 (dem), 33-66% = Stage2 (prop), 66-100% = Stage3 (tot)
    /// </summary>
    private void UpdateStageFromSimulation()
    {
        if (simulationHandler == null) return;

        // Read the same value the progress bar shows (0-100)
        float totalitarianPct = simulationHandler.GetStancePercentage();

        Stage newStage;
        if (totalitarianPct < 33f)
            newStage = Stage.Stage1;
        else if (totalitarianPct < 66f)
            newStage = Stage.Stage2;
        else
            newStage = Stage.Stage3;

        if (newStage != currentStage)
            SetStage(newStage);
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