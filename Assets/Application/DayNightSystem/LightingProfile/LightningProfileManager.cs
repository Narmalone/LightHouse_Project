using LightHouse.Game.DayNightSystem;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

public class LightingProfileManager : MonoBehaviour, ITimeCycleObserver
{
    public Light sunLight;
    public Light moonLight;
    public Volume globalVolume;

    [Header("Profils")]
    public LightingProfile nightProfile;
    public LightingProfile morningProfile;
    public LightingProfile dayProfile;
    public LightingProfile eveningProfile;

    private Fog fog;
    private Exposure exposure;
    private PhysicallyBasedSky sky;
    private ColorAdjustments colorAdjustments;

    private void Start()
    {
        FindFirstObjectByType<TimeManager>().RegisterObserver(this);

        globalVolume.profile.TryGet(out fog);
        globalVolume.profile.TryGet(out exposure);
        globalVolume.profile.TryGet(out sky);
    }

    public Light GetMainLight()
    {
        return TimeHandlerData.TimeOfDay == TimeOfDaySegment.Morning || TimeHandlerData.TimeOfDay == TimeOfDaySegment.Midday ? sunLight : moonLight;
    }

    public void OnTimeChanged(float timeOfDay)
    {
        LightingProfile from = null;
        LightingProfile to = null;
        float t = 0f;

        if (timeOfDay >= 6f && timeOfDay < 9f)
        {
            from = morningProfile;
            to = dayProfile;
            t = (timeOfDay - 6f) / 3f;
        }
        else if (timeOfDay >= 9f && timeOfDay < 17f)
        {
            from = dayProfile;
            to = dayProfile;
            t = 0f;
        }
        else if (timeOfDay >= 17f && timeOfDay < 20f)
        {
            from = dayProfile;
            to = eveningProfile;
            t = (timeOfDay - 17f) / 3f;
        }
        else if (timeOfDay >= 20f && timeOfDay < 24f)
        {
            from = eveningProfile;
            to = nightProfile;
            t = (timeOfDay - 20f) / 4f;
        }
        else if (timeOfDay >= 0f && timeOfDay < 5f)
        {
            from = nightProfile;
            to = nightProfile; // nuit constante
            t = 0f;
        }
        else // 5h - 6h
        {
            from = nightProfile;
            to = morningProfile;
            t = (timeOfDay - 5f) / 1f;
        }


        ApplyInterpolatedProfile(from, to, t);
    }

    private void ApplyInterpolatedProfile(LightingProfile a, LightingProfile b, float t)
    {
        // --- LIGHT ---
        sunLight.color = Color.Lerp(a.sunColor, b.sunColor, t);
        sunLight.intensity = Mathf.Lerp(a.sunIntensity, b.sunIntensity, t);
        sunLight.colorTemperature = Mathf.Lerp(a.temperature, b.temperature, t);

        // --- RENDER SETTINGS ---
        //RenderSettings.ambientLight = Color.Lerp(a.ambientColor, b.ambientColor, t);
        //RenderSettings.ambientIntensity = Mathf.Lerp(a.ambientIntensity, b.ambientIntensity, t);

        // --- EXPOSURE ---
        if (exposure != null)
        {
            exposure.fixedExposure.value = Mathf.Lerp(a.Exposure, b.Exposure, t);
            exposure.compensation.value = Mathf.Lerp(a.Compensation, b.Compensation, t);
        }

        // --- HDRP FOG ---
        if (fog != null)
        {
            fog.tint.value = Color.Lerp(a.Tint, b.Tint, t);
            fog.baseHeight.value = Mathf.Lerp(a.BaseHeight, b.BaseHeight, t);
            fog.maximumHeight.value = Mathf.Lerp(a.MaximumHeight, b.MaximumHeight, t);
            fog.meanFreePath.value = Mathf.Lerp(a.FogAttenuationDistance, b.FogAttenuationDistance, t);
            fog.maxFogDistance.value = Mathf.Lerp(a.MaxFogDistance, b.MaxFogDistance, t);
            fog.albedo.value = Color.Lerp(a.Albedo, b.Albedo, t);
            fog.enableVolumetricFog.value = a.VolumetricFog || b.VolumetricFog; // si un des deux l'active
            //fog.volumetricFog.meanFreePath.value = Mathf.Lerp(a.VolumetricFogDistance, b.VolumetricFogDistance, t);
            fog.denoisingMode.value = t < 0.5f ? a.DenoisingMode : b.DenoisingMode;
            fog.globalLightProbeDimmer.value = Mathf.Lerp(a.GIDimmer, b.GIDimmer, t);
        }

        // --- ARTISTIC SKY OVERRIDES ---
        if (sky != null)
        {
            sky.groundTint.value = Color.Lerp(a.GroundTint, b.GroundTint, t);
            sky.horizonTint.value = Color.Lerp(a.HorizonTint, b.HorizonTint, t);
            sky.zenithTint.value = Color.Lerp(a.ZenithTint, b.ZenithTint, t);
            sky.horizonZenithShift.value = Mathf.Lerp(a.HorizonZenithShift, b.HorizonZenithShift, t);
            sky.aerosolDensity.value = Mathf.Lerp(a.AerosolDensity, b.AerosolDensity, t);
            sky.aerosolTint.value = Color.Lerp(a.AerosolTint, b.AerosolTint, t);
            sky.aerosolMaximumAltitude.value = Mathf.Lerp(a.AerosolMaximumAltitude, b.AerosolMaximumAltitude, t);
        }

        // --- COLOR ADJUSTMENTS ---
        if (colorAdjustments != null)
        {
            colorAdjustments.postExposure.value = Mathf.Lerp(a.PostExposure, b.PostExposure, t);
            colorAdjustments.contrast.value = Mathf.Lerp(a.Contrasts, b.Contrasts, t);
            colorAdjustments.saturation.value = Mathf.Lerp(a.Saturation, b.Saturation, t);
        }
    }

}
