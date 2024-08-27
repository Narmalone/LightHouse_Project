using UnityEngine;

[CreateAssetMenu(fileName = "RainSettings_", menuName = "Meteo/Rain")]
public class RainSettings : ScriptableObject
{
    public float Intensity;
    public float MinRainSpawnRate;
    public float MaxRainSpawnRate;
    public float FogAttenuationDistance;
    public AnimationCurve VolumeWeightCurve;
}