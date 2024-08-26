using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

[CreateAssetMenu(fileName = "CloudSettings_", menuName = "Meteo/Clouds")]
public class CloudSettings : ScriptableObject
{
    public float DensityMultiplier;
    public AnimationCurve DensityCurve;

    public float ShapeFactor;
    public float ShapeScale;
    public float ErosionFactor;
    public float ErosionScale;
    public AnimationCurve ErosionCurve;
    public AnimationCurve AmbientOcclusionCurve;

    public float BottomAltitude;
    public float AltitudeRange;

    public Vector3 ShapeOffset;
    public float EarthCurvature;

    /*public void LoadSettings(VolumetricClouds target)
    {
        target.densityMultiplier.Override(DensityMultiplier);
        target.densityCurve.Override(DensityCurve);

        target.shapeFactor.Override(ShapeFactor);
        target.shapeScale.Override(ShapeScale);

        target.erosionFactor.Override(ErosionFactor);
        target.erosionScale.Override(ErosionScale);
        target.erosionCurve.Override(ErosionCurve);
        target.ambientOcclusionCurve.Override(AmbientOcclusionCurve);

        target.bottomAltitude.Override(BottomAltitude); 
        target.altitudeRange.Override(AltitudeRange);

        target.shapeOffset.Override(ShapeOffset);
        target.earthCurvature.Override(EarthCurvature);
    }*/
}
