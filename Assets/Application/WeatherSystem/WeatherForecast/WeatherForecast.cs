using System.Collections.Generic;
using UnityEngine;

public class WeatherForecast : MonoBehaviour
{
    public WeatherGenerator WeatherGenerator;

    public List<WeatherData> MorningsDatas = new List<WeatherData>();
    public List<WeatherData> MiddaysDatas = new List<WeatherData>();
    public List<WeatherData> EveningDatas = new List<WeatherData>();
    public List<WeatherData> MiddnightDatas = new List<WeatherData>();

    private void Awake()
    {
        WeatherGenerator.OnWeatherGenerated += WeatherGenerator_OnWeatherGenerated;
    }

    private void OnDestroy()
    {
        WeatherGenerator.OnWeatherGenerated -= WeatherGenerator_OnWeatherGenerated;
    }

    private void WeatherGenerator_OnWeatherGenerated()
    {
        MorningsDatas.Clear();
        MiddaysDatas.Clear();
        EveningDatas.Clear();
        MiddnightDatas.Clear();

        int totalDays = WeatherGenerator.TimeConfig.TotalDays;

        for (byte day = 0; day < totalDays; day++)
        {
            var timeline = WeatherGenerator.Timeline;
            var config = WeatherGenerator.TimeConfig;

            MorningsDatas.Add(WeatherUtils.GetWeatherAt(day, 6f, timeline, config));
            MiddaysDatas.Add(WeatherUtils.GetWeatherAt(day, 12f, timeline, config));
            EveningDatas.Add(WeatherUtils.GetWeatherAt(day, 18f, timeline, config));
            MiddnightDatas.Add(WeatherUtils.GetWeatherAt(day, 0f, timeline, config));
        }

        Debug.Log("✅ Prévisions météo enregistrées pour chaque tranche horaire !");
    }

}
