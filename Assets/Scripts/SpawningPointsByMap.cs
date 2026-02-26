using System.Collections.Generic;
using UnityEngine;

public enum SpawnProbabilityMode
{
    Constant,
    RedToBlue,
    BlueToRed
}

[System.Serializable]
public class IconSpawnSettings
{
    [Tooltip("Icon type assigned to spawned popup/icon object.")]
    public IconType iconType;
    [Tooltip("Prefab to instantiate when this spawn setting is selected.")]
    public GameObject prefabToSpawn;

    [Header("Ideology Range")]
    [Tooltip("Minimum ideology value to spawn (0 = blue, 0.5 = neutral, 1 = red).")]
    [Range(0f, 1f)]
    public float minIdeologyValue = 0f;

    [Tooltip("Maximum ideology value to spawn (0 = blue, 0.5 = neutral, 1 = red).")]
    [Range(0f, 1f)]
    public float maxIdeologyValue = 1.0f;

    [Header("Spawning Settings")]
    [Tooltip("Used when Spawn Probability Mode is Constant.")]
    [Range(0f, 1f)]
    public float constantSpawnProbability = 0.5f;

    [Tooltip("Used as the low end for RedToBlue/BlueToRed modes.")]
    [Range(0f, 1f)]
    public float minSpawnProbability = 0.0f;
    [Tooltip("Used as the high end for RedToBlue/BlueToRed modes.")]
    [Range(0f, 1f)]
    public float maxSpawnProbability = 1.0f;
    [Tooltip("Constant = always use Min Spawn Probability. RedToBlue = interpolate from red(1) to blue(0). BlueToRed = interpolate from blue(0) to red(1).")]
    public SpawnProbabilityMode spawnProbabilityMode = SpawnProbabilityMode.Constant;

    [Header("Group Settings")]
    [Tooltip("Minimum number of neighbors (0-8) that must also pass the color threshold.")]
    [Range(0, 8)]
    public int minSameColorNeighbors = 3;
}

