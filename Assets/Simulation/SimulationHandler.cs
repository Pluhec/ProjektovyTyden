using UnityEngine;
using UnityEngine.Rendering;
using System.Runtime.InteropServices;
using UnityEngine.InputSystem;
using System.Threading.Tasks;
using System.Collections.Generic;
using PlayerChoice.DataSets;
using TMPro;
using UnityEngine.UI;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct NPC // 12b (9b data + 3b padding)
{
    public byte population;         // 0 - 255              1b
    public byte age;                // 0 - 255              1b
    public byte education;          // 0 - 255              1b
    public byte impressionability;  // 0 - 255              1b
    public byte stance;            // -128 - 127           1b
    public uint friendIndex;        // 0 - 4,294,967,295    4b (friend index)
    public byte neighbors;          // 0 - 255              1b (up to 8 neighbors)
    private byte _padding0;         // padding              1b
    private byte _padding1;         // padding              1b
}

[System.Serializable]
public struct AgeBandPropaganda
{
    [Min(0f)] public float virality;
    [Min(0f)] public float impact;
    [Min(0f)] public float visibility;
}

[System.Serializable]
public struct PropagandaLevels
{
    public AgeBandPropaganda young;
    public AgeBandPropaganda adults;
    public AgeBandPropaganda seniors;
    [Range(-1f, 1f)] public float narrativeDirection;
    [Range(0, 255)] public int youngMaxAge;
    [Range(0, 255)] public int adultMaxAge;
}

public class SimulationHandler : MonoBehaviour
{
    public Vector2Int gridSize = new Vector2Int(32, 32);

    private int numNPCs;
    public Material gridMaterial;
    public Transform simulationQuadTransform;
    [Header("Render Settings")]
    private MaterialPropertyBlock propertyBlock;
    public ComputeShader cs;
    public ComputeBuffer npcBuffer;
    private int kernel_init;
    private int kernel_step;
    private NPC[] npcs;

    public Texture2D initialTexture;
    public Texture2D regionTexture;

    [Header("Visual Style")]
    [Range(2, 64)] public float posterizeLevels = 16f;
    [Range(0f, 10f)] public float ditherStrength = 0.3f;
    [Range(0, 5)] public int blurRadius = 1;
    [Range(0.01f, 4f)] public float alphaGamma = 1f;

    [Header("Propaganda")]
    public PropagandaLevels propaganda = new PropagandaLevels
    {
        young = new AgeBandPropaganda { virality = 1f, impact = 1f, visibility = 1f },
        adults = new AgeBandPropaganda { virality = 1f, impact = 1f, visibility = 1f },
        seniors = new AgeBandPropaganda { virality = 1f, impact = 1f, visibility = 1f },
        narrativeDirection = 1f,
        youngMaxAge = 85,
        adultMaxAge = 170
    };
    [Range(0.01f, 1f)] public float propagandaLevelAdaptationPerDay = 0.2f;

    public int paintingBrushRadius = 8;
    private PropagandaLevels runtimePropaganda;

    [HideInInspector] public uint simulationTime;

    [Header("Global Stats UI")]
    public TextMeshProUGUI globalStanceText;
    public Slider globalStanceSlider;
    public TextMeshProUGUI globalPopulationText;
    public TextMeshProUGUI globalAgeText;
    public TextMeshProUGUI globalEducationText;

    private const int NUM_REGIONS = 10;
    private int[] regionMap;
    public float[] regionAverages;
    public int[] regionPopulations;
    public float[] regionAgeAverages;
    public float[] regionEducationAverages;

