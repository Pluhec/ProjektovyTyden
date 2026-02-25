using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class IconSpawnSettings
{
    public IconType iconType;
    public GameObject prefabToSpawn;

    [Header("Color Settings")]
    [Tooltip("If true, checks for blue (high stance). If false, checks for red (low stance).")]
    public bool isBlue = true;
    
    [Tooltip("Minimum normalized color value (0 to 1) required to spawn.")]
    [Range(0f, 1f)]
    public float minColorThreshold = 0.55f;

    [Tooltip("Maximum normalized color value (0 to 1) allowed to spawn.")]
    [Range(0f, 1f)]
    public float maxColorThreshold = 1.0f;

    [Header("Spawning Settings")]
    [Range(0f, 1f)]
    public float minSpawnProbability = 0.0f;
    [Range(0f, 1f)]
    public float maxSpawnProbability = 1.0f;

    [Header("Group Settings")]
    [Tooltip("Minimum number of neighbors (0-8) that must also pass the color threshold.")]
    [Range(0, 8)]
    public int minSameColorNeighbors = 3;
}

public class SpawningPointsByMap : MonoBehaviour
{
    [Header("References")]
    public SimulationHandler simulationHandler;

    [Header("Spawning Settings")]
    public float spawnInterval = 1f;
    public int spawnAttemptsPerInterval = 10;
    [Range(0f, 0.5f)]
    public float randomOffsetInCell = 0.4f;

    [Header("Icon Types")]
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

            // Blue in renderer corresponds to high stance.
            // stance is -128..127, normalize to 0..1 where higher = bluer.
            float blueNormalized = (randomNpc.stance + 128f) / 255f;

            // Try to spawn one of the icon types
            foreach (var iconType in iconTypes)
            {
                if (iconType.prefabToSpawn == null) continue;

                float colorValue = iconType.isBlue ? blueNormalized : (1f - blueNormalized);

                if (colorValue < iconType.minColorThreshold || colorValue > iconType.maxColorThreshold)
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

                // Spawn chance increases with color value above threshold.
                float colorWeight = Mathf.InverseLerp(iconType.minColorThreshold, iconType.maxColorThreshold, colorValue);
                float spawnProbability = Mathf.Lerp(iconType.minSpawnProbability, iconType.maxSpawnProbability, colorWeight);

                if (Random.value <= spawnProbability)
                {
                    SpawnPrefabAtNpcIndex(randomIndex, iconType);
                    break; // Only spawn one icon per attempt
                }
            }
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

                float blueNormalized = (neighborNpc.stance + 128f) / 255f;
                float colorValue = settings.isBlue ? blueNormalized : (1f - blueNormalized);

                if (colorValue >= settings.minColorThreshold && colorValue <= settings.maxColorThreshold)
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
