Shader "Custom/ConstantFresnelLens"
{
    Properties
    {
        _Color ("Lens Color", Color) = (1, 1, 1, 1)
        _FresnelPower ("Fresnel Power", Range(1, 10)) = 2
        _MinIntensity ("Min Fresnel Intensity", Range(0, 1)) = 0.2
        _MaxIntensity ("Max Fresnel Intensity", Range(0, 5)) = 1.5
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        LOD 200

        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Back
            Lighting Off

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float3 normal : TEXCOORD0;
                float3 viewDir : TEXCOORD1;
            };

            float4 _Color;
            float _FresnelPower;
            float _MinIntensity;
            float _MaxIntensity;

            v2f vert (appdata_t v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);

                // Transform the normal to world space
                o.normal = normalize(mul((float3x3)UNITY_MATRIX_IT_MV, v.normal));
                
                // Calculate the view direction
                float3 worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.viewDir = normalize(_WorldSpaceCameraPos - worldPos);

                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // Compute Fresnel effect
                float fresnel = pow(1.0 - dot(i.normal, i.viewDir), _FresnelPower);

                // Clamp Fresnel intensity to avoid extreme values
                fresnel = lerp(_MinIntensity, _MaxIntensity, fresnel);

                return fixed4(_Color.rgb * fresnel, fresnel);
            }
            ENDCG
        }
    }
}