    void Start()
    {
        numNPCs = gridSize.x * gridSize.y;
        PlayerChoice.DataSets.DataFunctions.SetSimulationHandler(this);
        simulationTime = 0;
        propertyBlock = new MaterialPropertyBlock();
        kernel_init = cs.FindKernel("SimulationInit");
        kernel_step = cs.FindKernel("SimulationStep");
        npcs = new NPC[numNPCs];
        npcBuffer = new ComputeBuffer(numNPCs, 12, ComputeBufferType.Raw);
        npcBuffer.SetData(npcs);

        // Initialize global stance slider range
        if (globalStanceSlider != null)
        {
            globalStanceSlider.minValue = 0f;
            globalStanceSlider.maxValue = 100f;
        }

        // init thesimulation
        cs.SetBuffer(kernel_init, "npcs", npcBuffer);
        cs.SetTexture(kernel_init, "simumlationInitialTexture", initialTexture);
        cs.SetInts("populationSize", gridSize.x, gridSize.y);
        cs.SetInts("imageSize", initialTexture.width, initialTexture.height);
        cs.SetInt("_NumNPCs", numNPCs);
        cs.SetInt("seedValue", Random.Range(0, int.MaxValue));
        runtimePropaganda = propaganda;
        ApplyPropagandaToShader();
        cs.Dispatch(kernel_init, (numNPCs + 63) / 64, 1, 1);
        npcBuffer.GetData(npcs);
        for (int i = 0; i < numNPCs; i++)
            npcs[i].friendIndex = (uint)Random.Range(0, numNPCs);
        npcBuffer.SetData(npcs);

        RegionAverage(); // compute initial averages for tooltip
    }

    void Update()
    {
        // if (Mouse.current != null && Mouse.current.leftButton.isPressed)
        // {
        //     Vector2 mousePos = Mouse.current.position.ReadValue();
        //     if (TryGetGridPositionFromMouse(mousePos, out int gridX, out int gridY))
        //         PaintStance(gridX, gridY, -0.1f);
        // }
        // if (Keyboard.current != null && (Keyboard.current.spaceKey.isPressed || Keyboard.current.rightArrowKey.wasPressedThisFrame))
        //     Tick();
        // if (Keyboard.current != null && Keyboard.current.hKey.wasPressedThisFrame)
        //     RegionAverage();

        RenderSimulation();
    }

    public bool TryGetGridPositionFromMouse(Vector2 mousePosition, out int gridX, out int gridY)
    {
        gridX = -1;
        gridY = -1;

        Camera targetCamera = Camera.main;
        if (targetCamera == null || simulationQuadTransform == null)
            return false;

        Ray ray = targetCamera.ScreenPointToRay(mousePosition);
        Plane quadPlane = new Plane(simulationQuadTransform.forward, simulationQuadTransform.position);
        if (!quadPlane.Raycast(ray, out float enterDistance))
            return false;

        Vector3 hitWorld = ray.GetPoint(enterDistance);
        return TryGetGridPositionFromWorld(hitWorld, out gridX, out gridY);
    }

    public bool TryGetGridPositionFromWorld(Vector3 worldPosition, out int gridX, out int gridY)
    {
        gridX = -1;
        gridY = -1;

        if (simulationQuadTransform == null)
            return false;

        Vector3 localPos = simulationQuadTransform.InverseTransformPoint(worldPosition);

        float u = localPos.x + 0.5f;
        float v = localPos.y + 0.5f;
        if (u < 0f || u >= 1f || v < 0f || v >= 1f)
            return false;

        gridX = Mathf.Clamp(Mathf.FloorToInt(u * gridSize.x), 0, gridSize.x - 1);
        gridY = Mathf.Clamp(Mathf.FloorToInt(v * gridSize.y), 0, gridSize.y - 1);
        return true;
    }

    public void Tick(){
        SimulationStep();
        RegionAverage();
        UpdateGlobalStatsUI();
    }

    void UpdateGlobalStatsUI()
    {
        float stanceDisplay = GetGlobalStanceAverage() / 255f * 200f - 100f;
        if (globalStanceText != null)
            globalStanceText.text = $"{stanceDisplay:+0.0;-0.0;0.0}";

        // Sync slider (0..100) with displayed stance (-100..100)
        if (globalStanceSlider != null)
        {
            // Map -100..100 -> 0..100
            globalStanceSlider.value = stanceDisplay;
        }
        if (globalPopulationText != null)
            globalPopulationText.text = $"{GetGlobalPopulationAverage():0.0}";
        if (globalAgeText != null)
            globalAgeText.text = $"{GetGlobalAgeAverage():0.0}";
        if (globalEducationText != null)
            globalEducationText.text = $"{GetGlobalEducationAverage():0.0}";
    }

