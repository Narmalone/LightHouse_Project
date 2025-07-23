using LightHouse.Weather.Utils;
using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace LightHouse.Weather
{
    [CreateAssetMenu(fileName = "Generator_Default", menuName = "LightHouse/Weather/New Generator")]
    public class WeatherGenerator : ScriptableObject
    {
        public TimeConfiguration TimeConfig;
        public WeatherTimeline Timeline;
        public WeatherConfigDatabase WeatherDefinitions;
        public float MinWeathersDuration;
        public float MaxWeathersDuration;

        public event Action OnWeatherGenerated;

        public void FillTimeline(float minDuration, float maxDuration)
        {
            Timeline.Weathers.Clear(); // Évite d'empiler d'anciennes valeurs

            float current = 0f;
            float totalTime = TimeConfig.GetTotalGameTimeInSeconds();

            while (current < totalTime)
            {
                float duration = Random.Range(minDuration, maxDuration);

                if (current + duration > totalTime)
                {
                    duration = totalTime - current;
                }

                var chosenType = (WeatherType)Random.Range(0, System.Enum.GetValues(typeof(WeatherType)).Length);
                var typeParams = WeatherDefinitions.GetDefinition(chosenType);

                float randomOrientation = Random.Range(0f, 360f);

                var evt = new WeatherData
                {
                    WeatherType = chosenType,
                    StartTimeInSeconds = current,
                    DurationInSeconds = duration,
                    Humidity = Random.Range(typeParams.HumidityRange.x, typeParams.HumidityRange.y),
                    AtmosphericPressure = Random.Range(typeParams.PressureRange.x, typeParams.PressureRange.y),
                    WindSpeed = Random.Range(typeParams.WindSpeedRange.x, typeParams.WindSpeedRange.y),
                    AirTemperature = Random.Range(typeParams.AirTemperatureRange.x, typeParams.AirTemperatureRange.y),
                    WaterTemperature = Random.Range(typeParams.WaterTemperatureRange.x, typeParams.WaterTemperatureRange.y),
                    WindOrientation = randomOrientation,
                    WindOrientationType = WeatherUtils.AngleToOrientationType(randomOrientation)
                };

                Timeline.Weathers.Add(evt);
                current += duration;
            }
            OnWeatherGenerated?.Invoke();
        }
    }
}
