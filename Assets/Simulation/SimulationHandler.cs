using UnityEngine;
using UnityEngine.Rendering;
using System.Runtime.InteropServices;
using UnityEngine.InputSystem;
using System.Threading.Tasks;
using System.Collections.Generic;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct NPC // 12b (9b data + 3b padding)
{
    public byte population;         // 0 - 255              1b
    public byte age;                // 0 - 255              1b
    public byte education;          // 0 - 255              1b
    public byte impressionability;  // 0 - 255              1b
    public sbyte stance;            // -128 - 127           1b
    public uint friendIndex;        // 0 - 4,294,967,295    4b (friend index)
    public byte neighbors;          // 0 - 255              1b (up to 8 neighbors)
    private byte _padding0;         // padding              1b
    private byte _padding1;         // padding              1b
}

public class SimulationHandler : MonoBehaviour
{
    public Vector2Int gridSize = new Vector2Int(32, 32);

    private int numNPCs;
    public Material gridMaterial;
    public Transform simulationQuadTransform;
    private MaterialPropertyBlock propertyBlock;
    public ComputeShader cs;
    public ComputeBuffer npcBuffer;
    private int kernel_init;
    private int kernel_step;
    private NPC[] npcs;

    public Texture2D initialTexture;
    public Texture2D regionTexture;

    private const int NUM_REGIONS = 10;
    private int[] regionMap;
    public float[] regionAverages;

    void Start()
    {
        numNPCs = gridSize.x * gridSize.y;
        propertyBlock = new MaterialPropertyBlock();
        kernel_init = cs.FindKernel("SimulationInit");
        kernel_step = cs.FindKernel("SimulationStep");
        npcs = new NPC[numNPCs];
        npcBuffer = new ComputeBuffer(numNPCs, 12, ComputeBufferType.Raw);
        npcBuffer.SetData(npcs);

        // init thesimulation
        cs.SetBuffer(kernel_init, "npcs", npcBuffer);
        cs.SetTexture(kernel_init, "simumlationInitialTexture", initialTexture);
        cs.SetInts("populationSize", gridSize.x, gridSize.y);
        cs.SetInts("imageSize", initialTexture.width, initialTexture.height);
        cs.SetInt("seedValue", Random.Range(0, int.MaxValue));
        cs.Dispatch(kernel_init, (numNPCs + 63) / 64, 1, 1);
        npcBuffer.GetData(npcs);
    }

    void Update()
    {
        if (Keyboard.current != null &&
            (Keyboard.current.spaceKey.isPressed || Keyboard.current.rightArrowKey.wasPressedThisFrame))
        {
            NextDay();
        }

        if (Keyboard.current != null && Keyboard.current.hKey.wasPressedThisFrame)
        {
            RegionAverage();
        }

        RenderSimulation();
    }

    void NextDay(){
        SimulationStep();
    }

    void SimulationStep(){
        cs.SetBuffer(kernel_step, "npcs", npcBuffer);
        cs.SetInt("_NumNPCs", numNPCs);
        cs.Dispatch(kernel_step, (numNPCs + 63) / 64, 1, 1);
        npcBuffer.GetData(npcs);
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
        // Quantize to avoid anti-aliasing artifacts (round to nearest 8)
        Dictionary<int, int> colorToRegion = new Dictionary<int, int>();
        int nextRegion = 0;

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
            // Quantize red channel to reduce noise from compression/anti-aliasing
            int key = (c.r + 4) / 8;

            if (!colorToRegion.TryGetValue(key, out int region))
            {
                region = nextRegion;
                colorToRegion[key] = region;
                nextRegion++;
            }
            regionMap[i] = Mathf.Clamp(region, 0, NUM_REGIONS - 1);
        }

        Debug.Log($"[RegionMap] Found {colorToRegion.Count} unique regions (quantized)");
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
        float[][] localSums = new float[threadCount][];
        int[][] localCounts = new int[threadCount][];
        for (int t = 0; t < threadCount; t++)
        {
            localSums[t] = new float[NUM_REGIONS];
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
            float[] sums = localSums[t];
            int[] counts = localCounts[t];

            for (int i = start; i < end; i++)
            {
                if (npcArray[i].population == 0) continue; // skip empty cells
                int region = regionMapLocal[i];
                sums[region] += npcArray[i].population;
                counts[region]++;
            }
        });

        // Pass 3: Merge thread-local results into final averages
        if (regionAverages == null || regionAverages.Length != NUM_REGIONS)
            regionAverages = new float[NUM_REGIONS];

        for (int r = 0; r < NUM_REGIONS; r++)
        {
            float totalSum = 0f;
            int totalCount = 0;
            for (int t = 0; t < threadCount; t++)
            {
                totalSum += localSums[t][r];
                totalCount += localCounts[t][r];
            }
            regionAverages[r] = totalCount > 0 ? totalSum / totalCount : 0f;
        }
    }

    void OnDestroy()
    {
        if (npcBuffer != null)
            npcBuffer.Release();
    }
}