    void SimulationStep(){
        UpdateRuntimePropagandaLevels();
        cs.SetBuffer(kernel_step, "npcs", npcBuffer);
        cs.SetInt("_NumNPCs", numNPCs);
        cs.SetInt("_simulationTime", (int)simulationTime);
        ApplyPropagandaToShader();
        cs.Dispatch(kernel_step, (numNPCs + 63) / 64, 1, 1);
        npcBuffer.GetData(npcs);
    }

    void UpdateRuntimePropagandaLevels()
    {
        float t = Mathf.Clamp01(propagandaLevelAdaptationPerDay);

        runtimePropaganda.young.virality = Mathf.Lerp(runtimePropaganda.young.virality, Mathf.Max(0f, propaganda.young.virality), t);
        runtimePropaganda.young.impact = Mathf.Lerp(runtimePropaganda.young.impact, Mathf.Max(0f, propaganda.young.impact), t);
        runtimePropaganda.young.visibility = Mathf.Lerp(runtimePropaganda.young.visibility, Mathf.Max(0f, propaganda.young.visibility), t);

        runtimePropaganda.adults.virality = Mathf.Lerp(runtimePropaganda.adults.virality, Mathf.Max(0f, propaganda.adults.virality), t);
        runtimePropaganda.adults.impact = Mathf.Lerp(runtimePropaganda.adults.impact, Mathf.Max(0f, propaganda.adults.impact), t);
        runtimePropaganda.adults.visibility = Mathf.Lerp(runtimePropaganda.adults.visibility, Mathf.Max(0f, propaganda.adults.visibility), t);

        runtimePropaganda.seniors.virality = Mathf.Lerp(runtimePropaganda.seniors.virality, Mathf.Max(0f, propaganda.seniors.virality), t);
        runtimePropaganda.seniors.impact = Mathf.Lerp(runtimePropaganda.seniors.impact, Mathf.Max(0f, propaganda.seniors.impact), t);
        runtimePropaganda.seniors.visibility = Mathf.Lerp(runtimePropaganda.seniors.visibility, Mathf.Max(0f, propaganda.seniors.visibility), t);

        runtimePropaganda.narrativeDirection = Mathf.Lerp(runtimePropaganda.narrativeDirection, Mathf.Clamp(propaganda.narrativeDirection, -1f, 1f), t);
        runtimePropaganda.youngMaxAge = Mathf.Clamp(propaganda.youngMaxAge, 0, 255);
        runtimePropaganda.adultMaxAge = Mathf.Clamp(propaganda.adultMaxAge, runtimePropaganda.youngMaxAge, 255);
    }

    void ApplyPropagandaToShader()
    {
        int youngMaxAge = Mathf.Clamp(runtimePropaganda.youngMaxAge, 0, 255);
        int adultMaxAge = Mathf.Clamp(runtimePropaganda.adultMaxAge, youngMaxAge, 255);

        cs.SetFloat("_ViralityYoung", runtimePropaganda.young.virality);
        cs.SetFloat("_ImpactYoung", runtimePropaganda.young.impact);
        cs.SetFloat("_VisibilityYoung", runtimePropaganda.young.visibility);

        cs.SetFloat("_ViralityAdults", runtimePropaganda.adults.virality);
        cs.SetFloat("_ImpactAdults", runtimePropaganda.adults.impact);
        cs.SetFloat("_VisibilityAdults", runtimePropaganda.adults.visibility);

        cs.SetFloat("_ViralitySeniors", runtimePropaganda.seniors.virality);
        cs.SetFloat("_ImpactSeniors", runtimePropaganda.seniors.impact);
        cs.SetFloat("_VisibilitySeniors", runtimePropaganda.seniors.visibility);

        cs.SetFloat("_PropagandaDirection", runtimePropaganda.narrativeDirection);
        cs.SetInt("_YoungMaxAge", youngMaxAge);
        cs.SetInt("_AdultMaxAge", adultMaxAge);
    }
    void RenderSimulation(){
        propertyBlock.SetBuffer("npcs", npcBuffer);
        propertyBlock.SetInt("_Count", numNPCs);
        propertyBlock.SetInt("_GridWidth", gridSize.x);
        propertyBlock.SetInt("_GridHeight", gridSize.y);
        propertyBlock.SetFloat("_CellSizeX", 1f / gridSize.x * simulationQuadTransform.localScale.x);
        propertyBlock.SetFloat("_CellSizeY", 1f / gridSize.y * simulationQuadTransform.localScale.y);
        propertyBlock.SetFloat("_PosOffsetX", simulationQuadTransform.position.x);
        propertyBlock.SetFloat("_PosOffsetY", simulationQuadTransform.position.y);
        gridMaterial.SetFloat("_PosterizeLevels", Mathf.Max(posterizeLevels, 2f));
        gridMaterial.SetInt("_BlurRadius", blurRadius);
        gridMaterial.SetFloat("_AlphaGamma", alphaGamma);
        Graphics.DrawProcedural(
            gridMaterial,
            new Bounds(Vector3.zero, Vector3.one * 1000),
            MeshTopology.Triangles,
            numNPCs * 6,                // 6 vertices (1 quad = 2 triangles) per NPC
            1,
            null,
            propertyBlock,
            ShadowCastingMode.Off,
            false,
            gameObject.layer
        );
    }

