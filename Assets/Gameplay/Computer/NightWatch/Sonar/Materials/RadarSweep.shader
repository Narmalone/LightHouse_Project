Shader "Custom/RadarSweep"
{
    Properties
    {
        // === Couleurs ===
        [Header(Glow Settings)]
        [Space(5)]
        [HDR]_Color("Glow Color", Color) = (0, 1, 0, 1)
        _Intensity("Glow Intensity", Float) = 2

        // === Balayage ===
        [Header(Sweep Settings)]
        [Space(5)]
        _SweepAngle("Sweep Angle (Deg)", Range(0, 360)) = 0
        _SweepDistance("Sweep Distance (0-1)", Range(0.1, 1)) = 1
        _TrailLength("Trail Angle Length", Range(1, 180)) = 45

        // === Fondu ===
        [Header(Fade Sharpness)]
        [Space(5)]
        _AngleFade("Angular Fade Sharpness", Range(0.1, 10)) = 2
        _RadialFade("Radial Fade Sharpness", Range(0.1, 10)) = 2

        // === Texture et Aspect ===
        [Header(Advanced)]
        [Space(5)]
        _MainTex("Main Texture", 2D) = "white" {}
        _Aspect("Aspect Ratio (Width / Height)", Float) = 1
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off
        ZWrite Off
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            // === Propriétés ===
            sampler2D _MainTex;
            float4 _MainTex_ST;

            float4 _Color;
            float _Intensity;

            float _SweepAngle;
            float _SweepDistance;
            float _TrailLength;

            float _AngleFade;
            float _RadialFade;
            float _Aspect;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // Centrage UV
                float2 uv = i.uv - 0.5;

                // Corrige l’étirement en X selon le rapport hauteur/largeur
                uv.x *= _Aspect;

                float radius = length(uv);
                if (radius > _SweepDistance)
                    discard;

                // Calcul de l’angle en degrés [0, 360)
                float angle = degrees(atan2(uv.y, uv.x));
                if (angle < 0) angle += 360;

                // Différence entre l’angle du pixel et l’angle du balayage
                float sweepDiff = angle - _SweepAngle;
                if (sweepDiff < -180) sweepDiff += 360;
                if (sweepDiff > 180)  sweepDiff -= 360;

                // Fade angulaire (derrière la ligne de balayage uniquement)
                float behindSweep = -sweepDiff;
                float trailFade = saturate(1.0 - behindSweep / _TrailLength);
                float angularFade = (sweepDiff <= 0) ? pow(trailFade, _AngleFade) : 0.0;

                // Fade radial (en fonction de la distance depuis le centre)
                float radialFade = saturate(1.0 - pow(radius / _SweepDistance, _RadialFade));

                // Intensité finale
                float intensity = angularFade * radialFade * _Intensity;

                // Application de la couleur
                float3 color = _Color.rgb * intensity;
                float alpha = intensity * _Color.a;

                return float4(color, alpha);
            }
            ENDCG
        }
    }
}
