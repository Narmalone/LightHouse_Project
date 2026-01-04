using LightHouse.Game.DayNightSystem;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

namespace LightHouse.Weather.Ocean
{
    /// <summary>
    /// L'océan manager se base pour l'instant sur un Global Setting
    /// Souvent entre minimum et maximum e.g WindMin et WindMax et se basera par rapport au CurrentWind
    /// situé dans le WeatherHandlerData
    /// </summary>
    [RequireComponent(typeof(WaterSurface))]
    public class OceanManager : MonoBehaviour
    {
        public WeatherManager WeatherManager;
        public Light SunLight;
        public OceanConfiguration Config;

        private WaterSurface water;

        private Vector3 previousWindDir;
        private float previousLargeChaos;
        private float chaosVelocity;
        private float smoothTime = 0.5f;

        private void Awake()
        {
            water = GetComponent<WaterSurface>();
        }

        private void Update()
        {
            if (WeatherManager == null || WeatherManager.CurrentWeather == null || SunLight == null) return;

            var weather = WeatherManager.CurrentWeather;

            // 🔄 Normalisation
            float tWind = Mathf.InverseLerp(Config.windMin, Config.windMax, weather.WindSpeed);
            float tHumidity = Mathf.InverseLerp(Config.humidityMin, Config.humidityMax, weather.Humidity);
            float tTemp = Mathf.InverseLerp(Config.temperatureMin, Config.temperatureMax, weather.WaterTemperature);
            float tPressure = Mathf.InverseLerp(Config.pressureMin, Config.pressureMax, weather.AtmosphericPressure);
            float tTime = Mathf.InverseLerp(Config.timeMin, Config.timeMax, TimeHandlerData.CurrentTime);
            float sunIntensity01 = Mathf.Clamp01(SunLight.intensity / Config.sunIntensityMax);

            // 💨 Direction du vent
            Vector3 windDir = Quaternion.Euler(0, weather.WindOrientation, 0) * Vector3.forward;
            previousWindDir = Vector3.Lerp(previousWindDir, windDir, Time.deltaTime * 2f);
            float windAngle = Mathf.Atan2(previousWindDir.x, previousWindDir.z) * Mathf.Rad2Deg;


            float targetChaos = Mathf.Clamp01(Config.chaosBase + tWind * Config.chaosWindFactor + (1f - tPressure) * Config.chaosPressureFactor);

            water.largeBand0Multiplier = Mathf.Lerp(Config.band0Range.x, Config.band0Range.y, tWind);
            water.largeBand1Multiplier = Mathf.Lerp(Config.band1Range.x, Config.band1Range.y, tHumidity);

            water.ripplesFadeStart = Mathf.Lerp(Config.fadeStartRange.x, Config.fadeStartRange.y, tTemp);
            water.ripplesFadeDistance = Mathf.Lerp(Config.fadeDistanceRange.x, Config.fadeDistanceRange.y, tTemp);

            Color scatteringColor = Color.Lerp(Config.scatteringDark, Config.scatteringLight, tTime * sunIntensity01);
            scatteringColor = Color.Lerp(scatteringColor, Config.scatteringHazy, tHumidity * 0.7f);

            Color refractionColor = Color.Lerp(Config.refractionCold, Config.refractionWarm, tTemp);
            refractionColor = Color.Lerp(refractionColor, Color.gray, tHumidity * 0.4f);

            Color foamColor = Color.Lerp(Config.foamLowWind, Config.foamHighWind, tWind);

            float absorption = Mathf.Lerp(Config.absorptionRange.x, Config.absorptionRange.y, tTemp);

            water.ambientScattering = Mathf.Lerp(Config.ambientScatteringRange.x, Config.ambientScatteringRange.y, tHumidity + (1f - tPressure));
            water.heightScattering = Mathf.Lerp(Config.heightScatteringRange.x, Config.heightScatteringRange.y, tWind + tHumidity * 0.5f);
            water.displacementScattering = Mathf.Lerp(Config.displacementScatteringRange.x, Config.displacementScatteringRange.y, tWind + (1f - tPressure));


            //TU AS OUBLIE
            float largeChaos = Mathf.SmoothDamp(previousLargeChaos, targetChaos, ref chaosVelocity, smoothTime);
            previousLargeChaos = largeChaos;

            water.largeWindSpeed = weather.WindSpeed;
            water.largeOrientationValue = weather.WindOrientation;
            water.largeChaos = largeChaos;

            water.ripplesWindSpeed = Mathf.Clamp(weather.WindSpeed * 0.5f, 0f, Config.maxRipplesWind);
            water.ripplesOrientationValue = windAngle;
            water.ripplesChaos = Mathf.Clamp01(
                Config.ripplesChaosBase + tWind * Config.ripplesChaosWindFactor + tHumidity * Config.ripplesChaosHumidityFactor
            );
            water.ripplesFadeStart = Mathf.Lerp(Config.fadeStartRange.x, Config.fadeStartRange.y, tTemp);
            water.ripplesFadeDistance = Mathf.Lerp(Config.fadeDistanceRange.x, Config.fadeDistanceRange.y, tTemp);

            // ✅ Application finale
            water.scatteringColor = scatteringColor;
            water.refractionColor = refractionColor;
            water.foamColor = foamColor;
            water.absorptionDistance = absorption;
        }
    }

}