    // Pass 1: Build region map from texture (one-time, cached)
    // Maps each grid cell to a region index based on unique colors in regionTexture
    void ComputeRegionMap()
    {
        regionMap = new int[numNPCs];
        // Copy texture to a readable Texture2D via RenderTexture (works even if Read/Write is off)
        RenderTexture rt = RenderTexture.GetTemporary(regionTexture.width, regionTexture.height, 0, RenderTextureFormat.ARGB32);
        Graphics.Blit(regionTexture, rt);
        RenderTexture prev = RenderTexture.active;
        RenderTexture.active = rt;
        Texture2D readableTex = new Texture2D(regionTexture.width, regionTexture.height, TextureFormat.RGBA32, false);
        readableTex.ReadPixels(new Rect(0, 0, regionTexture.width, regionTexture.height), 0, 0);
        readableTex.Apply();
        RenderTexture.active = prev;
        RenderTexture.ReleaseTemporary(rt);

        Color32[] pixels = readableTex.GetPixels32();
        int texW = readableTex.width;
        int texH = readableTex.height;

        // Discover unique colors -> region indices
        // Use same formula as RaycastRegion in RegionMapHover: floor(r / 256 * 10)
        for (int i = 0; i < numNPCs; i++)
        {
            int gridX = i % gridSize.x;
            int gridY = i / gridSize.x;

            // Map grid cell center to texture pixel
            float u = (gridX + 0.5f) / gridSize.x;
            float v = (gridY + 0.5f) / gridSize.y;

            int texX = Mathf.Clamp((int)(u * texW), 0, texW - 1);
            int texY = Mathf.Clamp((int)(v * texH), 0, texH - 1);

            Color32 c = pixels[texY * texW + texX];
            int region = Mathf.FloorToInt(c.r / 256f * 10f);
            regionMap[i] = Mathf.Clamp(region, 0, NUM_REGIONS - 1);
        }

        Debug.Log($"[RegionMap] Region map computed (10 regions from red channel)");
    }

