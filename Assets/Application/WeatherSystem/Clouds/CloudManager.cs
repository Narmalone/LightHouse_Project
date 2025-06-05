using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

public class CloudManager : MonoBehaviour
{
    public WeatherManager WeatherManager;
    public Light SunLight;
    public Volume volume;

    private VolumetricClouds clouds;
    private float currentDensity;
    private float densityVelocity;

    public bool UpdateDensity;
    public bool UpdateErrosion;
    public bool UpdateMicroErrosion;
    public bool UpdateAltitude;


    private float currentErosionFactor;
    private float erosionVelocity;
    private float currentMicroErosionFactor;
    private float microErosionVelocity;
    private float erosionSmoothTime = 1.5f;

    private void Awake()
    {
        if (volume.profile.TryGet(out VolumetricClouds cloudComponent))
            clouds = cloudComponent;
        else
            Debug.LogError("Le Volume n’a pas de composant Volumetric Clouds.");
    }

    private void Update()
    {
        if (WeatherManager.CurrentWeather == null || clouds == null || SunLight == null) return;

        WeatherData weather = WeatherManager.CurrentWeather;

        // 🔄 Normalisation réaliste
        float tHumidity = Mathf.InverseLerp(0f, 100f, weather.Humidity);
        float tPressure = Mathf.InverseLerp(980f, 1030f, weather.AtmosphericPressure);
        float tWind = Mathf.InverseLerp(0f, 30f, weather.WindSpeed);
        float tTemp = Mathf.InverseLerp(-30f, 40f, weather.AirTemperature);
        float tTime = Mathf.InverseLerp(5f, 20f, WeatherManager.TimeManager.currentTime);
        float sunIntensity01 = Mathf.Clamp01(SunLight.intensity / 1.5f);

        // ☁️ DENSITÉ : influencée par l'humidité et le type météo
        float baseDensity = 0.2f + tHumidity * 0.6f;
        if (weather.WeatherType == WeatherType.Stormy) baseDensity = 1f;
        else if (weather.WeatherType == WeatherType.Foggy) baseDensity = Mathf.Clamp01(baseDensity + 0.3f);
        else if (weather.WeatherType == WeatherType.Sunny) baseDensity = Mathf.Clamp01(baseDensity - 0.2f);

        if (UpdateDensity)
        {
            float density = Mathf.SmoothDamp(currentDensity, baseDensity, ref densityVelocity, 1f);
            currentDensity = Mathf.Clamp(density, 0.1f, 0.95f);
            clouds.densityMultiplier.value = currentDensity;
        }
        

        // 🌀 FORME : dépend du vent (plus fort = plus turbulent)
        clouds.shapeFactor.value = Mathf.Lerp(0.5f, 1f, tWind);
        //clouds.shapeScale.value = Mathf.Lerp(4f, 6f, tHumidity);

        if (UpdateErrosion)
        {
            float targetErosion = Mathf.Lerp(0.6f, 0.95f, tPressure);
            currentErosionFactor = Mathf.SmoothDamp(currentErosionFactor, targetErosion, ref erosionVelocity, erosionSmoothTime);
            clouds.erosionFactor.value = currentErosionFactor;

            clouds.erosionScale.value = Mathf.Lerp(40f, 80f, tWind); // moins sensible
        }

        if (UpdateMicroErrosion)
        {
            float targetMicro = Mathf.Lerp(0.4f, 0.8f, tWind);
            currentMicroErosionFactor = Mathf.SmoothDamp(currentMicroErosionFactor, targetMicro, ref microErosionVelocity, erosionSmoothTime);
            clouds.microErosionFactor.value = currentMicroErosionFactor;

            clouds.microErosionScale.value = Mathf.Lerp(150f, 400f, tWind); // ok en direct
        }



        if (UpdateAltitude)
        {
            // 🏔️ ALTITUDE : plus basse quand humidité/pression fortes
            clouds.bottomAltitude.value = Mathf.Lerp(800f, 2500f, 1f - tHumidity);
            clouds.altitudeRange.value = Mathf.Lerp(1200f, 3000f, 1f - tPressure);
        }
       
        // 🎨 COULEUR (Scattering)
        Color baseColor = Color.Lerp(new Color(0.85f, 0.9f, 1f), Color.gray, tHumidity);
        if (weather.WeatherType == WeatherType.Stormy)
            baseColor = new Color(0.2f, 0.2f, 0.25f);
        else if (weather.WeatherType == WeatherType.Snowy)
            baseColor = Color.Lerp(baseColor, new Color(0.9f, 0.95f, 1f), tTemp);

        baseColor *= Mathf.Lerp(0.5f, 1.2f, tTime) * sunIntensity01;
        clouds.scatteringTint.value = baseColor;

/*        var s = new WindParameter.WindParamaterValue();
        s.customValue = weather.WindSpeed;
        clouds.globalWindSpeed.Override(s);
        s.customValue = weather.WindOrientation;
        clouds.orientation.value = s;*/
        // BONUS 🌬️ Vent (optionnel si HDRP le gère)
        // float angle = weather.WindOrientation;
        // clouds.orientation.value = angle;
    }
}

public static class CloudPresetBlender
{
    public static void ApplyLerpedPreset(VolumetricClouds target, CloudPreset from, CloudPreset to, float t)
    {
        target.densityMultiplier.value = Mathf.Lerp(from.densityMultiplier, to.densityMultiplier, t);
        target.shapeFactor.value = Mathf.Lerp(from.shapeFactor, to.shapeFactor, t);
        target.erosionFactor.value = Mathf.Lerp(from.erosionFactor, to.erosionFactor, t);
        target.erosionScale.value = Mathf.Lerp(from.erosionScale, to.erosionScale, t);
        target.microErosionFactor.value = Mathf.Lerp(from.microErosionFactor, to.microErosionFactor, t);
        target.microErosionScale.value = Mathf.Lerp(from.microErosionScale, to.microErosionScale, t);
        target.bottomAltitude.value = Mathf.Lerp(from.bottomAltitude, to.bottomAltitude, t);
        target.altitudeRange.value = Mathf.Lerp(from.altitudeRange, to.altitudeRange, t);
        target.scatteringTint.value = Color.Lerp(from.scatteringTint, to.scatteringTint, t);

        // Pour les courbes, tu peux interpoler chaque point (approche simple mais efficace) :
        target.densityCurve.value = LerpCurve(from.densityCurve, to.densityCurve, t);
        target.erosionCurve.value = LerpCurve(from.erosionCurve, to.erosionCurve, t);
        target.ambientOcclusionCurve.value = LerpCurve(from.ambientOcclusionCurve, to.ambientOcclusionCurve, t);
    }

    private static AnimationCurve LerpCurve(AnimationCurve a, AnimationCurve b, float t)
    {
        if (a == null || b == null || a.length != b.length)
            return t < 0.5f ? a : b;

        Keyframe[] keys = new Keyframe[a.length];
        for (int i = 0; i < keys.Length; i++)
        {
            var ka = a[i];
            var kb = b[i];

            keys[i] = new Keyframe(
                Mathf.Lerp(ka.time, kb.time, t),
                Mathf.Lerp(ka.value, kb.value, t),
                Mathf.Lerp(ka.inTangent, kb.inTangent, t),
                Mathf.Lerp(ka.outTangent, kb.outTangent, t)
            );
        }

        return new AnimationCurve(keys);
    }
}
