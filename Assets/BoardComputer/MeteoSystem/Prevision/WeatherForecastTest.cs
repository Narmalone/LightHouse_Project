using System;
using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class WeatherForecast : MonoBehaviour
{
    [System.Serializable]
    public struct DayWeather
    {
        public int dayNumber;
        public List<WeatherData> weatherEvents;

        public DayWeather(int dayNumber)
        {
            this.dayNumber = dayNumber;
            this.weatherEvents = new List<WeatherData>();
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
    private bool _isInitialized = false;

    public List<DayWeather> dayWeathers = new List<DayWeather>();

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


    private void _eventMidNight_handle()
    {
        
    }

    private void _onWeatherUpdate_handle(WeatherType obj)
    {
        if (!_isInitialized)
        {
            // Exemple de prévisions météo (weatherForecast serait une liste de WeatherData)
            List<WeatherData> weatherForecast = _weatherManager.weatherForecast;

            // Calculer les météos pour chaque jour
            dayWeathers = CalculateWeatherForDays(weatherForecast, gameSettings.DayCycleDuration.Seconds);

            /*WeatherData morningWeather = GetWeatherAtTime(8f, _dayNightManager._homeTime);
            Debug.Log($"Météo à 8h: Humidité = {morningWeather.humidity}%");

            // Obtenir la météo à 12h (midi)
            WeatherData middayWeather = GetWeatherAtTime(12f, _dayNightManager._homeTime);
            Debug.Log($"Météo à 12h: Humidité = {middayWeather.humidity}%");

            // Obtenir la météo à 15h
            WeatherData afternoonWeather = GetWeatherAtTime(18f, _dayNightManager._homeTime);
            Debug.Log($"Météo à 15h: Humidité = {afternoonWeather.humidity}%");*/

            CalculateWeatherInSeconds();
            _isInitialized = true;
        }
    }

    private List<DayWeather> CalculateWeatherForDays(List<WeatherData> weatherForecast, float dayDuration)
    {
        List<DayWeather> days = new List<DayWeather>();
        float remainingTimeInDay = dayDuration;
        int currentDay = 1;

        DayWeather currentDayWeather = new DayWeather(currentDay);
        float totalWeatherDuration = 0f; // Garder une trace de la durée totale de la météo pour la journée

        for (int i = 0; i < weatherForecast.Count; i++)
        {
            WeatherData weather = weatherForecast[i];
            float remainingWeatherDuration = weather.weatherInitialDuration;

            // Trouver la météo suivante pour remplacer les valeurs en cas de division de la météo
            WeatherData nextWeather = (i + 1 < weatherForecast.Count) ? weatherForecast[i + 1] : weatherForecast[0];

            while (remainingWeatherDuration > 0f)
            {
                // Calculer la durée qui peut être ajoutée à la journée en cours
                float durationToAdd = Mathf.Min(remainingWeatherDuration, remainingTimeInDay);

                // Créer un nouvel objet WeatherData avec la durée calculée
                WeatherData weatherToAdd = new WeatherData
                {
                    humidity = weather.humidity,
                    windSpeed = weather.windSpeed,
                    airTemperature = weather.airTemperature,
                    windOrientationValue = weather.windOrientationValue,
                    windDirection = weather.windDirection,
                    waterTemperature = weather.waterTemperature,
                    atmosphericPressure = weather.atmosphericPressure,
                    weatherClampedDuration = durationToAdd,
                    weatherInitialDuration = weather.weatherInitialDuration,
                    weatherType = weather.weatherType
                };

                currentDayWeather.weatherEvents.Add(weatherToAdd);

                remainingTimeInDay -= durationToAdd;
                totalWeatherDuration += durationToAdd; // Mettre à jour la durée totale de la météo

                // Si la durée totale atteint celle de la journée, terminer la journée en cours
                if (totalWeatherDuration >= dayDuration)
                {
                    days.Add(currentDayWeather);
                    currentDay++;
                    currentDayWeather = new DayWeather(currentDay);
                    remainingTimeInDay = dayDuration;
                    totalWeatherDuration = 0f; // Réinitialiser la durée totale pour le nouveau jour
                }

                // Réduire la durée restante de la météo
                remainingWeatherDuration -= durationToAdd;
            }
        }

        // Ajout de la dernière journée si elle contient des événements météo
        if (currentDayWeather.weatherEvents.Count > 0)
        {
            days.Add(currentDayWeather);
        }

        // Deuxième phase : ajustement des météos pour les jours avec une seule météo de 400s
        for (int j = 0; j < days.Count - 1; j++) // On boucle jusqu'à l'avant-dernier jour
        {
            DayWeather currentDayW = days[j];
            DayWeather nextDay = days[j + 1];

            // Vérifier si le jour actuel a une seule météo et que sa durée est de 400s
            if (currentDayW.weatherEvents.Count == 1 && currentDayW.weatherEvents[0].weatherClampedDuration == dayDuration)
            {
                // Vérifier si le jour suivant a plus d'une météo
                if (nextDay.weatherEvents.Count > 1)
                {
                    // Remplacer les valeurs de la première météo du jour suivant par celles de la deuxième, sauf la durée
                    WeatherData firstWeather = nextDay.weatherEvents[0];
                    WeatherData secondWeather = nextDay.weatherEvents[1];

                    firstWeather.humidity = secondWeather.humidity;
                    firstWeather.windSpeed = secondWeather.windSpeed;
                    firstWeather.airTemperature = secondWeather.airTemperature;
                    firstWeather.windOrientationValue = secondWeather.windOrientationValue;
                    firstWeather.windDirection = secondWeather.windDirection;
                    firstWeather.waterTemperature = secondWeather.waterTemperature;
                    firstWeather.atmosphericPressure = secondWeather.atmosphericPressure;
                    firstWeather.weatherType = secondWeather.weatherType;

                    // Garder la durée originale
                    firstWeather.weatherClampedDuration = nextDay.weatherEvents[0].weatherClampedDuration;
                    firstWeather.weatherInitialDuration = nextDay.weatherEvents[0].weatherInitialDuration;

                    nextDay.weatherEvents[0] = firstWeather;
                }
            }
        }

        return days;
    }

    public void GetWeatherTarget(int day)
    {
        DayWeather dayWeather = dayWeathers.Find(x => x.dayNumber == day);
        DayWeather target = new DayWeather();

        if(dayWeathers.Count == 1)
        {
            //regarder le prochain jour
            var nextDay = dayWeathers[day + 1];
        }
        //On a 400s donc la durée totale de une journée
    }

    private void Update()
    {
        Debug.Log(_dayNightManager.TimeUntil(8f));
        //donc par exemple 400 c'est 133.3 donc toutes les 133.3s on choppe les valeurs
        //Debug.Log(_dayNightManager.TimeTo(0f, 8f));

        //en gros au start on calcule depuis home time to 8f par ex pour le matin, puis on fais décalage de 400s à chaque fois
        //
        Debug.Log(_dayNightManager.TimeTo(_dayNightManager._homeTime, 8f));
    }

    /*public DayWeather GetWeatherAtHour(float hour)
    {

    }*/

    //sinon, vu qu'on a le temps total dans le jeu, on trouve le moyen de chopper à chaque fois le 8h du mat avec
    //les valeurs !!!!!!!

    /*public WeatherData GetWeatherAtTime(float gameHour, float startHour)
    {
        // Ajuster l'heure du jeu pour tenir compte de l'heure de début de la journée
        float adjustedHour = gameHour - startHour;

        // Si l'ajustement dépasse 24h, le ramener dans l'intervalle [0, 24]
        if (adjustedHour < 0)
        {
            adjustedHour += 24f;
        }

        // Convertir l'heure ajustée en secondes dans le cycle journalier (0-400s)
        float timeInSeconds = (adjustedHour / 24f) * 400f;

        // Utiliser la logique de CalculateWeatherInSeconds pour estimer la météo à un moment donné
        float cumulativeTime = 0f;
        for (int day = 0; day < dayWeathers.Count; day++)
        {
            DayWeather dayWeather = dayWeathers[day];

            foreach (var weatherEvent in dayWeather.weatherEvents)
            {
                float weatherDuration = weatherEvent.weatherInitialDuration;

                // Vérifier si l'heure du jeu tombe dans cette météo
                if (cumulativeTime + weatherDuration > timeInSeconds)
                {
                    // Calculer le temps restant dans cette météo
                    float timeInWeather = timeInSeconds - cumulativeTime;

                    // Utiliser PredictWeatherToTime pour obtenir la météo à ce moment précis
                    return PredictWeatherToTime(timeInWeather);
                }

                cumulativeTime += weatherDuration;
            }
        }

        // Retourner la dernière météo si nous dépassons la durée totale
        return dayWeathers.Last().weatherEvents.Last();
    }*/






    private void CalculateWeatherInSeconds()
    {
        float timeRemainingMorning = _weatherManager.currentWeather.weatherInitialDuration - _dayNightManager.TimeUntil(morningStart);
        float timeRemainingMidday = _weatherManager.currentWeather.weatherInitialDuration - _dayNightManager.TimeUntil(middayStart);
        float timeRemainingEvening = _weatherManager.currentWeather.weatherInitialDuration - _dayNightManager.TimeUntil(eveningStart);

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
            timeRemaining = currentWeather.weatherInitialDuration - overflow;
        }

        // Humidity
        weather.humidity = GetLerpedFloatAtTime(timeRemaining, currentWeather.humidity, nextWeather.humidity, currentWeather.weatherInitialDuration);

        // Temperatures
        weather.airTemperature = GetLerpedFloatAtTime(timeRemaining, currentWeather.airTemperature, nextWeather.airTemperature, currentWeather.weatherInitialDuration);
        weather.waterTemperature = GetLerpedFloatAtTime(timeRemaining, currentWeather.waterTemperature, nextWeather.waterTemperature, currentWeather.weatherInitialDuration);

        // Atmospheric pressure
        weather.atmosphericPressure = GetLerpedFloatAtTime(timeRemaining, currentWeather.atmosphericPressure, nextWeather.atmosphericPressure, currentWeather.weatherInitialDuration);

        // Wind
        weather.windSpeed = GetLerpedFloatAtTime(timeRemaining, currentWeather.windSpeed, nextWeather.windSpeed, currentWeather.weatherInitialDuration);
        weather.windOrientationValue = GetLerpedFloatAtTime(timeRemaining, currentWeather.windOrientationValue, nextWeather.windOrientationValue, currentWeather.weatherInitialDuration);
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
