using LightHouse.Core.Utilities;
using LightHouse.Features.TimeOfDay.Lighting;
using LightHouse.Features.TimeOfDay.TimeCore;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using CloudSettings = LightHouse.Features.Weather.Clouds.Settings.CloudSettings;

namespace LightHouse.Features.Weather.Clouds
{
    public class CloudManager : MonoBehaviour
    {
        public WeatherManager WeatherManager;
        public Light SunLight;
        public Volume volume;

        public bool RealisticMode = true;

        private UnityEngine.Rendering.HighDefinition.VolumetricClouds clouds;

        [Header("Presets météo")]
        public CloudSettings Sunny;
        public CloudSettings Cloudy;
        public CloudSettings Windy;
        public CloudSettings Rainy;
        public CloudSettings Foggy;
        public CloudSettings Snowy;
        public CloudSettings Stormy;

        public bool EnableUpdateErrosion;
        public float erosionUpdateInterval = 20f;
        private float erosionTimer = 0f;

        private float densityVelocity;
        private float erosionVelocity;
        private float microErosionVelocity;

        private Dictionary<WeatherType, CloudSettings> presetMap;

        private void Awake()
        {

            presetMap = new Dictionary<WeatherType, CloudSettings>
            {
                { WeatherType.Sunny, Sunny },
                { WeatherType.Cloudy, Cloudy },
                { WeatherType.Windy, Windy },
                { WeatherType.Rainy, Rainy },
                { WeatherType.Foggy, Foggy },
                { WeatherType.Stormy, Stormy }
            };
        }

        private void Start()
        {
            if (!volume)
            {
                volume = FindFirstObjectByType<Volume>();
            }

            if(!SunLight)
            {
                SunLight = FindFirstObjectByType<LightingProfileManager>().SunController.SunLight;
            }

            if(!WeatherManager)
            {
                WeatherManager = FindFirstObjectByType<WeatherManager>();
            }

            if (!volume.profile.TryGet(out clouds))
            {
                Debug.LogError("Le Volume n’a pas de composant Volumetric Clouds.");
                return;
            }
        }

        private void Update()
        {
            if (WeatherManager == null || WeatherManager.CurrentWeather == null || clouds == null || SunLight == null) return;

            WeatherData weather = WeatherManager.CurrentWeather;
            WeatherData from = WeatherManager.FromWeather;
            WeatherData to = WeatherManager.ToWeather;
            float t = WeatherManager.CurrentBlend;

            CloudSettings fromPreset = presetMap.ContainsKey(from.WeatherType) ? presetMap[from.WeatherType] : Sunny;
            CloudSettings toPreset = presetMap.ContainsKey(to.WeatherType) ? presetMap[to.WeatherType] : Sunny;

            // Appliquer uniquement les courbes
            clouds.densityCurve.value = AnimationUtilities.LerpCurve(fromPreset.DensityCurve, toPreset.DensityCurve, t);
            clouds.erosionCurve.value = AnimationUtilities.LerpCurve(fromPreset.ErosionCurve, toPreset.ErosionCurve, t);
            clouds.ambientOcclusionCurve.value = AnimationUtilities.LerpCurve(fromPreset.AmbientOcclusionCurve, toPreset.AmbientOcclusionCurve, t);

            // Logique selon le mode
            if (RealisticMode)
                ApplyRealisticSettings(weather);
            else
                ApplyPresetSettings(clouds, fromPreset, toPreset, t);
        }

