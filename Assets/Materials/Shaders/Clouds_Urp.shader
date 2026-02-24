Shader "Custom/Clouds_Urp"
{
    Properties
    {
        [Header(Cloud Colors)]
        _CloudColorA("Cloud Color A", Color) = (0.96, 0.97, 1.0, 1.0)
        _CloudColorB("Cloud Color B", Color) = (1.0, 1.0, 1.0, 1.0)

        [Header(Shape)]
        _CloudCoverage("Coverage", Range(0.0, 1.0)) = 0.52
        _CloudSoftness("Softness", Range(0.01, 0.5)) = 0.2
        _CloudOpacity("Opacity", Range(0.0, 1.0)) = 0.78
        _PrimaryScale("Primary Scale", Range(0.2, 12.0)) = 2.4
        _SecondaryScale("Secondary Scale", Range(0.2, 18.0)) = 5.0
        _DetailStrength("Detail Strength", Range(0.0, 1.0)) = 0.4

        [Header(Motion)]
        _PrimarySpeed("Primary Speed (XY)", Vector) = (0.03, 0.01, 0, 0)
        _SecondarySpeed("Secondary Speed (XY)", Vector) = (-0.015, 0.02, 0, 0)
        _DistortionStrength("Distortion Strength", Range(0.0, 1.0)) = 0.08
        _DistortionSpeed("Distortion Speed (XY)", Vector) = (0.02, -0.01, 0, 0)

        [Header(UV)]
        _UVTiling("UV Tiling", Vector) = (1, 1, 0, 0)
        _UVOffset("UV Offset", Vector) = (0, 0, 0, 0)
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Transparent"
            "Queue" = "Transparent"
            "RenderPipeline" = "UniversalPipeline"
            "UniversalMaterialType" = "Unlit"
        }

        Pass
        {
            Name "Clouds"
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

            CBUFFER_START(UnityPerMaterial)
                half4 _CloudColorA;
                half4 _CloudColorB;
                float _CloudCoverage;
                float _CloudSoftness;
                float _CloudOpacity;
                float _PrimaryScale;
                float _SecondaryScale;
                float _DetailStrength;
                float4 _PrimarySpeed;
                float4 _SecondarySpeed;
                float _DistortionStrength;
                float4 _DistortionSpeed;
                float4 _UVTiling;
                float4 _UVOffset;
            CBUFFER_END

            float hash21(float2 p)
            {
                p = frac(p * float2(123.34, 345.45));
                p += dot(p, p + 34.345);
                return frac(p.x * p.y);
            }

            float noise2d(float2 p)
            {
                float2 i = floor(p);
                float2 f = frac(p);
                float2 u = f * f * (3.0 - 2.0 * f);

                float a = hash21(i + float2(0.0, 0.0));
                float b = hash21(i + float2(1.0, 0.0));
                float c = hash21(i + float2(0.0, 1.0));
                float d = hash21(i + float2(1.0, 1.0));

                return lerp(lerp(a, b, u.x), lerp(c, d, u.x), u.y);
            }

            float fbm(float2 p)
            {
                float value = 0.0;
                float amplitude = 0.5;
                float frequency = 1.0;

                [unroll]
                for (int i = 0; i < 5; i++)
                {
                    value += amplitude * noise2d(p * frequency);
                    frequency *= 2.0;
                    amplitude *= 0.5;
                }

                return value;
            }

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = IN.uv;
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                float2 uv = IN.uv * _UVTiling.xy + _UVOffset.xy;
                float2 timePrimary = _Time.y * _PrimarySpeed.xy;
                float2 timeSecondary = _Time.y * _SecondarySpeed.xy;
                float2 timeDistort = _Time.y * _DistortionSpeed.xy;

                float2 distortionUV = uv * (_PrimaryScale * 0.8) + timeDistort;
                float2 distortion = (float2(noise2d(distortionUV), noise2d(distortionUV + 13.7)) - 0.5) * _DistortionStrength;

                float n1 = fbm((uv + timePrimary + distortion) * _PrimaryScale);
                float n2 = fbm((uv + timeSecondary - distortion) * _SecondaryScale);

                float cloudField = lerp(n1, n2, _DetailStrength);
                float threshold = 1.0 - _CloudCoverage;
                float alpha = smoothstep(threshold - _CloudSoftness, threshold + _CloudSoftness, cloudField) * _CloudOpacity;

                half3 cloudColor = lerp(_CloudColorA.rgb, _CloudColorB.rgb, saturate(cloudField * 1.1));
                return half4(cloudColor, alpha);
            }
            ENDHLSL
        }
    }
}
