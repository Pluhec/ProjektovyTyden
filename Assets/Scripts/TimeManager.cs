using UnityEngine;

public class TimeManager : MonoBehaviour
{
    SimulationHandler simulationHandler;
    public uint day;

    void Start()
    {
        simulationHandler = FindFirstObjectByType<SimulationHandler>();
    }
    void Update()
    {
        if (simulationHandler == null) return;

        day = simulationHandler.simulationTime / 60 + 1;
    }
}
