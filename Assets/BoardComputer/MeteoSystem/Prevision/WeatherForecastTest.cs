using System;
using UnityEngine;
using TMPro;

public class WeatherForecast : MonoBehaviour
{
    [Header("Weather Manager")]
    [SerializeField] private WeatherManager _weatherManager;
    [SerializeField] private DayNightManager _dayNightManager;

    [SerializeField] private CustomEvent_WeatherType _onWeatherGenerated;

    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI morningWeatherText;
    [SerializeField] private TextMeshProUGUI middayWeatherText;
    [SerializeField] private TextMeshProUGUI eveningWeatherText;

    [Header("Forecast Settings")]
    [SerializeField] private float morningStart = 6f;
    [SerializeField] private float middayStart = 12f;
    [SerializeField] private float eveningStart = 18f;

    private DayWeather morningWeather;
    private DayWeather middayWeather;
    private DayWeather eveningWeather;

    public int TargetDay;

    private void Awake()
    {
        _onWeatherGenerated.handle += _onWeatherUpdate_handle;
    }

    private void OnDestroy()
    {
        _onWeatherGenerated.handle -= _onWeatherUpdate_handle;
    }

    private void _onWeatherUpdate_handle(WeatherType obj)
    {
        CalculateWeatherInSeconds(TargetDay);
    }

    private void CalculateWeatherInSeconds(int day)
    {
        float timeRemainingMorning = _weatherManager.currentWeather.weatherDuration - _dayNightManager.TimeUntil(morningStart);
        float timeRemainingMidday = _weatherManager.currentWeather.weatherDuration - _dayNightManager.TimeUntil(middayStart);
        float timeRemainingEvening = _weatherManager.currentWeather.weatherDuration - _dayNightManager.TimeUntil(eveningStart);

        morningWeather = PredictWeatherToTime(timeRemainingMorning);
        middayWeather = PredictWeatherToTime(timeRemainingMidday);
        eveningWeather = PredictWeatherToTime(timeRemainingEvening);

        UpdateWeatherUI();
    }

    private DayWeather PredictWeatherToTime(float timeRemaining)
    {
        DayWeather weather = new DayWeather();

        DayWeather currentWeather = _weatherManager.currentWeather;
        DayWeather nextWeather = _weatherManager.nextWeather;

        //TO DO:: SECURITY IN CASE THERE IS NO +2 WEATHER AND OVERGENERATE SOME ?
        if (timeRemaining < 0f)
        {
            currentWeather = _weatherManager.nextWeather;
            nextWeather = _weatherManager.weatherForecast[_weatherManager.indexWeather + 2]; //+2 cause skip current & next to grab 2
        }

        //Humidity
        weather.humidity = GetLerpedFloatAtTime(timeRemaining, currentWeather.humidity, nextWeather.humidity, currentWeather.weatherDuration);

        //Temperatures
        weather.airTemperature = GetLerpedFloatAtTime(timeRemaining, currentWeather.airTemperature, nextWeather.airTemperature, currentWeather.weatherDuration);
        weather.waterTemperature = GetLerpedFloatAtTime(timeRemaining, currentWeather.waterTemperature, nextWeather.waterTemperature, currentWeather.weatherDuration);

        //Atmospheric pressure
        weather.atmosphericPressure = GetLerpedFloatAtTime(timeRemaining, currentWeather.atmosphericPressure, nextWeather.atmosphericPressure, currentWeather.weatherDuration);

        //Wind
        weather.windSpeed = GetLerpedFloatAtTime(timeRemaining, currentWeather.windSpeed, nextWeather.windSpeed, currentWeather.weatherDuration);
        weather.windOrientationValue = GetLerpedFloatAtTime(timeRemaining, currentWeather.windOrientationValue, nextWeather.windOrientationValue, currentWeather.weatherDuration);
        weather.windDirection = _weatherManager.DetermineWindDirection(weather.windOrientationValue);

        //Type
        weather.weatherType = _weatherManager.DetermineWeatherType(weather);

        return weather;
    }

    //Start + (end - start) * (time / duration)
    private float GetLerpedFloatAtTime(float timeRemaining, float startValue, float endValue, float totalDuration)
    {
        return startValue + (endValue - startValue) * (1 - (timeRemaining / totalDuration));
    }

    private void Update()
    {
        //Debug.Log(_dayNightManager.TimeUntil(12f));
        
    }

   
    private void UpdateWeatherUI()
    {
        // Afficher les prévisions dans l'UI
        morningWeatherText.text = FormatWeather(morningWeather, "Matin");
        middayWeatherText.text = FormatWeather(middayWeather, "Midi");
        eveningWeatherText.text = FormatWeather(eveningWeather, "Soir");
    }

    private string FormatWeather(DayWeather weather, string period)
    {
        // Formatage des données météo pour l'affichage
        return $"{period}: {weather.weatherType}\n" +
               $"Température de l'air: {weather.airTemperature}°C\n" +
               $"Température de l'eau: {weather.waterTemperature}°C\n" +
               $"Humidité: {weather.humidity}%\n" +
               $"Pression atmosphérique: {weather.atmosphericPressure} hPa\n" +
               $"Vitesse du vent: {weather.windSpeed} km/h\n" +
               $"Direction du vent: {weather.windDirection}";
    }
}
