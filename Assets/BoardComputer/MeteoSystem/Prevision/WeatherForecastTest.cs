using System;
using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using static WeatherForecast;

public class WeatherForecast : MonoBehaviour
{
    [System.Serializable]
    public struct DayWeathers
    {
        public int Day;
        public List<WeatherData> Weathers;

        public DayWeathers(int day, List<WeatherData> datas)
        {
            this.Day = day;
            this.Weathers = datas;
        }
    }
    [SerializeField] private GameSettings gameSettings;

    [Header("Weather Manager")]
    [SerializeField] private WeatherManager _weatherManager;
    [SerializeField] private DayNightManager _dayNightManager;

    [SerializeField] private CustomEvent_WeatherType _onWeatherGenerated;
    [SerializeField] private CustomEvent _eventMidNight;

    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI morningWeatherText;
    [SerializeField] private TextMeshProUGUI middayWeatherText;
    [SerializeField] private TextMeshProUGUI eveningWeatherText;

    [Header("Forecast Settings")]
    [SerializeField] private float morningStart = 6f;
    [SerializeField] private float middayStart = 12f;
    [SerializeField] private float eveningStart = 18f;

    private WeatherData morningWeather;
    private WeatherData middayWeather;
    private WeatherData eveningWeather;

    public int TargetDay;
    public int totalDays;

    public List<DayWeathers> dayWeathers = new List<DayWeathers>();

    private void Awake()
    {
        _onWeatherGenerated.handle += _onWeatherUpdate_handle;
        _eventMidNight.handle += _eventMidNight_handle;
        totalDays = gameSettings.TotalDays;
    }

    private void OnDestroy()
    {
        _onWeatherGenerated.handle -= _onWeatherUpdate_handle;
        _eventMidNight.handle -= _eventMidNight_handle;
    }

    private bool _isInitialized = false;

    private void _eventMidNight_handle()
    {
        CalculateWeatherInSeconds();
    }