    // Pass 2 & 3: Parallel accumulation + merge
    // Computes average population per region using thread-local accumulators
    void RegionAverage()
    {
        if (regionMap == null)
            ComputeRegionMap();

        // Ensure npcs array is up-to-date from GPU
        npcBuffer.GetData(npcs);

        int threadCount = System.Environment.ProcessorCount;
        int chunkSize = (numNPCs + threadCount - 1) / threadCount;

        // Thread-local accumulators to avoid synchronization
        float[][] localStanceSums = new float[threadCount][];
        float[][] localAgeSums = new float[threadCount][];
        float[][] localEduSums = new float[threadCount][];
        int[][] localPopSums = new int[threadCount][];
        int[][] localCounts = new int[threadCount][];
        for (int t = 0; t < threadCount; t++)
        {
            localStanceSums[t] = new float[NUM_REGIONS];
            localAgeSums[t] = new float[NUM_REGIONS];
            localEduSums[t] = new float[NUM_REGIONS];
            localPopSums[t] = new int[NUM_REGIONS];
            localCounts[t] = new int[NUM_REGIONS];
        }

        // Capture locals for thread safety
        int npcCount = numNPCs;
        NPC[] npcArray = npcs;
        int[] regionMapLocal = regionMap;

        // Pass 2: Parallel accumulation across partitioned chunks
        Parallel.For(0, threadCount, t =>
        {
            int start = t * chunkSize;
            int end = System.Math.Min(start + chunkSize, npcCount);
            float[] stSums = localStanceSums[t];
            float[] agSums = localAgeSums[t];
            float[] edSums = localEduSums[t];
            int[] popSums = localPopSums[t];
            int[] counts = localCounts[t];

            for (int i = start; i < end; i++)
            {
                int pop = npcArray[i].population;
                if (pop == 0) continue;
                int region = regionMapLocal[i];
                stSums[region] += npcArray[i].stance * pop;     // population-weighted
                agSums[region] += npcArray[i].age * pop;        // population-weighted
                edSums[region] += npcArray[i].education * pop;  // population-weighted
                popSums[region] += pop;                         // total population weight
                counts[region]++;                               // number of cells in region
            }
        });

        // Pass 3: Merge thread-local results into final averages
        if (regionAverages == null || regionAverages.Length != NUM_REGIONS)
        {
            regionAverages = new float[NUM_REGIONS];
            regionPopulations = new int[NUM_REGIONS];
            regionAgeAverages = new float[NUM_REGIONS];
            regionEducationAverages = new float[NUM_REGIONS];
        }

        for (int r = 0; r < NUM_REGIONS; r++)
        {
            float stanceSum = 0f, ageSum = 0f, eduSum = 0f;
            int popSum = 0, count = 0;
            for (int t = 0; t < threadCount; t++)
            {
                stanceSum += localStanceSums[t][r];
                ageSum += localAgeSums[t][r];
                eduSum += localEduSums[t][r];
                popSum += localPopSums[t][r];
                count += localCounts[t][r];
            }
            regionAverages[r] = popSum > 0 ? stanceSum / popSum : 0f;
            regionPopulations[r] = count > 0 ? popSum / count : 0;    // average population density per cell
            regionAgeAverages[r] = popSum > 0 ? (ageSum / popSum) / 255f * 75f + 15f : 0f;      // remap 0-255 -> 15-90
            regionEducationAverages[r] = popSum > 0 ? (eduSum / popSum) / 255f * 100f : 0f;      // remap 0-255 -> 0-100
        }
    }

    public float GetRegionAverage(int regionIndex)
    {
        if (regionAverages == null || regionIndex < 0 || regionIndex >= regionAverages.Length)
            return 0f;
        return regionAverages[regionIndex];
    }

    public int GetRegionPopulation(int regionIndex)
    {
        if (regionPopulations == null || regionIndex < 0 || regionIndex >= regionPopulations.Length)
            return 0;
        return regionPopulations[regionIndex];
    }

    public float GetRegionAgeAverage(int regionIndex)
    {
        if (regionAgeAverages == null || regionIndex < 0 || regionIndex >= regionAgeAverages.Length)
            return 0f;
        return regionAgeAverages[regionIndex];
    }

    public float GetRegionEducationAverage(int regionIndex)
    {
        if (regionEducationAverages == null || regionIndex < 0 || regionIndex >= regionEducationAverages.Length)
            return 0f;
        return regionEducationAverages[regionIndex];
    }

    public float GetGlobalStanceAverage()
    {
        if (npcs == null) return 0f;
        float stanceSum = 0f;
        int popSum = 0;
        for (int i = 0; i < numNPCs; i++)
        {
            int pop = npcs[i].population;
            if (pop == 0) continue;
            stanceSum += npcs[i].stance * pop;
            popSum += pop;
        }
        return popSum > 0 ? stanceSum / popSum : 0f;
    }

    public float GetGlobalPopulationAverage()
    {
        if (npcs == null) return 0f;
        int count = 0;
        int popSum = 0;
        for (int i = 0; i < numNPCs; i++)
        {
            if (npcs[i].population == 0) continue;
            popSum += npcs[i].population;
            count++;
        }
        return count > 0 ? (float)popSum / count : 0f;
    }

