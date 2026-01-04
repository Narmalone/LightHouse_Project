using LightHouse.EditorTools.SuperGameManager;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

[CreateAssetMenu(fileName = "LightingProfile", menuName = "DayNight/Lighting Profile")]
public class LightingProfile : ScriptableObject
{
    [Header("--- SUN LIGHT ---")]
    [SgmExpose(label: "Sun Color")]
    public Color sunColor = Color.white;
    [SgmExpose(label: "Sun Intensity")]
    public float sunIntensity = 5000f;
    [SgmExpose(label: "Sun Temperature")]
    public float temperature = 6500f;

    [Header("--- SUN FLARE ---")]
    [SgmExpose(label: "Flare Intensity")]
    public float FlareIntensity = 2.0f;
    [SgmExpose(label: "Flare Scale")]
    public float FlareScale = 1.0f;

    [Header("--- PHYSICALLY BASED SKY ---")]
    [SgmExpose(label: "Ground Tint")]
    public Color GroundTint;

    [Header("- AEROSOL -")]
    [SgmExpose(label: "Aerosol Density")]
    [Range(0, 1)] public float AerosolDensity;
    [SgmExpose(label: "Aerosol Tint")]
    public Color AerosolTint;
    [SgmExpose(label: "Aerosol Maximum Altitude")]
    public float AerosolMaximumAltitude = 8290f;

    [Header("- Artistic Overrides -")]
    [SgmExpose(label: "Horizon tint")]
    public Color HorizonTint;
    [SgmExpose(label: "Horizon Zenith Shift")]
    public float HorizonZenithShift;
    [SgmExpose(label: "Zenith Tint")]
    public Color ZenithTint;

    [Header("--- RENDER SETTINGS ---")]
    [SgmExpose(label: "Ambient Color")]
    public Color ambientColor = Color.gray;
    [SgmExpose(label: "Ambient Intensity")]
    public float ambientIntensity = 1.0f;

    [Header(" --- FOG --- ")]
    [SgmExpose(label: "Fog Attenuation Distance")]
    public float FogAttenuationDistance = 400f;
    [SgmExpose(label: "Base Height")]
    public float BaseHeight = 5.0f;
    [SgmExpose(label: "Maximum Height")]
    public float MaximumHeight = 115f;
    [SgmExpose(label: "Max Fog Distance")]
    public float MaxFogDistance = 5000f;
    [SgmExpose(label: "Tint")]
    [ColorUsage(true, true)] public Color Tint;

    [Header("- Volumetric -")]
    [SgmExpose(label: "Volumetric Fog")]
    public bool VolumetricFog = true;
    [SgmExpose(label: "Albedo")]
    public Color Albedo;
    [SgmExpose(label: "GI Dimmer")]
    [Range(0, 1)] public float GIDimmer = 1.0f;
    [SgmExpose(label: "Volumetric Fog Distance")]
    public float VolumetricFogDistance;
    [SgmExpose(label: "Denoising Mode")]
    public FogDenoisingMode DenoisingMode = FogDenoisingMode.Gaussian;

    [Header(" --- EXPOSURE --- ")]
    [SgmExpose(label: "Exposure")]
    public float Exposure = 12.0f;
    [SgmExpose(label: "Compensation")]
    public float Compensation = 0.0f;

    [Header(" --- COLOR ADJUSTEMENTS --- ")]
    [SgmExpose(label: "Post Exposure")]
    public float PostExposure = 0.1f;
    [SgmExpose(label: "Contrasts")]
    public float Contrasts = -15f;
    [SgmExpose(label: "Saturation")]
    public float Saturation = -15f;
}
