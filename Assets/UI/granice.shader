Shader "Custom/WorldMapHighlight"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _MouseUV ("Mouse UV", Vector) = (-1,-1,0,0)
        _OutlineColor ("Outline Color", Color) = (1,1,1,1)
        _OutlineWidth ("Outline Width", Float) = 0.005
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };
            struct v2f {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MouseUV;
            float4 _OutlineColor;
            float _OutlineWidth;

            v2f vert (appdata v) {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target {
                fixed4 col = tex2D(_MainTex, i.uv);
                fixed4 targetCol = tex2D(_MainTex, _MouseUV.xy);

                // Pokud je pixel stejné barvy jako ten pod myší
                if (distance(col.rgb, targetCol.rgb) < 0.01) {
                    float2 off = _OutlineWidth;
                    // Kontrola sousedù pro vykreslení hrany
                    if (distance(tex2D(_MainTex, i.uv + float2(off.x, 0)).rgb, col.rgb) > 0.01 ||
                        distance(tex2D(_MainTex, i.uv - float2(off.x, 0)).rgb, col.rgb) > 0.01 ||
                        distance(tex2D(_MainTex, i.uv + float2(0, off.y)).rgb, col.rgb) > 0.01 ||
                        distance(tex2D(_MainTex, i.uv - float2(0, off.y)).rgb, col.rgb) > 0.01) {
                        return _OutlineColor;
                    }
                }
                return col;
            }
            ENDCG
        }
    }
}