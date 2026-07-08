using LightHouse.Features.TimeOfDay.TimeCore;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

namespace LightHouse.Features.Weather.Ocean
{
    /// <summary>
    /// L'océan manager se base pour l'instant sur un Global Setting
    /// Souvent entre minimum et maximum e.g WindMin et WindMax et se basera par rapport au CurrentWind
    /// situé dans le WeatherHandlerData
    /// </summary>
    [RequireComponent(typeof(WaterSurface))]
    public class OceanManager : NotPersistentSingleton<OceanManager>
    {
        public WeatherManager WeatherManager;
        public Light SunLight;
        public OceanConfiguration Config;

        [SerializeField] private WaterSurface _waterSurface;
        public WaterSurface WaterSurfaceComponent => _waterSurface;

        private Vector3 previousWindDir;
        private float previousLargeChaos;
        private float chaosVelocity;
        private float smoothTime = 0.5f;

        protected override void Awake()
        {
            base.Awake();
            Debug.Log(Instance);
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

            _waterSurface.largeBand0Multiplier = Mathf.Lerp(Config.band0Range.x, Config.band0Range.y, tWind);
            _waterSurface.largeBand1Multiplier = Mathf.Lerp(Config.band1Range.x, Config.band1Range.y, tHumidity);

            _waterSurface.ripplesFadeStart = Mathf.Lerp(Config.fadeStartRange.x, Config.fadeStartRange.y, tTemp);
            _waterSurface.ripplesFadeDistance = Mathf.Lerp(Config.fadeDistanceRange.x, Config.fadeDistanceRange.y, tTemp);

            Color scatteringColor = Color.Lerp(Config.scatteringDark, Config.scatteringLight, tTime * sunIntensity01);
            scatteringColor = Color.Lerp(scatteringColor, Config.scatteringHazy, tHumidity * 0.7f);

            Color refractionColor = Color.Lerp(Config.refractionCold, Config.refractionWarm, tTemp);
            refractionColor = Color.Lerp(refractionColor, Color.gray, tHumidity * 0.4f);

            Color foamColor = Color.Lerp(Config.foamLowWind, Config.foamHighWind, tWind);

            float absorption = Mathf.Lerp(Config.absorptionRange.x, Config.absorptionRange.y, tTemp);

            _waterSurface.ambientScattering = Mathf.Lerp(Config.ambientScatteringRange.x, Config.ambientScatteringRange.y, tHumidity + (1f - tPressure));
            _waterSurface.heightScattering = Mathf.Lerp(Config.heightScatteringRange.x, Config.heightScatteringRange.y, tWind + tHumidity * 0.5f);
            _waterSurface.displacementScattering = Mathf.Lerp(Config.displacementScatteringRange.x, Config.displacementScatteringRange.y, tWind + (1f - tPressure));


            //TU AS OUBLIE
            float largeChaos = Mathf.SmoothDamp(previousLargeChaos, targetChaos, ref chaosVelocity, smoothTime);
            previousLargeChaos = largeChaos;

            _waterSurface.largeWindSpeed = weather.WindSpeed;
            _waterSurface.largeOrientationValue = weather.WindOrientation;
            _waterSurface.largeChaos = largeChaos;

            _waterSurface.ripplesWindSpeed = Mathf.Clamp(weather.WindSpeed * 0.5f, 0f, Config.maxRipplesWind);
            _waterSurface.ripplesOrientationValue = windAngle;
            _waterSurface.ripplesChaos = Mathf.Clamp01(
                Config.ripplesChaosBase + tWind * Config.ripplesChaosWindFactor + tHumidity * Config.ripplesChaosHumidityFactor
            );
            _waterSurface.ripplesFadeStart = Mathf.Lerp(Config.fadeStartRange.x, Config.fadeStartRange.y, tTemp);
            _waterSurface.ripplesFadeDistance = Mathf.Lerp(Config.fadeDistanceRange.x, Config.fadeDistanceRange.y, tTemp);

            // ✅ Application finale
            _waterSurface.scatteringColor = scatteringColor;
            _waterSurface.refractionColor = refractionColor;
            _waterSurface.foamColor = foamColor;
            _waterSurface.absorptionDistance = absorption;
        }
    }

}
