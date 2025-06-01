using LightHouse.Game.DayNightSystem;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

public class LightingProfileManager : MonoBehaviour, ITimeCycleObserver
{
    public Light sunLight;
    public Volume skyVolume;
    public Volume postProcessVolume;

    [Header("Profils")]
    public LightingProfile nightProfile;
    public LightingProfile morningProfile;
    public LightingProfile dayProfile;
    public LightingProfile eveningProfile;

    private HDRISky hdriSky;
    private Fog fog;
    private Exposure exposure;

    private void Start()
    {
        FindObjectOfType<TimeManager>().RegisterObserver(this);

        skyVolume.profile.TryGet(out hdriSky);
        skyVolume.profile.TryGet(out fog);
        postProcessVolume.profile.TryGet(out exposure);
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
        else if (timeOfDay >= 20f || timeOfDay < 5f)
        {
            from = eveningProfile;
            to = nightProfile;
            t = timeOfDay < 5f ? timeOfDay / 5f : (timeOfDay - 20f) / 4f;
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
        // Lumière du soleil
        sunLight.color = Color.Lerp(a.sunColor, b.sunColor, t);
        sunLight.intensity = Mathf.Lerp(a.sunIntensity, b.sunIntensity, t);
        sunLight.colorTemperature = Mathf.Lerp(a.temperature, b.temperature, t);

        // Couleur ambiante
        RenderSettings.ambientLight = Color.Lerp(a.ambientColor, b.ambientColor, t);
        RenderSettings.ambientIntensity = Mathf.Lerp(a.ambientIntensity, b.ambientIntensity, t);

        // Fog HDRP
        if (fog != null)
        {
            fog.albedo.value = Color.Lerp(a.fogColor, b.fogColor, t);
            fog.meanFreePath.value = Mathf.Lerp(100f, 1f / Mathf.Max(0.001f, Mathf.Lerp(a.fogDensity, b.fogDensity, t)), t);
        }

        // Post-process (exposition)
        if (exposure != null)
        {
            exposure.fixedExposure.value = Mathf.Lerp(a.exposure, b.exposure, t);
        }
    }
}
