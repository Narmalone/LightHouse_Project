using LightHouse.Weather.Utils;
using System.Collections.Generic;
using UnityEngine;

namespace LightHouse.Weather
{
    public class WeatherForecast : MonoBehaviour
    {
        public TimeConfiguration TimeConfig;
        public WeatherTimeline WeatherGenerator;

        public List<WeatherData> MorningsDatas = new List<WeatherData>();
        public List<WeatherData> MiddaysDatas = new List<WeatherData>();
        public List<WeatherData> EveningDatas = new List<WeatherData>();
        public List<WeatherData> MiddnightDatas = new List<WeatherData>();

        private void Awake()
        {
            WeatherTimeline.OnWeatherGenerated += WeatherGenerator_OnWeatherGenerated;
        }

        private void OnDestroy()
        {
            WeatherTimeline.OnWeatherGenerated -= WeatherGenerator_OnWeatherGenerated;
        }

        private void WeatherGenerator_OnWeatherGenerated()
        {
            MorningsDatas.Clear();
            MiddaysDatas.Clear();
            EveningDatas.Clear();
            MiddnightDatas.Clear();

            int totalDays = TimeConfig.TotalDays;

            for (byte day = 0; day < totalDays; day++)
            {
                var timeline = WeatherGenerator;
                var config = TimeConfig;

                MorningsDatas.Add(WeatherUtils.GetWeatherAt(day, 6f, timeline, config));
                MiddaysDatas.Add(WeatherUtils.GetWeatherAt(day, 12f, timeline, config));
                EveningDatas.Add(WeatherUtils.GetWeatherAt(day, 18f, timeline, config));
                MiddnightDatas.Add(WeatherUtils.GetWeatherAt(day, 0f, timeline, config));
            }

            Debug.Log("✅ Prévisions météo enregistrées pour chaque tranche horaire !");
        }

    }
}
