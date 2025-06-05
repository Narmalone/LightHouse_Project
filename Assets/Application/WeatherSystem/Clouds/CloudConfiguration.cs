using UnityEngine;

[CreateAssetMenu(menuName = "Environment/Cloud Preset")]
public class CloudPreset : ScriptableObject
{
    public float densityMultiplier;
    public float shapeFactor;
    public float erosionFactor;
    public float erosionScale;
    public float microErosionFactor;
    public float microErosionScale;
    public float bottomAltitude;
    public float altitudeRange;
    public Color scatteringTint;
    public AnimationCurve densityCurve;
    public AnimationCurve erosionCurve;
    public AnimationCurve ambientOcclusionCurve;
}
