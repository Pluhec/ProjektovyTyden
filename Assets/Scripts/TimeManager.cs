using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TimeManager : MonoBehaviour
{
    SimulationHandler simulationHandler;
    public uint day;
    public TextMeshProUGUI dayText;
    public Image[] allMapResources;
    public Gradient dayNightGradient;
    private float timer;
    private float prevTimeOfTheDay = 1f;

    void Start()
    {
        simulationHandler = FindFirstObjectByType<SimulationHandler>();
    }
    void Update()
    {
        if (simulationHandler == null) return;

        timer += Time.deltaTime;
        if (timer >= 1f)
        {
            timer = 0f;
            simulationHandler.Tick();
            simulationHandler.simulationTime++;
        }

        float timeOfTheDay = Mathf.Pow(Mathf.Cos((simulationHandler.simulationTime % 60 + timer) / 60f * Mathf.PI * 2)*0.5f+0.5f, 0.28f);
        print(timeOfTheDay);
        foreach (var img in allMapResources)
            img.color = dayNightGradient.Evaluate(timeOfTheDay);

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
