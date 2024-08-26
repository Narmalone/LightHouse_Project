using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

[CreateAssetMenu(fileName = "OceanSettings_", menuName = "Meteo/Ocean")]
public class OceanSettings : ScriptableObject
{
    [Header("--- SIMULATION ---")]
    public float TimeMultiplier = 1;

    [Header("Swell")]
    public float RepetitionSize;
    public float DistantWindSpeed;
    public float DistantWindOrientation = 0f;

    [Range(0, 1)] public float SwellChaos = 0f;
    public float ChaosSpeed = 0f;
    public float ChaosOrientation = 0f;

    [Header("First Band")]
    [Range(0, 1f)] public float AmplitudeAttenuationFirst = 0f;

    [Header("Second Band")]
    [Range(0, 1f)] public float AmplitudeAttenuationSecond = 0f;

    [Header("Ripples")]
    public bool EnableRipples = true;
    [Range(0, 15f)] public float LocalWindSpeed = 15f;
    public float LocalWindOrientation = 0f;
    [Range(0, 1f)] public float RippleChaos = 0f;

    [Header("--- APPEARANCE ---")]
    [Header("Smoothness")]
    public float FadeRangeStart = 100f;
    public float FadeRangeDistance = 500f;

    [Header("Refraction")]
    public Color RefractionColor;
    [Range(0, 3.5f)] public float MaximumDistance = 1f;
    [Range(0, 100f)] public float AbsorbtionDistance = 20f;

    [Header("Scattering")]
    public Color ScatteringColor;
    [Range(0, 1f)] public float AmbientTerm = 0.2f;
    [Range(0, 1f)] public float HeightTerm = 0.2f;
    [Range(0, 1f)] public float DisplacementTerm = 0.1f;
    [Range(0, 1f)] public float DirectLightTipTerm = 0.1f;
    [Range(0, 1f)] public float DirectLightBodyTerm = 0.1f;

    [Header("CAUSTIC")]
    public bool EnableCaustic = true;
    public WaterSurface.WaterCausticsResolution CausticsResolution = WaterSurface.WaterCausticsResolution.Caustics256;
    [Range(0, 2)] public int SimulationBand = 2;
    public float VirtualPlaneDistance = 60f;

    [Header("UNDERWATER")]
    public bool EnableUnderWater = false;
    public float VolumeDepth = 50f;
    public float VolumePriority = 0f;
    public float TransitionSize = 0.1f;
    public float AbsorbtionDistanceMultiplier = 1f;

}
