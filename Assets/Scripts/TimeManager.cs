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

        day = simulationHandler.simulationTime / 60 + 1;
        float timeOfTheDay = Mathf.Pow(Mathf.Cos((simulationHandler.simulationTime % 60 + timer) / 60f * Mathf.PI * 2)*0.5f+0.5f, 0.28f);
        print(timeOfTheDay);
        foreach (var img in allMapResources)
            img.color = dayNightGradient.Evaluate(timeOfTheDay);
    }
}
