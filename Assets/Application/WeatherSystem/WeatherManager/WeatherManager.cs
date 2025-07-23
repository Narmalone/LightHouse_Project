using LightHouse.Game.DayNightSystem;
using LightHouse.Weather.Utils;
using UnityEngine;

namespace LightHouse.Weather
{
    public class WeatherManager : MonoBehaviour
    {
        public WeatherGenerator WeatherGenerator;
        public WeatherTimeline WeatherTime;
        public TimeConfiguration TimeConfig;

        public WeatherData CurrentWeather;
        private WeatherData fromWeather;
        private WeatherData toWeather;

        private WeatherType _previousWeatherType;
        private WeatherType _currentWeatherType;

        public WeatherData FromWeather => fromWeather;
        public WeatherData ToWeather => toWeather;

        private float currentGameSeconds;
        private float fromStart;
        public float CurrentBlend => Mathf.Clamp01((currentGameSeconds - fromStart) / fromWeather.DurationInSeconds);

        private int currentIndex = 0;
        private void Awake()
        {
            WeatherGenerator.FillTimeline(WeatherGenerator.MinWeathersDuration, WeatherGenerator.MaxWeathersDuration);
        }

        private void Start()
        {
            if (WeatherTime.Weathers.Count < 2)
            {
                Debug.LogError("Pas assez d'événements météo pour interpoler !");
                return;
            }

            // Initialisation
            fromWeather = WeatherTime.Weathers[0];
            toWeather = WeatherTime.Weathers[1];
        }

        private void Update()
        {
            float secondsPerDay = TimeConfig.GetTotalSecondsPerDay();
            currentGameSeconds = TimeHandlerData.CurrentDay * secondsPerDay + (TimeHandlerData.CurrentTime / 24f) * secondsPerDay;

            fromStart = fromWeather.StartTimeInSeconds;
            float fromEnd = fromStart + fromWeather.DurationInSeconds;

            // Passe à la météo suivante si on a dépassé la durée
            if (currentGameSeconds > fromEnd && currentIndex < WeatherTime.Weathers.Count - 2)
            {
                currentIndex++;
                fromWeather = WeatherTime.Weathers[currentIndex];
                toWeather = WeatherTime.Weathers[currentIndex + 1];
            }

            // Interpolation
            float localTime = currentGameSeconds - fromWeather.StartTimeInSeconds;
            float t = Mathf.Clamp01(localTime / fromWeather.DurationInSeconds);
            _previousWeatherType = CurrentWeather.WeatherType;
            CurrentWeather = WeatherUtils.LerpWeatherData(fromWeather, toWeather, t);
            _currentWeatherType = CurrentWeather.WeatherType;

            if(_previousWeatherType != _currentWeatherType)
            {
                WeatherHandlerData.OnWeatherTypeChanged?.Invoke(_currentWeatherType);
            }
            WeatherHandlerData.SetCurrentWeatherDatas(CurrentWeather);
        }
    }
}

