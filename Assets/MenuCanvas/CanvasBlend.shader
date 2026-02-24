Shader "Custom/CanvasBlend"
{
    Properties
    {
        [MainColor] _BaseColor("Base Color", Color) = (1, 1, 1, 1)
        [MainTexture] _ImgA("Image 1", 2D) = "white" {}
        _ImgB("Image 2", 2D) = "white" {}
        _Speed("Blend Speed", Float) = 1.0
        _Gamma("Balance value", Range(0.1, 3.0)) = 1.0
        _Exposure("Exposure", Float) = 1.0
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" }

        Pass
        {
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

            TEXTURE2D(_ImgA);
            TEXTURE2D(_ImgB);
            SAMPLER(sampler_ImgA);
            SAMPLER(sampler_ImgB);
            float _Speed;
            float _Gamma;
            float _Exposure;

            CBUFFER_START(UnityPerMaterial)
                half4 _BaseColor;
                float4 _ImgA_ST;
                float4 _ImgB_ST;
            CBUFFER_END

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = TRANSFORM_TEX(IN.uv, _ImgA);
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                half4 color = SAMPLE_TEXTURE2D(_ImgA, sampler_ImgA, IN.uv) * _BaseColor;
                half4 colorB = SAMPLE_TEXTURE2D(_ImgB, sampler_ImgB, IN.uv);
                float blend = sin(_Time.x * _Speed) * 0.5 + 0.5;
                color = lerp(color, colorB, pow(blend, _Gamma));
                return color * _Exposure;
            }
            ENDHLSL
        }
    }
}