    private void _onWeatherUpdate_handle(WeatherType obj)
    {
        if (!_isInitialized)
        {
            CalculateMeteoPerDay();
            
            _isInitialized = true;
            CalculateWeatherInSeconds();
        }
        //CalculateWeatherInSeconds(3);
    }

/*    private void CalculateWeatherInSeconds(int targetDay)
    {
        // Prendre en compte les prévisions du jour cible
        DayWeathers targetDayWeather = dayWeathers.Find(dayWeather => dayWeather.Day == targetDay);

        float timeRemainingMorning = gameSettings.DayCycleDuration.Seconds - _dayNightManager.TimeUntil(morningStart);
        float timeRemainingMidday = gameSettings.DayCycleDuration.Seconds - _dayNightManager.TimeUntil(middayStart);
        float timeRemainingEvening = gameSettings.DayCycleDuration.Seconds - _dayNightManager.TimeUntil(eveningStart);

        morningWeather = PredictWeatherToTimeForDay(targetDayWeather, timeRemainingMorning);
        middayWeather = PredictWeatherToTimeForDay(targetDayWeather, timeRemainingMidday);
        eveningWeather = PredictWeatherToTimeForDay(targetDayWeather, timeRemainingEvening);

        UpdateWeatherUI();
    }

    private WeatherData PredictWeatherToTimeForDay(DayWeathers dayWeather, float timeRemaining)
    {
        WeatherData weather = new WeatherData();
        WeatherData currentWeather = new WeatherData();  // Commence avec la première météo
        currentWeather = dayWeather.Weathers[0];  // Commence avec la première météo
        WeatherData nextWeather = dayWeather.Weathers.Count > 1 ? dayWeather.Weathers[1] : currentWeather;

        float currentTime = 0f;
        foreach (WeatherData weatherData in dayWeather.Weathers)
        {
            if (currentTime + weatherData.weatherDuration > timeRemaining)
            {
                // Si la météo actuelle couvre le temps restant
                weather = PredictWeatherFromWeatherData(weatherData, timeRemaining - currentTime);
                break;
            }
            currentTime += weatherData.weatherDuration;
        }

        return weather;
    }


    public void CalculateMeteoPerDay()
    {
        dayWeathers = new List<DayWeathers>();
        WeatherData currentWeather = _weatherManager.currentWeather;
        int totalSeconds = (int)_weatherManager._totalWeatherDuration; // total duration of all weather conditions in seconds
        int currentDay = 0;
        List<WeatherData> storage = new List<WeatherData>();

        for (int i = 0; i < _weatherManager.weatherForecast.Count; i++)
        {
            var dayWeatherData = new DayWeathers();
            dayWeatherData.Day = i + 1; // jour actuel (1, 2, 3, etc.)
            dayWeatherData.Weathers = new List<WeatherData>();

            float calc = 0f;
            if (storage.Count > 0)
            {
                for (int j = 0; j < storage.Count; j++)
                {
                    calc += storage[j].weatherDuration;
                    dayWeatherData.Weathers.Add(storage[j]);
                }
                storage.Clear();
            }

            calc += _weatherManager.weatherForecast[i].weatherDuration;


            //if météo plus longue que la journée
            if (calc >= gameSettings.DayCycleDuration.Seconds)
            {
                float diff = Mathf.Abs(calc - gameSettings.DayCycleDuration.Seconds);
                var copy = _weatherManager.weatherForecast[i];
                copy.weatherDuration = diff;
                storage.Add(copy);
                dayWeatherData.Weathers.Add(_weatherManager.weatherForecast[i]);
            }
            else
            {
                float diff = Mathf.Abs(calc - gameSettings.DayCycleDuration.Seconds);
                var copy = _weatherManager.weatherForecast[i];
                copy.weatherDuration = diff;
                storage.Add(copy);
                dayWeatherData.Weathers.Add(_weatherManager.weatherForecast[i]);
            }

            dayWeathers.Add(dayWeatherData);
        }
    }*/

/*    private void CalculateWeatherInSeconds(int targetDay)
    {
        DayWeathers targetDayWeather = dayWeathers[targetDay - 1];

        float timeRemainingMorning = gameSettings.DayCycleDuration.Seconds - _dayNightManager.TimeUntil(morningStart);
        float timeRemainingMidday = gameSettings.DayCycleDuration.Seconds - _dayNightManager.TimeUntil(middayStart);
        float timeRemainingEvening = gameSettings.DayCycleDuration.Seconds - _dayNightManager.TimeUntil(eveningStart);

        morningWeather = PredictWeatherToTime(targetDayWeather, timeRemainingMorning);
        middayWeather = PredictWeatherToTime(targetDayWeather, timeRemainingMidday);
        eveningWeather = PredictWeatherToTime(targetDayWeather, timeRemainingEvening);

        UpdateWeatherUI();
    }*/

/*    private WeatherData PredictWeatherToTime(DayWeathers dayWeather, float timeRemaining)
    {
        WeatherData weather = new WeatherData();

        if (dayWeather.Weathers.Count > 1)
        {
            //si on suppose qu'il y'a plus de un alors on compare et on fais comme avec l'autre
        }

        float currentTime = 0f;
        foreach (WeatherData weatherData in dayWeather.Weathers)
        {
            if (currentTime + weatherData.weatherDuration > timeRemaining)
            {
                // If the current weather condition ends after the specified time, predict the weather
                weather = PredictWeatherFromWeatherData(weatherData, timeRemaining - currentTime);
                break;
            }
            currentTime += weatherData.weatherDuration;
        }

        return weather;
    }*/

/*    private WeatherData PredictWeatherFromWeatherData(WeatherData currentWeather, float timeRemaining)
    {
        WeatherData weather = new WeatherData();

        // Humidity
        weather.humidity = GetLerpedFloatAtTime(timeRemaining, currentWeather.humidity, currentWeather.humidity, currentWeather.weatherDuration);

        // Temperatures
        weather.airTemperature = GetLerpedFloatAtTime(timeRemaining, currentWeather.airTemperature, currentWeather.airTemperature, currentWeather.weatherDuration);
        weather.waterTemperature = GetLerpedFloatAtTime(timeRemaining, currentWeather.waterTemperature, currentWeather.waterTemperature, currentWeather.weatherDuration);

        // Atmospheric pressure
        weather.atmosphericPressure = GetLerpedFloatAtTime(timeRemaining, currentWeather.atmosphericPressure, currentWeather.atmosphericPressure, currentWeather.weatherDuration);

        // Wind
        weather.windSpeed = GetLerpedFloatAtTime(timeRemaining, currentWeather.windSpeed, currentWeather.windSpeed, currentWeather.weatherDuration);
        weather.windOrientationValue = GetLerpedFloatAtTime(timeRemaining, currentWeather.windOrientationValue, currentWeather.windOrientationValue, currentWeather.weatherDuration);
        weather.windDirection = _weatherManager.DetermineWindDirection(weather.windOrientationValue);

        // Type
        weather.weatherType = _weatherManager.DetermineWeatherType(weather);

        return weather;
    }*/

