Shader "Hidden/SimulationRenderer"
{
    Properties {
        _PosterizeLevels ("Posterize Levels", Range(2, 64)) = 16
        _DitherStrength ("Dither Strength", Range(0, 10)) = 0.3
        _BlurRadius ("Blur Radius", Range(0, 5)) = 1
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
            float _PosterizeLevels;
            float _DitherStrength;
            int _BlurRadius;
            
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

            // Average color from cells within a configurable radius around a corner vertex
            float4 SmoothCornerColor(int gridX, int gridY, int cx, int cy)
            {
                float4 total = float4(0, 0, 0, 0);
                float weight = 0;
                int r = max(_BlurRadius, 0);
                for (int dy = -r; dy <= 0; dy++) {
                    for (int dx = -r; dx <= 0; dx++) {
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
                float4 smoothCol = SmoothCornerColor(gridX, gridY, cx, cy);
                float levels = _PosterizeLevels;
                smoothCol.rgb = floor(smoothCol.rgb * levels + 0.5) / levels;
                o.color = smoothCol;

                return o;
            }

            // 8x8 Bayer dither threshold matrix (normalized 0..1)
            static const float bayerMatrix[64] = {
                 0.0/64.0, 48.0/64.0, 12.0/64.0, 60.0/64.0,  3.0/64.0, 51.0/64.0, 15.0/64.0, 63.0/64.0,
                32.0/64.0, 16.0/64.0, 44.0/64.0, 28.0/64.0, 35.0/64.0, 19.0/64.0, 47.0/64.0, 31.0/64.0,
                 8.0/64.0, 56.0/64.0,  4.0/64.0, 52.0/64.0, 11.0/64.0, 59.0/64.0,  7.0/64.0, 55.0/64.0,
                40.0/64.0, 24.0/64.0, 36.0/64.0, 20.0/64.0, 43.0/64.0, 27.0/64.0, 39.0/64.0, 23.0/64.0,
                 2.0/64.0, 50.0/64.0, 14.0/64.0, 62.0/64.0,  1.0/64.0, 49.0/64.0, 13.0/64.0, 61.0/64.0,
                34.0/64.0, 18.0/64.0, 46.0/64.0, 30.0/64.0, 33.0/64.0, 17.0/64.0, 45.0/64.0, 29.0/64.0,
                10.0/64.0, 58.0/64.0,  6.0/64.0, 54.0/64.0,  9.0/64.0, 57.0/64.0,  5.0/64.0, 53.0/64.0,
                42.0/64.0, 26.0/64.0, 38.0/64.0, 22.0/64.0, 41.0/64.0, 25.0/64.0, 37.0/64.0, 21.0/64.0
            };

            float GetBayerDither(float2 screenPos) {
                uint x = (uint)screenPos.x % 8;
                uint y = (uint)screenPos.y % 8;
                return bayerMatrix[y * 8 + x] - 0.5; // centered: -0.5 .. +0.5
            }

            half4 Frag (Varyings i) : SV_Target {
                return half4(i.color);
            }
            ENDHLSL
        }
    }
    Fallback Off
}
