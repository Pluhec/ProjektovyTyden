Shader "Hidden/SimulationRenderer"
{
    Properties {

    }
    SubShader {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" "RenderPipeline"="UniversalPipeline" }
        LOD 100
        
        Pass {
            Name "ForwardLit"
            Tags { "LightMode"="UniversalForward" }
            
            ZWrite On
            ZTest LEqual
            Blend SrcAlpha OneMinusSrcAlpha
            Cull Off

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma target 4.5
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Npc {
                uint population;         // 0 - 255
                uint age;
                uint education;
                uint impressionability;
                int stance;
                uint friendIndex;                  // friend index
                uint neighbors;           // up to 8 neighbors
            };

            ByteAddressBuffer npcs;
            int _Count;
            int _GridWidth;
            int _GridHeight;
            float _CellSizeX;
            float _CellSizeY;
            float _PosOffsetX;
            float _PosOffsetY;
            
            // Load NPC from byte buffer
            Npc LoadNpc(uint index) {
                uint address = index * 12;
                uint3 data = npcs.Load3(address);
                Npc npc;
                npc.population = (data.x >> 0) & 0xFF;
                npc.age = (data.x >> 8) & 0xFF;
                npc.education = (data.x >> 16) & 0xFF;
                npc.impressionability = (data.x >> 24) & 0xFF;
                npc.stance = int((data.y >> 0) & 0xFF) - 128;
                npc.friendIndex = (data.y >> 8);
                npc.friendIndex |= (data.z & 0xFF) << 24;
                npc.neighbors = (data.z >> 8) & 0xFF;
                return npc;
            }

            // Compute color for a single cell
            float4 ComputeCellColor(int gx, int gy)
            {
                if (gx < 0 || gy < 0 || gx >= _GridWidth || gy >= _GridHeight)
                    return float4(0, 0, 0, 0); // out-of-bounds -> zero weight
                uint idx = (uint)gy * (uint)_GridWidth + (uint)gx;
                if (idx >= (uint)_Count)
                    return float4(0, 0, 0, 0);
                Npc npc = LoadNpc(idx);
                if (npc.population == 0u)
                    return float4(0, 0, 0, 0);
                float normalizedValue = (npc.stance + 128.0) / 255.0;
                float3 redCol = float3(1.0, 0.0, 0.0);
                float3 grayCol = float3(0.5, 0.5, 0.5);
                float3 blueCol = float3(0.0, 0.0, 1.0);
                float3 col = lerp(redCol, lerp(grayCol, blueCol, saturate(normalizedValue*2-1)), saturate(normalizedValue*2));
                return float4(col, 1.0);
            }

            // Average color at a shared grid vertex from up to 4 adjacent cells
            // cx, cy: 0 or 1 indicating which corner of the current cell
            float4 SmoothCornerColor(int gridX, int gridY, int cx, int cy)
            {
                float4 total = float4(0, 0, 0, 0);
                float weight = 0;
                // The 4 cells sharing this corner vertex
                for (int dy = -1; dy <= 0; dy++) {
                    for (int dx = -1; dx <= 0; dx++) {
                        float4 c = ComputeCellColor(gridX + cx + dx, gridY + cy + dy);
                        if (c.w > 0.5) {
                            total.rgb += c.rgb;
                            weight += 1.0;
                        }
                    }
                }
                if (weight > 0)
                    return float4(total.rgb / weight, 1.0);
                return float4(0.5, 0.5, 0.5, 0.0);
            }

            struct Attributes {
                uint vertexID : SV_VertexID;
            };

            struct Varyings {
                float4 positionCS : SV_Position;
                float2 uv : TEXCOORD0;
                float4 color : COLOR0;
            };

            Varyings Vert (Attributes i)
            {
                Varyings o;
                uint npcIdx = i.vertexID / 6;  // 6 vertices per quad (2 triangles)
                uint vertIdx = i.vertexID % 6;
                
                if (npcIdx >= (uint)_Count) {
                    o.positionCS = float4(-9999,-9999,0,1);
                    o.uv = float2(0,0);
                    o.color = 0;
                    return o;
                }
                
                // Calculate grid position
                int gridX = npcIdx % _GridWidth;
                int gridY = npcIdx / _GridWidth;
                float2 gridPos = float2(gridX, gridY) * float2(_CellSizeX, _CellSizeY);
                
                // Center the grid
                float2 gridCenter = float2(_GridWidth, _GridHeight) * float2(_CellSizeX, _CellSizeY) * 0.5;
                gridPos -= gridCenter;
                gridPos += float2(_PosOffsetX, _PosOffsetY);
                
                // Create quad as 2 triangles: 0-1-2, 0-2-3 -> 0,1,2, 0,2,3
                float2 corners[4] = {
                    float2(0, 0),           // 0: bottom-left
                    float2(_CellSizeX, 0),   // 1: bottom-right
                    float2(_CellSizeX, _CellSizeY), // 2: top-right
                    float2(0, _CellSizeY)    // 3: top-left
                };
                
                // Map vertex index to corner: 0,1,2,0,2,3
                uint cornerIndices[6] = {0, 1, 2, 0, 2, 3};
                uint cornerIdx = cornerIndices[vertIdx];
                
                float3 worldPos = float3(gridPos + corners[cornerIdx], 0);
                o.positionCS = TransformWorldToHClip(worldPos);
                o.uv = float2(cornerIdx % 2, cornerIdx / 2);

                // Smooth color: average stance from the 4 cells sharing this corner vertex
                // Corner 0 (BL): cx=0, cy=0 | Corner 1 (BR): cx=1, cy=0
                // Corner 2 (TR): cx=1, cy=1 | Corner 3 (TL): cx=0, cy=1
                int cx = (cornerIdx == 1 || cornerIdx == 2) ? 1 : 0;
                int cy = (cornerIdx >= 2) ? 1 : 0;
                o.color = SmoothCornerColor(gridX, gridY, cx, cy);

                return o;
            }

            half4 Frag (Varyings i) : SV_Target {
                return half4(i.color);
            }
            ENDHLSL
        }
    }
    Fallback Off
}
