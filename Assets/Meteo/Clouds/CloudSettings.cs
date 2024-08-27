using UnityEngine;

[CreateAssetMenu(fileName = "CloudSettings_", menuName = "Meteo/Clouds")]
public class CloudSettings : ScriptableObject
{
    [Header("Density")]
    public float DensityMultiplier;
    public AnimationCurve DensityCurve;

    [Header("Shape")]
    public float ShapeFactor;
    public float ShapeScale;

    [Header("Erosion")]
    public float ErosionFactor;
    public float ErosionScale;
    public AnimationCurve ErosionCurve;

    [Header("Ambiant Occlusion")]
    public AnimationCurve AmbientOcclusionCurve;

    [Header("Altitude")]
    public float BottomAltitude;
    public float AltitudeRange;

    [Header("Other")]
    public Vector3 ShapeOffset;
    public float EarthCurvature;
}