    public float GetGlobalAgeAverage()
    {
        if (npcs == null) return 0f;
        float ageSum = 0f;
        int popSum = 0;
        for (int i = 0; i < numNPCs; i++)
        {
            int pop = npcs[i].population;
            if (pop == 0) continue;
            ageSum += npcs[i].age * pop;
            popSum += pop;
        }
        return popSum > 0 ? (ageSum / popSum) / 255f * 75f + 15f : 0f; // remap 0-255 -> 15-90
    }

    public float GetGlobalEducationAverage()
    {
        if (npcs == null) return 0f;
        float eduSum = 0f;
        int popSum = 0;
        for (int i = 0; i < numNPCs; i++)
        {
            int pop = npcs[i].population;
            if (pop == 0) continue;
            eduSum += npcs[i].education * pop;
            popSum += pop;
        }
        return popSum > 0 ? (eduSum / popSum) / 255f * 100f : 0f; // remap 0-255 -> 0-100
    }

    public void PaintStance(int gridX, int gridY, float stanceOffset, int customRadius = -1, float cellAspectXY = 1f)
    {
        int radius = customRadius >= 0 ? customRadius : Mathf.Max(1, paintingBrushRadius);
        float radiusSq = radius * radius;
        float stanceDeltaByte = stanceOffset * 127f;
        // Account for non-square grid cells so the influence region is a world-space circle.
        // cellAspectXY = cellWidth / cellHeight. x coords are scaled into y-cell units before
        // distance is measured, and the x iteration range is adjusted accordingly.
        float safeAspect = Mathf.Max(0.0001f, cellAspectXY);
        int xExtent = Mathf.CeilToInt(radius / safeAspect);

        for (int y = -radius; y <= radius; y++)
        {
            for (int x = -xExtent; x <= xExtent; x++)
            {
                float nx = x * safeAspect; // convert x to y-cell units
                float distSq = nx * nx + y * y;
                if (distSq > radiusSq) continue;

                float t = 1f - (distSq / radiusSq);
                float weight = t * t * (3f - 2f * t);

                int px = gridX + x;
                int py = gridY + y;
                if (px >= 0 && px < gridSize.x && py >= 0 && py < gridSize.y)
                {
                    int index = py * gridSize.x + px;
                    if (index >= 0 && index < numNPCs)
                    {
                        float current = npcs[index].stance;
                        float additive = stanceDeltaByte * weight;
                        npcs[index].stance = (byte)Mathf.Clamp(Mathf.RoundToInt(current + additive), 0, 255);
                    }
                }
            }
        }
        npcBuffer.SetData(npcs);
    }

    void OnDestroy()
    {
        if (npcBuffer != null)
            npcBuffer.Release();
    }

    // Applies S_StatData array (Young, Adult, Senior) as additive offsets to propaganda levels.
    // sbyte values (-128..127) are normalized to (-1..1) before being added.
    public void ApplyStatData(EnumStructs.S_StatData[] statData)
    {
        foreach (EnumStructs.S_StatData stat in statData)
        {
            float virality = stat.Virality / 127f;
            float impact = stat.Impact / 127f;
            float visibility = stat.Visibility / 127f;

            switch (stat.AgeGroup)
            {
                case EnumStructs.E_Age.Young:
                    propaganda.young.virality = Mathf.Max(0f, propaganda.young.virality   + virality);
                    propaganda.young.impact = Mathf.Max(0f, propaganda.young.impact     + impact);
                    propaganda.young.visibility = Mathf.Max(0f, propaganda.young.visibility + visibility);
                    break;
                case EnumStructs.E_Age.Adult:
                    propaganda.adults.virality = Mathf.Max(0f, propaganda.adults.virality   + virality);
                    propaganda.adults.impact = Mathf.Max(0f, propaganda.adults.impact     + impact);
                    propaganda.adults.visibility = Mathf.Max(0f, propaganda.adults.visibility + visibility);
                    break;
                case EnumStructs.E_Age.Senior:
                    propaganda.seniors.virality = Mathf.Max(0f, propaganda.seniors.virality   + virality);
                    propaganda.seniors.impact = Mathf.Max(0f, propaganda.seniors.impact     + impact);
                    propaganda.seniors.visibility = Mathf.Max(0f, propaganda.seniors.visibility + visibility);
                    break;
            }
        }
    }
}
