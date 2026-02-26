using System;
using UnityEngine;
using UnityEngine.UI;

public class TimeManager : MonoBehaviour
{
    SimulationHandler simulationHandler;
    public uint day;
    public Image[] allMapResources;
    public Gradient dayNightGradient;
    private float timer;
    private bool isPaused;

    [Header("Day Slider")]
    public Slider daySlider;
    public Image daySliderFill;
    public Color playingColor = Color.white;
    public Color pausedColor = Color.red;

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

        day = simulationHandler.simulationTime / 60 + 1;
        float timeOfTheDay = Mathf.Pow(Mathf.Cos((simulationHandler.simulationTime % 60 + timer) / 60f * Mathf.PI * 2)*0.5f+0.5f, 0.28f);
        foreach (var img in allMapResources)
            img.color = dayNightGradient.Evaluate(timeOfTheDay);

        // Day progress slider (0-1 within current day)
        if (daySlider != null)
        {
            float dayProgress = (simulationHandler.simulationTime % 60 + timer) / 60f;
            daySlider.value = dayProgress;
        }

        // Fill color: playing vs paused
        if (daySliderFill != null)
            daySliderFill.color = isPaused ? pausedColor : playingColor;
    }
}
