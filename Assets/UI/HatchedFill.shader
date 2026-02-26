Shader "UI/HatchedFill"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)

        [Header(Hatching)]
        _LineColor ("Line Color", Color) = (1,1,1,1)
        _BgColor   ("Background Color", Color) = (0,0,0,0)
        _LineWidth ("Line Width", Range(0.01, 0.5)) = 0.15
        _LineSpacing ("Line Spacing", Range(1, 100)) = 20
        _LineAngle ("Line Angle (degrees)", Range(0, 180)) = 45
        _TileScale ("Tile Scale (world units)", Range(0.001, 0.1)) = 0.01

        [Header(Wave Animation)]
        _WaveAmount ("Wave Intensity", Range(0, 1)) = 0.2
        _WaveFreq  ("Wave Frequency", Range(1, 50)) = 10
        _WaveSpeed ("Wave Speed", Range(0, 10)) = 2

        [Header(Antialiasing)]
        _Smoothness ("Edge Smoothness", Range(0, 0.1)) = 0.02

        // --- UI support ---
        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil     ("Stencil ID", Float) = 0
        _StencilOp   ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask  ("Stencil Read Mask", Float) = 255
        _ColorMask ("Color Mask", Float) = 15

        [Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]

        Pass
        {
            Name "Default"

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0

            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            #pragma multi_compile_local _ UNITY_UI_CLIP_RECT
            #pragma multi_compile_local _ UNITY_UI_ALPHACLIP

            struct appdata
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 uv       : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                fixed4 color    : COLOR;
                float2 uv       : TEXCOORD0;
                float4 worldPos : TEXCOORD1;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            sampler2D _MainTex;
            fixed4 _Color;
            fixed4 _TextureSampleAdd;
            float4 _ClipRect;

            fixed4 _LineColor;
            fixed4 _BgColor;
            float _LineWidth;
            float _LineSpacing;
            float _LineAngle;
            float _Smoothness;
            float _TileScale;
            float _WaveAmount;
            float _WaveFreq;
            float _WaveSpeed;

            v2f vert(appdata v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

                o.vertex   = UnityObjectToClipPos(v.vertex);
                o.worldPos = v.vertex;
                o.uv       = v.uv;
                o.color    = v.color * _Color;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // Sample the sprite texture (needed for fill amount to work)
                half4 texColor = (tex2D(_MainTex, i.uv) + _TextureSampleAdd) * i.color;

                // Use world-space position for seamless tiling across multiple elements
                float2 wpos = i.worldPos.xy * _TileScale;

                // Compute hatching pattern
                float angleRad = _LineAngle * 3.14159265 / 180.0;
                float2 dir = float2(cos(angleRad), sin(angleRad));

                // Perpendicular direction for wave offset
                float2 perp = float2(-dir.y, dir.x);
                float alongLine = dot(wpos, perp);

                // Sinusoidal wave offset - use round(_WaveFreq) so wave tiles perfectly
                float waveFreqRound = round(_WaveFreq);
                float wave = sin(alongLine * waveFreqRound * 6.2831853 + _Time.y * _WaveSpeed) * _WaveAmount;

                // Project world pos onto the line direction + wave
                float proj = dot(wpos * _LineSpacing, dir) + wave;

                // Create repeating stripe pattern
                float stripe = frac(proj);

                // Smooth step for anti-aliased lines
                float halfWidth = _LineWidth * 0.5;
                float hatch = smoothstep(0.5 - halfWidth - _Smoothness, 0.5 - halfWidth, stripe)
                            - smoothstep(0.5 + halfWidth, 0.5 + halfWidth + _Smoothness, stripe);

                // Mix line color and background color
                fixed4 col = lerp(_BgColor, _LineColor, hatch);

                // Apply sprite alpha (this is what makes Fill Amount work)
                col.a *= texColor.a;

                #ifdef UNITY_UI_CLIP_RECT
                col.a *= UnityGet2DClipping(i.worldPos.xy, _ClipRect);
                #endif

                #ifdef UNITY_UI_ALPHACLIP
                clip(col.a - 0.001);
                #endif

                return col;
            }
            ENDCG
        }
    }
}