        private void ApplyRealisticSettings(WeatherData weather)
        {
            float tHumidity = Mathf.InverseLerp(0f, 100f, weather.Humidity);
            float tPressure = Mathf.InverseLerp(980f, 1030f, weather.AtmosphericPressure);
            float tWind = Mathf.InverseLerp(0f, 30f, weather.WindSpeed);
            float tTemp = Mathf.InverseLerp(-30f, 40f, weather.AirTemperature);
            float tTime = Mathf.InverseLerp(5f, 20f, TimeHandlerData.CurrentTime);
            float sunIntensity01 = Mathf.Clamp01(SunLight.intensity / 1.5f);

            // --- Densité équilibrée ---
            float targetDensity = 0.2f + tHumidity * 0.6f;
            if (weather.WeatherType == WeatherType.Stormy) targetDensity = 0.7f;
            else if (weather.WeatherType == WeatherType.Foggy) targetDensity = Mathf.Clamp(targetDensity + 0.3f, 0f, 0.75f);
            else if (weather.WeatherType == WeatherType.Sunny) targetDensity = Mathf.Clamp(targetDensity - 0.2f, 0f, 0.6f);

            clouds.densityMultiplier.value = Mathf.SmoothDamp(clouds.densityMultiplier.value, targetDensity, ref densityVelocity, 1f);

            // --- Forme nuages : plus bas/compact si vent ou pression basse ---
            float pressureEffect = 1f - tPressure;
            float shapeIntensity = Mathf.Clamp01(tWind * 0.8f + (1f - tPressure));
            clouds.shapeFactor.value = Mathf.Lerp(0.4f, 1.0f, 1f - shapeIntensity); // + intense = plus petit

            // --- Érosion ---
            float targetErosion = Mathf.Lerp(0.6f, 0.95f, tPressure);
            clouds.erosionFactor.value = Mathf.SmoothDamp(clouds.erosionFactor.value, targetErosion, ref erosionVelocity, 1f);
            clouds.erosionScale.value = Mathf.Lerp(40f, 80f, tWind);

            float targetMicro = Mathf.Lerp(0.4f, 0.8f, tWind);
            clouds.microErosionFactor.value = Mathf.SmoothDamp(clouds.microErosionFactor.value, targetMicro, ref microErosionVelocity, 1f);
            clouds.microErosionScale.value = Mathf.Lerp(150f, 400f, tWind);

            // --- Altitude ---
            clouds.bottomAltitude.value = Mathf.Lerp(800f, 2500f, 1f - tHumidity);
            clouds.altitudeRange.value = Mathf.Lerp(1200f, 3000f, 1f - tPressure);

            // --- Scattering ---
            Color baseColor = Color.Lerp(new Color(0.85f, 0.9f, 1f), Color.gray, tHumidity);
            if (weather.WeatherType == WeatherType.Stormy)
                baseColor = new Color(0.25f, 0.25f, 0.3f); // moins noir

            baseColor *= Mathf.Lerp(0.6f, 1.2f, tTime) * sunIntensity01;
            clouds.scatteringTint.value = baseColor;
        }

        private void ApplyPresetSettings(UnityEngine.Rendering.HighDefinition.VolumetricClouds target, CloudSettings from, CloudSettings to, float t)
        {
            target.densityMultiplier.value = Mathf.SmoothDamp(
                target.densityMultiplier.value,
                Mathf.Lerp(from.DensityMultiplier, to.DensityMultiplier, t),
                ref densityVelocity, 1f
            );

            target.shapeFactor.value = Mathf.Lerp(from.ShapeFactor, to.ShapeFactor, t);

            erosionTimer += Time.deltaTime;
            if (erosionTimer >= erosionUpdateInterval)
            {
                erosionTimer = 0f;

                UpdateErosionFromWeather(WeatherManager.CurrentWeather);
            }

            if (EnableUpdateErrosion)
            {
                target.erosionFactor.value = Mathf.SmoothDamp
                (
                    target.erosionFactor.value,
                    Mathf.Lerp(from.ErosionFactor, to.ErosionFactor, t),
                    ref erosionVelocity, 1f
                );
                target.erosionScale.value = Mathf.Lerp(from.ErosionScale, to.ErosionScale, t);
                target.microErosionFactor.value = Mathf.SmoothDamp(
                    target.microErosionFactor.value,
                    Mathf.Lerp(from.ErosionFactor, to.ErosionFactor, t),
                    ref microErosionVelocity, 1f
                );

                target.microErosionScale.value = Mathf.Lerp(150f, 400f, t);
            }


            target.bottomAltitude.value = Mathf.Lerp(from.BottomAltitude, to.BottomAltitude, t);
            target.altitudeRange.value = Mathf.Lerp(from.AltitudeRange, to.AltitudeRange, t);

            target.ambientLightProbeDimmer.value = Mathf.Lerp(from.AmbientLightProbeDimmer, to.AmbientLightProbeDimmer, t);
            target.sunLightDimmer.value = Mathf.Lerp(from.SunLightDimmer, to.SunLightDimmer, t);
            target.scatteringTint.value = Color.Lerp(from.ScatteringTint, to.ScatteringTint, t);
        }

        private void UpdateErosionFromWeather(WeatherData weather)
        {
            float tPressure = Mathf.InverseLerp(980f, 1030f, weather.AtmosphericPressure);
            float tWind = Mathf.InverseLerp(0f, 30f, weather.WindSpeed);

            clouds.erosionFactor.value = Mathf.Lerp(0.6f, 0.95f, tPressure);
            clouds.erosionScale.value = Mathf.Lerp(40f, 80f, tWind);
            clouds.microErosionFactor.value = Mathf.Lerp(0.4f, 0.8f, tWind);
            clouds.microErosionScale.value = Mathf.Lerp(150f, 400f, tWind);
        }

    }

}
