Shader "Custom/RegionMapHover"
{
    Properties
    {
        _HoverColor("Hover Color", Color) = (1,0,0,1)
        [MainTexture] _RegionBorders("Borders Texture", 2D) = "white" {}
        _RegionsTexture("Regions Texture", 2D) = "white" {}
    }

    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" "RenderPipeline"="UniversalPipeline" }

        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Off

            HLSLPROGRAM
            

            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            TEXTURE2D(_RegionBorders);
            SAMPLER(sampler_RegionBorders);

            TEXTURE2D(_RegionsTexture);
            SAMPLER(sampler_RegionsTexture);

            CBUFFER_START(UnityPerMaterial)
                float4 _RegionBorders_ST;
                float4 _RegionsTexture_ST;
            CBUFFER_END

            float _RegionHovers[10];
            float4 _RegionsTexture_TexelSize;
            float4 _HoverColor;

            float LinearToGamma(float v)
            {
                return v <= 0.0031308 ? v * 12.92 : 1.055 * pow(v, 1.0 / 2.4) - 0.055;
            }

            int DecodeRegionIndex(float regionValue)
            {
                float encoded = saturate(regionValue);
            #ifndef UNITY_COLORSPACE_GAMMA
                encoded = LinearToGamma(encoded);
            #endif

                float regionByte = floor(encoded * 255.0 + 0.5);
                return clamp((int)floor(regionByte * (10.0 / 256.0)), 0, 9);
            }

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = TRANSFORM_TEX(IN.uv, _RegionsTexture);
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                float2 texelCoord = floor(IN.uv * _RegionsTexture_TexelSize.zw);
                float2 pixelUV = (texelCoord + 0.5) * _RegionsTexture_TexelSize.xy;
                float4 regionsTextureData = SAMPLE_TEXTURE2D(_RegionsTexture, sampler_RegionsTexture, pixelUV);
                int regionIndex = DecodeRegionIndex(regionsTextureData.r);
                float alphaMask = _RegionHovers[regionIndex] * regionsTextureData.a;
                return float4(saturate(_HoverColor.rgb), saturate(alphaMask) * _HoverColor.a);
            }
            ENDHLSL
        }
    }
}
