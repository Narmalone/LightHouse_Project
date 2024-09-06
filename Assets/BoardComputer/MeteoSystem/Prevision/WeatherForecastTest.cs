using System;
using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using static UnityEngine.Analytics.IAnalytic;

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
        CalculateWeatherInSeconds();
    }

    public float offSetMorning;
    public float offSetMidDay;
    public float offSetEvening;

    public List<WeatherData> weathersMornings = new List<WeatherData>();
    public List<WeatherData> weathersMidday = new List<WeatherData>();
    public List<WeatherData> weathersEvening = new List<WeatherData>();
    private void _onWeatherUpdate_handle(WeatherType obj)
    {
        if (!_isInitialized)
        {
            // Exemple de prévisions météo (weatherForecast serait une liste de WeatherData)
            List<WeatherData> weatherForecast = _weatherManager.weatherForecast;

            // Calculer les météos pour chaque jour
            dayWeathers = CalculateWeatherForDays(weatherForecast, gameSettings.DayCycleDuration.Seconds);

            offSetMorning = _dayNightManager.TimeUntil(morningStart);
            offSetMidDay = _dayNightManager.TimeUntil(middayStart);
            offSetEvening = _dayNightManager.TimeUntil(eveningStart);

            for(int i = 0; i < gameSettings.TotalDays; i++)
            {

                var data2 = GetWeatherAtTimeForDay(i, offSetMorning);
                offSetMorning += 400f;
                weathersMornings.Add(data2);
                Debug.Log($"Météo le jour_{1} à {morningStart}h: {data2.weatherType}, Humidité: {data2.humidity}%, Température de l'air: {data2.airTemperature}°C");
            }


            //CalculateWeathersWithOffsets();
            CalculateWeatherInSeconds();
            _isInitialized = true;
        }
    }

    private WeatherData FindNextDifferentWeather(int currentDayIndex, WeatherData currentWeather)
    {
        // Parcours des météos suivantes dans la journée actuelle
        var currentDay = dayWeathers[currentDayIndex];
        int currentWeatherIndex = currentDay.weatherEvents.IndexOf(currentWeather);

        for (int i = currentWeatherIndex + 1; i < currentDay.weatherEvents.Count; i++)
        {
            var nextWeather = currentDay.weatherEvents[i];
            if (!AreWeatherValuesEqual(currentWeather, nextWeather))
            {
                return nextWeather;
            }
        }

        // Si on a atteint la fin de la journée, vérifier les jours suivants
        for (int dayIndex = currentDayIndex + 1; dayIndex < dayWeathers.Count; dayIndex++)
        {
            foreach (var nextWeather in dayWeathers[dayIndex].weatherEvents)
            {
                if (!AreWeatherValuesEqual(currentWeather, nextWeather))
                {
                    return nextWeather;
                }
            }
        }

        // Si aucune météo différente n'est trouvée, retourner la météo actuelle
        return currentWeather;
    }



    private bool AreWeatherValuesEqual(WeatherData weather1, WeatherData weather2)
    {
        return Mathf.Approximately(weather1.humidity, weather2.humidity) &&
               Mathf.Approximately(weather1.airTemperature, weather2.airTemperature) &&
               Mathf.Approximately(weather1.waterTemperature, weather2.waterTemperature) &&
               Mathf.Approximately(weather1.windSpeed, weather2.windSpeed) &&
               Mathf.Approximately(weather1.windOrientationValue, weather2.windOrientationValue) &&
               Mathf.Approximately(weather1.atmosphericPressure, weather2.atmosphericPressure);
    }


    public WeatherData GetWeatherAtTimeForDay(int dayNumber, float gameTimeInSeconds)
    {
        // Calculer le temps de départ pour ce jour
        float dayStartTime = dayNumber * gameSettings.DayCycleDuration.Seconds;
        float timeInTotalSimulation = dayStartTime + gameTimeInSeconds + (dayNumber * gameSettings.DayCycleDuration.Seconds);  // Temps total dans la simulation globale

        // Boucle à travers tous les WeatherData pour trouver celui qui correspond à ce temps

        for (int dayIndex = 0; dayIndex < dayWeathers.Count; dayIndex++)
        {
            var day = dayWeathers[dayIndex];

            //CHOPPER SOUS FORME DE DUREE LE TEMPS EN SECONDE A IGNORER, DONC DE GENRE 18H A 6H

            foreach (var weather in day.weatherEvents)
            {
                // Vérifier si le temps total est dans la plage de cette météo
                if (timeInTotalSimulation >= weather.startAtTime && timeInTotalSimulation < (weather.startAtTime + weather.weatherInitialDuration))
                {
                    // Trouver la météo suivante avec des valeurs différentes pour l'interpolation
                    WeatherData nextDifferentWeather = FindNextDifferentWeather(dayIndex, weather);
                    Debug.Log("humidity " + weather.humidity + " on next weather " + nextDifferentWeather.humidity);
                    // Calculer le temps écoulé depuis le début de la météo
                    float elapsedTime = timeInTotalSimulation - weather.startAtTime;

                    float remainingSeconds = weather.weatherInitialDuration - gameTimeInSeconds;
                    return PredictWeatherToTime(remainingSeconds, weather, nextDifferentWeather);
                    // Interpoler les valeurs avec la prochaine météo différente
                }
            }
        }

        // Si aucune météo n'est trouvée, retourner la dernière météo par défaut
        return dayWeathers.Last().weatherEvents.Last();
    }









    private void CalculateWeathersWithOffsets()
    {
        // Calculer les météos pour chaque période de la journée
        weathersMornings = new List<WeatherData>();
        weathersMidday = new List<WeatherData>();
        weathersEvening = new List<WeatherData>();

        float timeStep = gameSettings.DayCycleDuration.Seconds; // Durée entre chaque pas de temps

        float morningTimeStep = 0f;
        float middayTimeStep = 0f;
        float eveningTimeStep = 0f;

        morningTimeStep = _dayNightManager.TimeUntil(morningStart);
        middayTimeStep = _dayNightManager.TimeUntil(middayStart);
        eveningTimeStep = _dayNightManager.TimeUntil(eveningStart);

        for (int i = 0; i < totalDays; i++)
        {
            WeatherData morningWeather = new WeatherData();
            WeatherData middayWeather = new WeatherData();
            WeatherData eveningWeather = new WeatherData();

            morningWeather = GetWeatherDataInWeatherSeconds(i, morningTimeStep);
            //middayWeather = GetWeatherDataInWeatherSeconds(i, middayTimeStep);
            //eveningWeather = GetWeatherDataInWeatherSeconds(i, eveningTimeStep);
            if(i != 0)
            {
                offSetMorning = _dayNightManager.TimeTo(0f, morningStart);
            }

            GetWeatherData(i, offSetMorning);

            weathersMornings.Add(morningWeather);
            weathersMidday.Add(middayWeather);
            weathersEvening.Add(eveningWeather);

            // Avancer dans le temps
            morningTimeStep += timeStep;
            middayTimeStep += timeStep;
            eveningTimeStep += timeStep;
        }
    }

    public WeatherData GetWeatherDataInWeatherSeconds(int day, float offsetInSeconds)
    {
        //comparer le elapsed time du truc de data à l'offset ????
        //a l'aide l'offset de départ plus après l'offset on ajouteu n cycle d'une journée à chaque x donc 400 pour nous 
        //actuellememnt. on peut déterminer grâce à la durée totale du jeu weathertotalDuration
        //Debug.Log(offsetInSeconds);
        //float days = (int)offsetInSeconds / (int)gameSettings.DayCycleDuration.Seconds;
        float totalDuration = 0f;

        
        for (int i = 0; i < _weatherManager.weatherForecast.Count; i++)
        {
            totalDuration += _weatherManager.weatherForecast[i].weatherInitialDuration;
            if (totalDuration >= offsetInSeconds)
            {
                float s = _weatherManager.weatherForecast[i].weatherInitialDuration - offsetInSeconds;
                //Debug.Log($"le jour {day}, A X heure du matin, la météo sera: " + _weatherManager.weatherForecast[i].weatherType);
                //on vient calculer le lerp entre cette météo et la suivante
                //Debug.Log("horaire humidity : " + _weatherManager.weatherForecast[i].humidity + ", a x heure " + _weatherManager.weatherForecast[i + 1].humidity);
                if(i + 1 < _weatherManager.weatherForecast.Count)
                {
                    return PredictWeatherToTime(s, _weatherManager.weatherForecast[i], _weatherManager.weatherForecast[i + 1]);
                }

            }
        }

        return new WeatherData();
    }


    //
    public WeatherData GetWeatherData(int dayIndex, float offsetTime)
    {
        float cumulativeSeconds = 0f;
        DayWeather currDay = dayWeathers[dayIndex];


        foreach (var meteo in currDay.weatherEvents)
        {
            // Ajoute la durée de la météo à cumulativeSeconds pour avancer dans la journée
            cumulativeSeconds += meteo.weatherClampedDuration;
            // Si le cumul atteint ou dépasse offsetTime, nous sommes dans cette météo
            if (cumulativeSeconds >= offsetTime)
            {

                if (meteo.weatherClampedDuration >= gameSettings.DayCycleDuration.Seconds)
                {
                    //Debug.Log("ALERTE ROUGE " + day);
                    // Si la météo déborde, on peut interpoler entre les valeurs actuelles et les suivantes

                }

                if (dayIndex + 1 < dayWeathers.Count)
                {
                    //si le truc dure plusde 400s  par exemple ça va regarder le prochain event du prochain jour
                    //qui est une dupplication de cette météo mais avec des valeurs de la prochaine :)
                    var data = PredictWeatherDataInTime(offsetTime, meteo, dayWeathers[dayIndex + 1].weatherEvents[0]);
                    data.weatherInitialDuration = meteo.weatherInitialDuration;
                    data.weatherClampedDuration = meteo.weatherClampedDuration;
                    return data;
                }
                
            }
            else
            {
                int meteoIndex = currDay.weatherEvents.IndexOf(meteo);
                float lowClampedDuration = meteo.weatherClampedDuration;

                //donc on sait que la weatherclamped duration de celle ci est < à l'offset donc la météo ne fais pas partie
                //de la plage horaire donc ça va déborder, de X secondes, X étant la weather clamped duration

            }
        }

        // Si aucune météo n'a été trouvée, on retourne la dernière météo
        return currDay.weatherEvents.Last();
    }

    public WeatherData PredictWeatherDataInTime(float inSecondsRealTime, WeatherData current, WeatherData next)
    {
        WeatherData lerpedData = new WeatherData();

        // Calculate the interpolation factor based on the inSecondsRealTime
        float interpolationFactor = inSecondsRealTime / current.weatherInitialDuration;

        // Interpolate the values using a linear formula
        lerpedData.humidity = current.humidity + (next.humidity - current.humidity) * interpolationFactor;
        lerpedData.airTemperature = current.airTemperature + (next.airTemperature - current.airTemperature) * interpolationFactor;
        lerpedData.waterTemperature = current.waterTemperature + (next.waterTemperature - current.waterTemperature) * interpolationFactor;
        lerpedData.windSpeed = current.windSpeed + (next.windSpeed - current.windSpeed) * interpolationFactor;
        lerpedData.windOrientationValue = current.windOrientationValue + (next.windOrientationValue - current.windOrientationValue) * interpolationFactor;
        // Add more properties as needed

        return lerpedData;
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
                    weatherType = weather.weatherType,
                    startAtTime = weather.startAtTime,
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

                    //firstWeather.weatherType = secondWeather.weatherType;

                    // Garder la durée originale
                    //firstWeather.weatherClampedDuration = currentDayW.weatherEvents[0].weatherClampedDuration;
                    //firstWeather.weatherInitialDuration = currentDayW.weatherEvents[0].weatherInitialDuration;

                    nextDay.weatherEvents[0] = firstWeather;
                }
            }
        }

        return days;
    }


    private void CalculateWeatherInSeconds()
    {
        float timeRemainingMorning = _weatherManager.currentWeather.weatherInitialDuration - _dayNightManager.TimeUntil(morningStart);
        float timeRemainingMidday = _weatherManager.currentWeather.weatherInitialDuration - _dayNightManager.TimeUntil(middayStart);
        float timeRemainingEvening = _weatherManager.currentWeather.weatherInitialDuration - _dayNightManager.TimeUntil(eveningStart);

        morningWeather = PredictWeatherToTime(timeRemainingMorning, _weatherManager.currentWeather, _weatherManager.nextWeather);
        middayWeather = PredictWeatherToTime(timeRemainingMidday, _weatherManager.currentWeather, _weatherManager.nextWeather);
        eveningWeather = PredictWeatherToTime(timeRemainingEvening, _weatherManager.currentWeather, _weatherManager.nextWeather);

        UpdateWeatherUI();
    }


    private WeatherData PredictWeatherToTime(float timeRemaining, WeatherData currentWeather, WeatherData nextWeather)
    {
        WeatherData weather = new WeatherData();

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
