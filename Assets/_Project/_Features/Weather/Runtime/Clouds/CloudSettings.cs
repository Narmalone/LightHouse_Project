using LightHouse.EditorTools.SuperGameManager;
using UnityEngine;

namespace LightHouse.Features.Weather.Clouds.Settings
{
    [CreateAssetMenu(fileName = "CloudSettings_", menuName = "Meteo/Clouds")]
    public class CloudSettings : ScriptableObject
    {
        [Header("Density")]
        [SgmExpose("Weather", "Density Multiplier")]
        public float DensityMultiplier;
        [SgmExpose("Weather", "Density Curve")]
        public AnimationCurve DensityCurve;

        [Header("Shape")]
        [SgmExpose("Weather", "Shape Factor")]
        public float ShapeFactor;
        [SgmExpose("Weather", "Shape Scale")]
        public float ShapeScale;

        [Header("Erosion")]
        [SgmExpose("Weather", "Erosion Factor")]
        public float ErosionFactor;
        [SgmExpose("Weather", "Erosion Scale")]
        public float ErosionScale;
        [SgmExpose("Weather", "Erosion Curve")]
        public AnimationCurve ErosionCurve;

        [Header("Ambiant Occlusion")]
        [SgmExpose("Weather", "Ambien Occlusion Curve")]
        public AnimationCurve AmbientOcclusionCurve;

        [Header("Altitude")]
        [SgmExpose("Weather", "Bottom Altitude")]
        public float BottomAltitude;
        [SgmExpose("Weather", "Altitude Range")]
        public float AltitudeRange;

        [Header("Lighting")]
        [SgmExpose("Weather", "Ambient Light Probe Dimmer")]
        public float AmbientLightProbeDimmer = 1.0f;
        [SgmExpose("Weather", "Sunlight Dimmer")]
        public float SunLightDimmer = 1.0f;

        [SgmExpose("Weather", "Scattering Tint")]
        public Color ScatteringTint = Color.black;

        [Header("Other")]
        [SgmExpose("Weather", "Shape Offset")]
        public Vector3 ShapeOffset;
        [SgmExpose("Weather", "Earth Curvature")]
        public float EarthCurvature;
    }

}
