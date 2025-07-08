Shader "UI/CanvasGlowImage"
{
    Properties
    {
        _MainTex("Main Texture", 2D) = "white" {}
        [HDR]_GlowColor("Glow Color", Color) = (0, 1, 0, 1)
        _GlowIntensity("Glow Intensity", Float) = 2

        [Header(Alpha)]
        _Alpha("Global Alpha", Range(0, 1)) = 1
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "IgnoreProjector" = "True"
            "RenderType" = "Transparent"
            "PreviewType" = "Plane"
            "CanUseSpriteAtlas" = "True"
        }

        Lighting Off
        ZWrite Off
        Cull Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _MainTex_ST;

            fixed4 _GlowColor;
            float _GlowIntensity;
            float _Alpha;

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
                o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 texColor = tex2D(_MainTex, i.uv);

                // Multiplie la couleur HDR par intensité
                float3 glow = _GlowColor.rgb * _GlowIntensity;

                // Utilise l’alpha de la texture, modulé par alpha global
                float alpha = texColor.a * _GlowColor.a * _Alpha;

                return fixed4(glow, alpha);
            }
            ENDCG
        }
    }
}
