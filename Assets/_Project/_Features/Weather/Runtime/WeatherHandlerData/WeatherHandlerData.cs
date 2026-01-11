using System;
using UnityEngine;

namespace LightHouse.Features.Weather
{
    public static class WeatherHandlerData
    {
        /// <summary>
        /// Changement non pas lorsqu'une météo est complétée mais lorsqu'on a passée plus de la moitié
        /// de la première météo vers l'autre
        /// </summary>
        public static Action<WeatherType> OnWeatherTypeChanged { get; set; }
        public static WeatherData CurrentWeather { get; private set; }

        public static void SetCurrentWeatherDatas(WeatherData weatherData)
        {
            CurrentWeather = weatherData;
        }
    }

}