    private void CalculateWeatherInSeconds()
    {
        float timeRemainingMorning = _weatherManager.currentWeather.weatherDuration - _dayNightManager.TimeUntil(morningStart);
        float timeRemainingMidday = _weatherManager.currentWeather.weatherDuration - _dayNightManager.TimeUntil(middayStart);
        float timeRemainingEvening = _weatherManager.currentWeather.weatherDuration - _dayNightManager.TimeUntil(eveningStart);

        morningWeather = PredictWeatherToTime(timeRemainingMorning);
        middayWeather = PredictWeatherToTime(timeRemainingMidday);
        eveningWeather = PredictWeatherToTime(timeRemainingEvening);

        UpdateWeatherUI();
    }

    private WeatherData PredictWeatherToTime(float timeRemaining)
    {
        WeatherData weather = new WeatherData();

        WeatherData currentWeather = _weatherManager.currentWeather;
        WeatherData nextWeather = _weatherManager.nextWeather;

        // Ajuster en fonction des durées courtes
        if (timeRemaining < 0f)
        {
            float overflow = Mathf.Abs(timeRemaining);
            currentWeather = nextWeather;

            // Prendre en compte les météos suivantes si overflow est plus grand
            if (_weatherManager.indexWeather + 2 < _weatherManager.weatherForecast.Count)
            {
                nextWeather = _weatherManager.weatherForecast[_weatherManager.indexWeather + 2];
            }
            else
            {
                nextWeather = _weatherManager.weatherForecast[0];  // Revenir à la première prévision si fin du cycle
            }

            // Mettre à jour timeRemaining avec l'overflow pour la prochaine météo
            timeRemaining = currentWeather.weatherDuration - overflow;
        }

        // Humidity
        weather.humidity = GetLerpedFloatAtTime(timeRemaining, currentWeather.humidity, nextWeather.humidity, currentWeather.weatherDuration);

        // Temperatures
        weather.airTemperature = GetLerpedFloatAtTime(timeRemaining, currentWeather.airTemperature, nextWeather.airTemperature, currentWeather.weatherDuration);
        weather.waterTemperature = GetLerpedFloatAtTime(timeRemaining, currentWeather.waterTemperature, nextWeather.waterTemperature, currentWeather.weatherDuration);

        // Atmospheric pressure
        weather.atmosphericPressure = GetLerpedFloatAtTime(timeRemaining, currentWeather.atmosphericPressure, nextWeather.atmosphericPressure, currentWeather.weatherDuration);

        // Wind
        weather.windSpeed = GetLerpedFloatAtTime(timeRemaining, currentWeather.windSpeed, nextWeather.windSpeed, currentWeather.weatherDuration);
        weather.windOrientationValue = GetLerpedFloatAtTime(timeRemaining, currentWeather.windOrientationValue, nextWeather.windOrientationValue, currentWeather.weatherDuration);
        weather.windDirection = _weatherManager.DetermineWindDirection(weather.windOrientationValue);

        // Type (Ne pas interpoler le type de météo)
        weather.weatherType = currentWeather.weatherType;

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

    private string FormatWeather(WeatherData weather, string period)
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