public class SpawningPointsByMap : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Simulation handler containing NPC grid data and map transform.")]
    public SimulationHandler simulationHandler;

    [Header("Spawning Settings")]
    [Tooltip("Seconds between spawn attempt batches.")]
    public float spawnInterval = 1f;
    [Tooltip("How many random spawn attempts are made each interval.")]
    public int spawnAttemptsPerInterval = 10;
    [Range(0f, 0.5f)]
    [Tooltip("Random position jitter inside each grid cell when spawning prefabs.")]
    public float randomOffsetInCell = 0.4f;

    [Header("Icon Types")]
    [Tooltip("Per-icon spawn rules and prefab settings.")]
    public List<IconSpawnSettings> iconTypes = new List<IconSpawnSettings>();

    private float spawnTimer;
    private NPC[] localNpcsArray;

    void Start()
    {
        spawnTimer = spawnInterval;
    }

    void Update()
    {
        if (simulationHandler == null || simulationHandler.npcBuffer == null || iconTypes.Count == 0)
            return;

        spawnTimer -= Time.deltaTime;
        if (spawnTimer <= 0f)
        {
            spawnTimer = spawnInterval;
            TrySpawnPrefabs();
        }
    }

    private void TrySpawnPrefabs()
    {
        int numNPCs = simulationHandler.gridSize.x * simulationHandler.gridSize.y;

        // Initialize or resize the local array if needed
        if (localNpcsArray == null || localNpcsArray.Length != numNPCs)
        {
            localNpcsArray = new NPC[numNPCs];
        }

        // Read the latest data from the compute buffer
        simulationHandler.npcBuffer.GetData(localNpcsArray);

        for (int i = 0; i < spawnAttemptsPerInterval; i++)
        {
            // Pick a random NPC index
            int randomIndex = Random.Range(0, numNPCs);
            NPC randomNpc = localNpcsArray[randomIndex];

            if (randomNpc.population == 0)
            {
                continue;
            }

            // stance byte: 0=red in shader, 128=neutral, 255=blue
            // map to ideology where 0=blue, 0.5=neutral, 1=red
            float blueNormalized = randomNpc.stance / 255f;
            float ideologyValue = 1f - blueNormalized;

            // Try to spawn one of the icon types
            foreach (var iconType in iconTypes)
            {
                if (iconType.prefabToSpawn == null) continue;

                float minValue = Mathf.Min(iconType.minIdeologyValue, iconType.maxIdeologyValue);
                float maxValue = Mathf.Max(iconType.minIdeologyValue, iconType.maxIdeologyValue);

                if (ideologyValue < minValue || ideologyValue > maxValue)
                {
                    continue;
                }

                int gridX = randomIndex % simulationHandler.gridSize.x;
                int gridY = randomIndex / simulationHandler.gridSize.x;

                int sameColorNeighbors = CountSameColorNeighbors(gridX, gridY, iconType);
                if (sameColorNeighbors < iconType.minSameColorNeighbors)
                {
                    continue;
                }

                float spawnProbability = CalculateSpawnProbability(iconType, ideologyValue);

                if (Random.value <= spawnProbability)
                {
                    SpawnPrefabAtNpcIndex(randomIndex, iconType);
                    break; // Only spawn one icon per attempt
                }
            }
        }
    }

    private float CalculateSpawnProbability(IconSpawnSettings settings, float ideologyValue)
    {
        float constantProbability = Mathf.Clamp01(settings.constantSpawnProbability);
        float minProbability = Mathf.Clamp01(settings.minSpawnProbability);
        float maxProbability = Mathf.Clamp01(settings.maxSpawnProbability);

        switch (settings.spawnProbabilityMode)
        {
            case SpawnProbabilityMode.RedToBlue:
                // ideology: red=1, blue=0
                return Mathf.Lerp(minProbability, maxProbability, 1f - Mathf.Clamp01(ideologyValue));

            case SpawnProbabilityMode.BlueToRed:
                // ideology: blue=0, red=1
                return Mathf.Lerp(minProbability, maxProbability, Mathf.Clamp01(ideologyValue));

            case SpawnProbabilityMode.Constant:
            default:
                return constantProbability;
        }
    }

    private int CountSameColorNeighbors(int x, int y, IconSpawnSettings settings)
    {
        int count = 0;
        int width = simulationHandler.gridSize.x;
        int height = simulationHandler.gridSize.y;

        for (int ny = y - 1; ny <= y + 1; ny++)
        {
            for (int nx = x - 1; nx <= x + 1; nx++)
            {
                if (nx == x && ny == y) continue;
                if (nx < 0 || nx >= width || ny < 0 || ny >= height) continue;

                int neighborIndex = ny * width + nx;
                NPC neighborNpc = localNpcsArray[neighborIndex];

                if (neighborNpc.population == 0) continue;

                float blueNormalized = neighborNpc.stance / 255f;
                float ideologyValue = 1f - blueNormalized;
                float minValue = Mathf.Min(settings.minIdeologyValue, settings.maxIdeologyValue);
                float maxValue = Mathf.Max(settings.minIdeologyValue, settings.maxIdeologyValue);

                if (ideologyValue >= minValue && ideologyValue <= maxValue)
                {
                    count++;
                }
            }
        }
        return count;
    }

    private void SpawnPrefabAtNpcIndex(int npcIndex, IconSpawnSettings settings)
    {
        int gridX = npcIndex % simulationHandler.gridSize.x;
        int gridY = npcIndex / simulationHandler.gridSize.x;

        Transform quadTransform = simulationHandler.simulationQuadTransform;
        
        float cellSizeX = 1f / simulationHandler.gridSize.x * quadTransform.localScale.x;
        float cellSizeY = 1f / simulationHandler.gridSize.y * quadTransform.localScale.y;

        float gridPosX = gridX * cellSizeX;
        float gridPosY = gridY * cellSizeY;

        float gridCenterX = simulationHandler.gridSize.x * cellSizeX * 0.5f;
        float gridCenterY = simulationHandler.gridSize.y * cellSizeY * 0.5f;

        float worldX = gridPosX - gridCenterX + quadTransform.position.x + (cellSizeX * 0.5f);
        float worldY = gridPosY - gridCenterY + quadTransform.position.y + (cellSizeY * 0.5f);

        Vector3 spawnPosition = new Vector3(worldX, worldY, 0f);

        // Add some random offset within the cell so they don't all spawn exactly in the center
        float randomOffsetX = Random.Range(-cellSizeX * randomOffsetInCell, cellSizeX * randomOffsetInCell);
        float randomOffsetY = Random.Range(-cellSizeY * randomOffsetInCell, cellSizeY * randomOffsetInCell);
        spawnPosition += new Vector3(randomOffsetX, randomOffsetY, 0f);

        GameObject spawned = Instantiate(settings.prefabToSpawn, spawnPosition, Quaternion.identity, transform);

        IconShower iconShower = spawned.GetComponent<IconShower>();
        if (iconShower != null)
        {
            iconShower.SetIconType(settings.iconType);
        }

        // Align the popup so that its bottom point (pivotOffset anchor) is exactly at target spawn point.
        popup_click popup = spawned.GetComponent<popup_click>();
        if (popup != null)
        {
            spawned.transform.position = spawnPosition - popup.pivotOffset;
        }
    }
}
