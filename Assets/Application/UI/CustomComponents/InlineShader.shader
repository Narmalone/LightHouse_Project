Shader "UI/InlineRectShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Inline Color", Color) = (1,1,1,1)
        _InlineSize ("Inline Thickness", Range(0, 0.5)) = 0.1
        _Tiling ("Tiling", Vector) = (1,1,0,0)
        _Offset ("Offset", Vector) = (0,0,0,0)
        _Aspect ("Aspect Ratio (Width / Height)", Float) = 1
    }

    SubShader
    {
        Tags { "Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent" }
        LOD 100

        Cull Off
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _MainTex_ST;

            float4 _Color;
            float _InlineSize;
            float2 _Tiling;
            float2 _Offset;
            float _Aspect;

            struct appdata_t
            {
                float4 vertex : POSITION;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            v2f vert(appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.texcoord, _MainTex) * _Tiling + _Offset;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float2 uv = i.uv;

                // Aspect-corrected thickness
                float2 t = float2(_InlineSize, _InlineSize * _Aspect);

                float2 innerMin = t;
                float2 innerMax = 1.0 - t;

                // Distance from border (0 at edge, >0 inside)
                float2 borderDist = min(uv - innerMin, innerMax - uv);
                float dist = min(borderDist.x, borderDist.y);

                // Hard edge version (produit les artefacts)
                // float border = step(0.0, dist);

                // Smooth inline edge
                float edge = smoothstep(0.0, 0.005, dist); // <-- contrôle la douceur ici
                float4 col = _Color;
                col.a *= (1.0 - edge);

                return col;
            }

            ENDCG
        }
    }
}
