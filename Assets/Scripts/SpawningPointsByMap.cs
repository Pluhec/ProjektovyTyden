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

    [Header("Ideology Scaling")]
    [Tooltip("If true, spawn rate decreases when this ideology is rare in the population (riots, protests). If false, spawns normally regardless of ideology distribution (money, resources).")]
    public bool scaleByIdeologyProportion = false;
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

    // Cache ideology proportions to avoid recalculating every spawn attempt
    private readonly Dictionary<IconSpawnSettings, float> ideologyProportions = new Dictionary<IconSpawnSettings, float>();
    private float proportionCacheTimer = 0f;
    [Tooltip("How often (in seconds) to recalculate global ideology proportions.")]
    public float proportionUpdateInterval = 2f;

    void Start()
    {
        spawnTimer = spawnInterval;
        proportionCacheTimer = proportionUpdateInterval;

        // Calculate initial proportions
        if (simulationHandler != null && simulationHandler.npcBuffer != null)
        {
            UpdateIdeologyProportions();
        }
    }

    void Update()
    {
        if (simulationHandler == null || simulationHandler.npcBuffer == null || iconTypes.Count == 0)
            return;

        proportionCacheTimer -= Time.deltaTime;
        if (proportionCacheTimer <= 0f)
        {
            proportionCacheTimer = proportionUpdateInterval;
            UpdateIdeologyProportions();
        }

        spawnTimer -= Time.deltaTime;
        if (spawnTimer <= 0f)
        {
            spawnTimer = spawnInterval;
            TrySpawnPrefabs();
        }
    }

    /// <summary>
    /// Calculates what proportion of the total population falls into each icon type's ideology range.
    /// This is used to scale spawn probability for riots/protests - if only 5% of the population is extreme, riots should be much rarer.
    /// Only applies to icon types with scaleByIdeologyProportion = true.
    /// </summary>
    private void UpdateIdeologyProportions()
    {
        int numNPCs = simulationHandler.gridSize.x * simulationHandler.gridSize.y;

        if (localNpcsArray == null || localNpcsArray.Length != numNPCs)
        {
            localNpcsArray = new NPC[numNPCs];
        }

        simulationHandler.npcBuffer.GetData(localNpcsArray);

        ideologyProportions.Clear();

        foreach (var iconType in iconTypes)
        {
            // Skip ideology scaling for money and other non-riot icons
            if (!iconType.scaleByIdeologyProportion)
            {
                continue;
            }

            int totalPopulation = 0;
            int matchingPopulation = 0;

            float minValue = Mathf.Min(iconType.minIdeologyValue, iconType.maxIdeologyValue);
            float maxValue = Mathf.Max(iconType.minIdeologyValue, iconType.maxIdeologyValue);

            for (int i = 0; i < numNPCs; i++)
            {
                NPC npc = localNpcsArray[i];
                if (npc.population == 0) continue;

                totalPopulation += npc.population;

                // Convert stance to ideology (0=blue, 0.5=neutral, 1=red)
                float blueNormalized = npc.stance / 255f;
                float ideologyValue = 1f - blueNormalized;

                if (ideologyValue >= minValue && ideologyValue <= maxValue)
                {
                    matchingPopulation += npc.population;
                }
            }

            // Calculate proportion (0 to 1)
            float proportion = totalPopulation > 0 ? (float)matchingPopulation / totalPopulation : 0f;

            // Square the proportion to make rare ideologies exponentially less likely to spam spawns
            // Example: 5% of population → 0.05² = 0.0025 = 0.25% spawn rate multiplier
            float scaledProportion = proportion * proportion;

            ideologyProportions[iconType] = scaledProportion;

            Debug.Log($"[Riot Scaling] {iconType.iconType} ({minValue:F2}-{maxValue:F2}): {proportion * 100f:F1}% of population → {scaledProportion * 100f:F2}% spawn rate");
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

                float baseProbability = CalculateSpawnProbability(iconType, ideologyValue);

                // Scale probability by global ideology proportion (only for riots/protests, not money)
                float finalProbability = baseProbability;
                if (iconType.scaleByIdeologyProportion && ideologyProportions.TryGetValue(iconType, out float cachedProportion))
                {
                    // If only 5% of population has this ideology, multiply by 0.05² = 0.0025
                    float proportionScale = Mathf.Max(cachedProportion, 0.01f); // Minimum 1% to prevent complete suppression
                    finalProbability = baseProbability * proportionScale;
                }

                if (Random.value <= finalProbability)
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
