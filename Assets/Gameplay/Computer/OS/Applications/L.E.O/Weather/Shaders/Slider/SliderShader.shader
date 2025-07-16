Shader "UI/RadialRingSegmentFill"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}
        _FillColor("Fill Color", Color) = (1,1,1,1)
        _FillValue("Fill Amount", Range(0,1)) = 0.5
        _RingThickness("Ring Thickness", Range(0,1)) = 0.1
        _StartAngle("Start Angle (rad)", Range(0,6.2831)) = 0
        _EndAngle("End Angle (rad)", Range(0,6.2831)) = 6.2831
        _Direction("Direction (1 = clockwise, -1 = counter)", Float) = 1
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }
        LOD 100

        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            Cull Off
            ZWrite Off

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex : POSITION;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            fixed4 _FillColor;
            float _FillValue;
            float _RingThickness;
            float _StartAngle;
            float _EndAngle;
            float _Direction;

            float NormalizeAngle(float angle)
            {
                return fmod(fmod(angle, 6.2831853) + 6.2831853, 6.2831853);
            }

            float AngleDiffClockwise(float a, float b)
            {
                float diff = b - a;
                if (diff < 0) diff += 6.2831853;
                return diff;
            }

            float AngleDiffCounterClockwise(float a, float b)
            {
                float diff = a - b;
                if (diff < 0) diff += 6.2831853;
                return diff;
            }

            v2f vert(appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float2 center = float2(0.5, 0.5);
                float2 dir = i.uv - center;
                float dist = length(dir);

                float outer = 0.5;
                float inner = outer - _RingThickness;

                if (dist < inner || dist > outer)
                    return float4(0, 0, 0, 0);

                float angle = NormalizeAngle(atan2(dir.y, dir.x));
                float start = NormalizeAngle(_StartAngle);
                float end = NormalizeAngle(_EndAngle);

                float arcSpan = (_Direction > 0)
                    ? AngleDiffClockwise(start, end)
                    : AngleDiffCounterClockwise(start, end);

                float filledArc = arcSpan * _FillValue;

                float angleFromStart = (_Direction > 0)
                    ? AngleDiffClockwise(start, angle)
                    : AngleDiffCounterClockwise(start, angle);

                if (angleFromStart > arcSpan)
                    return float4(0, 0, 0, 0);

                if (angleFromStart <= filledArc)
                    return _FillColor;

                return float4(0, 0, 0, 0);
            }
            ENDCG
        }
    }
}
