using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Runtime.CompilerServices;

public class TimeManager : MonoBehaviour
{
    SimulationHandler simulationHandler;
    CloudTimeController cloudTimeController;
    public uint day = 1;
    public TextMeshProUGUI dayText;
    public Image[] allMapResources;
    public Gradient dayNightGradient;
    private float timer;
    private bool isPaused;
    private float prevTimeOfTheDay = 1f;

    [Header("Day Slider")]
    public Slider daySlider;
    public Image daySliderFill;
    public Material playingMaterial;
    public Material pausedMaterial;

    public void TogglePause()
    {
        isPaused = !isPaused;
        if (cloudTimeController != null)
            cloudTimeController.stopped = isPaused;
    }

    void Start()
    {
        simulationHandler = FindFirstObjectByType<SimulationHandler>();
        cloudTimeController = FindFirstObjectByType<CloudTimeController>();
        if (daySlider != null)
            daySlider.maxValue = 1f;
    }
    void Update()
    {
        if (simulationHandler == null) return;

        if (Input.GetKeyDown(KeyCode.Space))
        {
            isPaused = !isPaused;
            if (cloudTimeController != null)
                cloudTimeController.stopped = isPaused;
        }

        if (!isPaused)
        {
            timer += Time.deltaTime;
            if (timer >= 1f)
            {
                timer = 0f;
                simulationHandler.Tick();
                simulationHandler.simulationTime++;
            }
        }

        float timeOfTheDay = Mathf.Cos((simulationHandler.simulationTime % 60 + timer) / 60f * Mathf.PI * 2)*0.5f+0.5f;
        foreach (var img in allMapResources)
            img.color = dayNightGradient.Evaluate(Mathf.Clamp01(timeOfTheDay));

        if (daySlider != null)
        {
            float dayProgress = ((simulationHandler.simulationTime + 30) % 60 + timer) / 60f;
            daySlider.value = dayProgress;
        }

        if (daySliderFill != null)
            daySliderFill.material = isPaused ? pausedMaterial : playingMaterial;

        day = (uint)((simulationHandler.simulationTime + 30) / 60) + 1;

        if (dayText != null)
        {
            var startDate = new System.DateTime(2026, 1, 1);
            var currentDate = startDate.AddDays(day - 1);
            var czechCulture = new System.Globalization.CultureInfo("cs-CZ");
            string dateString = currentDate.ToString("d. MMMM yyyy", czechCulture);
            dayText.text = dateString;
        }
    }

}
