using UnityEngine;

public class SpawningPointsByMap : MonoBehaviour
{
    [Header("References")]
    public SimulationHandler simulationHandler;
    public GameObject prefabToSpawn;

    [Header("Spawning Settings")]
    public float spawnInterval = 1f;
    public int spawnAttemptsPerInterval = 10;
    [Range(0f, 1f)]
    public float minSpawnProbability = 0.0f;
    [Range(0f, 1f)]
    public float maxSpawnProbability = 1.0f;

    private float spawnTimer;
    private NPC[] localNpcsArray;

    void Start()
    {
        spawnTimer = spawnInterval;
    }

    void Update()
    {
        if (simulationHandler == null || simulationHandler.npcBuffer == null || prefabToSpawn == null)
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

            // Calculate spawn probability based on population (0-255)
            // "red" means high population, "dark" means low/zero population
            float populationNormalized = randomNpc.population / 255f;
            
            // Map the normalized population to our probability range
            float spawnProbability = Mathf.Lerp(minSpawnProbability, maxSpawnProbability, populationNormalized);

            // If population is 0, we don't spawn (less to no where it is dark)
            if (randomNpc.population == 0)
            {
                spawnProbability = 0f;
            }

            if (Random.value <= spawnProbability)
            {
                SpawnPrefabAtNpcIndex(randomIndex);
            }
        }
    }

    private void SpawnPrefabAtNpcIndex(int npcIndex)
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
        float randomOffsetX = Random.Range(-cellSizeX * 0.4f, cellSizeX * 0.4f);
        float randomOffsetY = Random.Range(-cellSizeY * 0.4f, cellSizeY * 0.4f);
        spawnPosition += new Vector3(randomOffsetX, randomOffsetY, 0f);

        Instantiate(prefabToSpawn, spawnPosition, Quaternion.identity, transform);
    }
}
