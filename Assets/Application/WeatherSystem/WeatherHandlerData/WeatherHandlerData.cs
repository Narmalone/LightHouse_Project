using UnityEngine;

namespace LightHouse.Weather
{
    public static class WeatherHandlerData
    {
        public static WeatherData CurrentWeather { get; private set; }

        public static void SetCurrentWeatherDatas(WeatherData weatherData)
        {
            CurrentWeather = weatherData;
        }
    }

}
