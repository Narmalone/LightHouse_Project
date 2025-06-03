using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

[CreateAssetMenu(fileName = "LightingProfile", menuName = "DayNight/Lighting Profile")]
public class LightingProfile : ScriptableObject
{
    [Header("--- LIGHT ---")]
    public Color sunColor = Color.white;
    public float sunIntensity = 5000f;
    public float temperature = 6500f;

    [Header("--- PHYSICALLY BASED SKY ---")]
    public Color GroundTint;

    [Header("- AEROSOL -")]
    [Range(0, 1)] public float AerosolDensity;
    public Color AerosolTint;
    public float AerosolMaximumAltitude = 8290f;

    [Header("- Artistic Overrides -")]
    public Color HorizonTint;
    public float HorizonZenithShift;
    public Color ZenithTint;

    [Header("--- RENDER SETTINGS ---")]
    public Color ambientColor = Color.gray;
    public float ambientIntensity = 1.0f;

    [Header(" --- FOG --- ")]
    public float FogAttenuationDistance = 400f;
    public float BaseHeight = 5.0f;
    public float MaximumHeight = 115f;
    public float MaxFogDistance = 5000f;
    [ColorUsage(true, true)] public Color Tint;

    [Header("- Volumetric -")]
    public bool VolumetricFog = true;
    public Color Albedo;
    [Range(0, 1)] public float GIDimmer = 1.0f;
    public float VolumetricFogDistance;
    public FogDenoisingMode DenoisingMode = FogDenoisingMode.Gaussian;

    [Header(" --- EXPOSURE --- ")]
    public float Exposure = 12.0f;
    public float Compensation = 0.0f;

    [Header(" --- COLOR ADJUSTEMENTS --- ")]
    public float PostExposure = 0.1f;
    public float Contrasts = -15f;
    public float Saturation = -15f;
}
