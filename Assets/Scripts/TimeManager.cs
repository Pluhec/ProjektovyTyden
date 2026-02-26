using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TimeManager : MonoBehaviour
{
    SimulationHandler simulationHandler;
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

    void Start()
    {
        simulationHandler = FindFirstObjectByType<SimulationHandler>();
        if (daySlider != null)
            daySlider.maxValue = 1f;
    }
    void Update()
    {
        if (simulationHandler == null) return;

        if (Input.GetKeyDown(KeyCode.Space))
            isPaused = !isPaused;

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

        float timeOfTheDay = Mathf.Pow(Mathf.Cos((simulationHandler.simulationTime % 60 + timer) / 60f * Mathf.PI * 2)*0.5f+0.5f, 0.28f);
        foreach (var img in allMapResources)
            img.color = dayNightGradient.Evaluate(timeOfTheDay);

        // Day progress slider (0-1 within current day)
        if (daySlider != null)
        {
            float dayProgress = (simulationHandler.simulationTime % 60 + timer) / 60f;
            daySlider.value = dayProgress;
        }

        // Switch material based on pause state
        if (daySliderFill != null)
            daySliderFill.material = isPaused ? pausedMaterial : playingMaterial;

        // Increment day only when timeOfTheDay crosses from high to low (night)
        float nightThreshold = 0.1f;
        if (prevTimeOfTheDay > nightThreshold && timeOfTheDay <= nightThreshold)
        {
            day++;
        }
        prevTimeOfTheDay = timeOfTheDay;

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
