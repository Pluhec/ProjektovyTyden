using UnityEngine;
using UnityEngine.Rendering;
using System.Runtime.InteropServices;
using UnityEngine.InputSystem;

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

    void RegionAverage()
    {
        // calculate positions of the NPCs (grid) to match texture (regionTexture)
        // calculate average for each region (out of 10)
        // every region in the texture has different shade of gray (as index)
        // last, for optimization, split the average to multiple passes (maybe parallel for? if possible for averaging)
    }

    void OnDestroy()
    {
        if (npcBuffer != null)
            npcBuffer.Release();
    }
}
