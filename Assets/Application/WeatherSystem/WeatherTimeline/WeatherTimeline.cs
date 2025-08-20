using System;
using System.Collections.Generic;
using UnityEngine;
using LightHouse.Weather.Utils;

namespace LightHouse.Weather
{
    /// <summary>
    /// Ligne de temps météo pour toute la durée du jeu (enchaînement de segments).
    /// </summary>
    [CreateAssetMenu(fileName = "WeatherTimeline_", menuName = "LightHouse/WeatherSystem/New Timeline")]
    public class WeatherTimeline : ScriptableObject
    {
        #region Data

        public List<WeatherData> Weathers = new();

        /// <summary>Appelé après régénération complète de la timeline.</summary>
        public static Action OnWeatherGenerated { get; set; }

        #endregion

        #region Generation

        /// <param name="totalTime">Durée totale du jeu (secondes).</param>
        public void GenerateTimeline(WeatherConfigDatabase database, float totalTime)
        {
            Weathers.Clear();

            float t = 0f;
            while (t < totalTime)
            {
                var type = (WeatherType)UnityEngine.Random.Range(0, Enum.GetValues(typeof(WeatherType)).Length);
                var def = database.GetDefinition(type);

                float duration = UnityEngine.Random.Range(def.MinWeatherDuration, def.MaxWeatherDuration);
                if (duration <= 0f) duration = 1f; // garde-fou

                if (t + duration > totalTime)
                    duration = totalTime - t;

                float orientation = UnityEngine.Random.Range(0f, 360f);

                Weathers.Add(new WeatherData
                {
                    WeatherType = type,
                    StartTimeInSeconds = t,
                    DurationInSeconds = duration,
                    Humidity = UnityEngine.Random.Range(def.HumidityRange.x, def.HumidityRange.y),
                    AtmosphericPressure = UnityEngine.Random.Range(def.PressureRange.x, def.PressureRange.y),
                    WindSpeed = UnityEngine.Random.Range(def.WindSpeedRange.x, def.WindSpeedRange.y),
                    AirTemperature = UnityEngine.Random.Range(def.AirTemperatureRange.x, def.AirTemperatureRange.y),
                    WaterTemperature = UnityEngine.Random.Range(def.WaterTemperatureRange.x, def.WaterTemperatureRange.y),
                    WindOrientation = WeatherUtils.NormalizeAngle360(orientation),
                    WindOrientationType = WeatherUtils.AngleToOrientationType(orientation)
                });

                t += duration;
            }

            OnWeatherGenerated?.Invoke();
        }

        #endregion
    }
}
