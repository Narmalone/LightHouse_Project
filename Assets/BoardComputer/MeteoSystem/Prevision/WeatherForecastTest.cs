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

    public float StartoffSetMorning;
    public float StartoffSetMidDay;
    public float StartoffSetEvening;

    public float OffsetMorningFromMidnight;
    public float OffsetMiddayFromMidnight;
    public float OffsetEveningFromMidnight;


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
            //dayWeathers = CalculateWeatherForDays(weatherForecast, gameSettings.DayCycleDuration.Seconds);

            StartoffSetMorning = _dayNightManager.TimeUntil(morningStart);
            //StartoffSetMorning = _dayNightManager.TimeTo(0f, 3f);
            StartoffSetMidDay = _dayNightManager.TimeUntil(middayStart);
            StartoffSetEvening = _dayNightManager.TimeUntil(eveningStart);

            OffsetMorningFromMidnight = _dayNightManager.TimeTo(0f, morningStart);
            OffsetMiddayFromMidnight = _dayNightManager.TimeTo(0f, middayStart);
            OffsetEveningFromMidnight = _dayNightManager.TimeTo(0f, eveningStart);

            dayWeathers = CalculateWeatherFor(_weatherManager.weatherForecast, gameSettings.DayCycleDuration.Seconds, StartoffSetMorning, OffsetMorningFromMidnight);


            _dayNightManager.AfficherHeureMeteo(_weatherManager.weatherForecast, _dayNightManager._homeTime);

            MorningX.Add(_dayNightManager.GetWeatherAtTime(0, 6f));
            
            //weathersMornings = TryCalculateWeathers(StartoffSetMorning);
            //weathersMornings = GenerateWeatherForecastEveryMornings(StartoffSetMorning);

            GetMorningWeatherForDay(1);

            //CalculateWeathersWithOffsets();
            CalculateWeatherInSeconds();
            _isInitialized = true;
        }
    }

    //3 listes d'offset, une le matin, une le midi et une le soir
    //cela permettra de savoir dans à combien de secondes tombe la période
    //on cherche la météo qui sera le plus proche ou celle qui correspond la fourchette donc matin j 8 = 3458 -> trouver météo
    //qui correspond à 3458s, une fois qu'on a la météo
    public List<float> offsetsMornings = new List<float>();
    public List<float> offsetsMidday = new List<float>();
    public List<float> offsetsEvenings = new List<float>();

    public List<float> MorningAtTime = new List<float>();
    public List<WeatherData> MorningX = new();

    [System.Serializable]
    public struct WeatherPeriodData
    {
        public List<float> morningsTimes;
        public WeatherData morningData;
    }


    public void GetMorningWeatherForDay(int baseDay)
    {
        float morningTargetTime = 6f * 60f; // 6h du matin en secondes dans la journée (360s)
        float startOfDayTime = baseDay * gameSettings.DayCycleDuration.Seconds; // Temps de début de la journée en secondes
        float targetTimeInSimulation = startOfDayTime + morningTargetTime; // Temps total dans la simulation

        float step = 400f; // Durée d'une journée en secondes
        float currentStep = StartoffSetMorning;

        // Stocker les temps précis des "matins" dans MorningAtTime
        for (int i = 0; i < gameSettings.TotalDays; i++)
        {
            MorningAtTime.Add(currentStep);
            currentStep += step;
        }


        for (int i = 0; i < MorningAtTime.Count; i++)
        {
            WeatherPeriodData periodData = new WeatherPeriodData();
            int index = GetWeatherIndexForecastFromTotalSeconds(MorningAtTime[i], _weatherManager.weatherForecast);


        }
        // Boucle à travers les prévisions météo pour trouver la bonne météo à 6h du matin
        var d = dayWeathers[baseDay];

        
        //Debug.Log($"Prévision Météo du jour{d.dayNumber} pour 6h: ");
    }

    public WeatherData FindNextWeatherWithDifferentValues(List<DayWeather> days, int startIndex, WeatherData currentWeatherData)
    {
        for (int i = startIndex; i < days.Count; i++)
        {
            DayWeather day = days[i];
            for (int j = 0; j < day.weatherEvents.Count; j++)
            {
                WeatherData weatherData = day.weatherEvents[j];
                if (j == 0 || !AreWeatherDataSimilar(currentWeatherData, weatherData))
                {
                    return weatherData;
                }
            }
        }

        // Si on arrive ici, c'est que l'on a parcouru toute la liste sans trouver de WeatherData différente
        // On peut donc retourner null ou throw une exception
        return new WeatherData();
    }

    private bool AreWeatherDataSimilar(WeatherData weatherData1, WeatherData weatherData2)
    {
        return weatherData1.weatherType == weatherData2.weatherType &&
               weatherData1.humidity == weatherData2.humidity 
               // ajoutez d'autres conditions pour vérifier la similarité
               ;
    }

    public List<WeatherData> GenerateWeatherForecastEveryMornings(float startStep)
    {
        float step = 400f; // Durée d'une journée en secondes
        float currentStep = startStep;

        // Stocker les temps précis des "matins" dans MorningAtTime
        for (int i = 0; i < gameSettings.TotalDays; i++)
        {
            MorningAtTime.Add(currentStep);
            currentStep += step;
        }

        List<WeatherData> weatherDataList = new List<WeatherData>();

        // Calculer la météo pour chaque moment stocké dans MorningAtTime
        foreach (float morningTime in MorningAtTime)
        {
            WeatherData weatherAtTime = GetWeatherAtSpecificTime(morningTime);
            weatherDataList.Add(weatherAtTime);
        }

        return weatherDataList;
    }

    private WeatherData GetWeatherAtSpecificTime(float timeInSeconds)
    {
        float elapsedTime = 0f;

        // Parcourir les météos pour trouver celle correspondant au moment donné
        for (int i = 0; i < _weatherManager.weatherForecast.Count - 1; i++)
        {
            WeatherData currentWeather = _weatherManager.weatherForecast[i];
            WeatherData nextWeather = _weatherManager.weatherForecast[i + 1];


            elapsedTime += currentWeather.weatherInitialDuration;
            // Vérifier si le temps en secondes se situe dans cette météo
            if (timeInSeconds <= elapsedTime)
            {
                // Calculer le temps restant dans la météo actuelle
                float timeInCurrentWeather = timeInSeconds - (elapsedTime - currentWeather.weatherInitialDuration);

                // Interpoler les valeurs météorologiques entre la météo actuelle et la suivante
                return InterpolateWeatherData(currentWeather.weatherInitialDuration - MorningAtTime[0], currentWeather, nextWeather);
            }
        }

        // Si aucune météo n'est trouvée (fin du cycle), retourner la dernière météo
        return _weatherManager.weatherForecast.Last();
    }

    private WeatherData InterpolateWeatherData(float timeRemaining, WeatherData currentWeather, WeatherData nextWeather)
    {
        WeatherData weather = new WeatherData();

        // Interpoler les valeurs météorologiques entre la météo actuelle et la suivante
        weather.humidity = GetLerpedFloatAtTime(timeRemaining, currentWeather.humidity, nextWeather.humidity, currentWeather.weatherInitialDuration);
        weather.airTemperature = GetLerpedFloatAtTime(timeRemaining, currentWeather.airTemperature, nextWeather.airTemperature, currentWeather.weatherInitialDuration);
        weather.waterTemperature = GetLerpedFloatAtTime(timeRemaining, currentWeather.waterTemperature, nextWeather.waterTemperature, currentWeather.weatherInitialDuration);
        weather.atmosphericPressure = GetLerpedFloatAtTime(timeRemaining, currentWeather.atmosphericPressure, nextWeather.atmosphericPressure, currentWeather.weatherInitialDuration);
        weather.windSpeed = GetLerpedFloatAtTime(timeRemaining, currentWeather.windSpeed, nextWeather.windSpeed, currentWeather.weatherInitialDuration);
        weather.windOrientationValue = GetLerpedFloatAtTime(timeRemaining, currentWeather.windOrientationValue, nextWeather.windOrientationValue, currentWeather.weatherInitialDuration);
        weather.windDirection = _weatherManager.DetermineWindDirection(weather.windOrientationValue);

        // Conserver le type de météo
        weather.weatherType = currentWeather.weatherType;

        return weather;
    }


    public List<WeatherData> TryCalculateWeathers(float startOffsetTarget)
    {
        List<WeatherData> periodData = new List<WeatherData>();
        float step = 400f;  // Suppose un cycle de 400 secondes par jour
        float currentStep = startOffsetTarget;
        int day = 0;

        while (day < gameSettings.TotalDays)
        {
            // Trouver la météo qui couvre le temps actuel
            int weatherForecastIdFromOffset = GetWeatherIndexForecastFromTotalSeconds(currentStep, _weatherManager.weatherForecast);

            // Si la météo est trouvée et que la suivante existe
            if (weatherForecastIdFromOffset + 1 < _weatherManager.weatherForecast.Count)
            {
                WeatherData currentWeather = _weatherManager.weatherForecast[weatherForecastIdFromOffset];
                WeatherData nextWeather = _weatherManager.weatherForecast[weatherForecastIdFromOffset + 1];

                // Si la météo déborde sur le jour suivant
                if (currentWeather.startAtTime + currentWeather.weatherInitialDuration > currentStep + step)
                {
                    // Lerp entre la météo actuelle et la prochaine pour le matin du jour suivant
                    var lerpedWeather = LerpWeatherOverDayBoundary(currentWeather, nextWeather, currentStep, step);
                    periodData.Add(lerpedWeather);
                }
                else
                {
                    // Générer les données météo sans débordement
                    var d = GenerateWeatherDatas(currentWeather, nextWeather, startOffsetTarget, currentStep);
                    periodData.Add(d);
                }

                currentStep += step;  // Avance au prochain jour
                day++;  // Passer au jour suivant
            }
            else
            {
                // Si aucune météo n'est trouvée, avancer au jour suivant
                currentStep += step;
                day++;
            }
        }

        return periodData;
    }

    public WeatherData LerpWeatherOverDayBoundary(WeatherData current, WeatherData next, float currentStep, float dayDuration)
    {
        // Le pourcentage du débordement de la météo sur le jour suivant
        float timeOverlappingIntoNextDay = (currentStep + dayDuration) - (current.startAtTime + current.weatherInitialDuration);
        float lerpFactor = timeOverlappingIntoNextDay / current.weatherInitialDuration;

        WeatherData dataToReturn = new WeatherData();
        dataToReturn.weatherType = current.weatherType;
        dataToReturn.windDirection = current.windDirection;

        // Interpolation basée sur le débordement de la météo
        dataToReturn.humidity = GetLerpedFloatAtTime(lerpFactor, current.humidity, next.humidity, current.weatherInitialDuration);
        dataToReturn.airTemperature = GetLerpedFloatAtTime(lerpFactor, current.airTemperature, next.airTemperature, current.weatherInitialDuration);
        dataToReturn.waterTemperature = GetLerpedFloatAtTime(lerpFactor, current.waterTemperature, next.waterTemperature, current.weatherInitialDuration);
        dataToReturn.windSpeed = GetLerpedFloatAtTime(lerpFactor, current.windSpeed, next.windSpeed, current.weatherInitialDuration);
        dataToReturn.windOrientationValue = GetLerpedFloatAtTime(lerpFactor, current.windOrientationValue, next.windOrientationValue, current.weatherInitialDuration);
        dataToReturn.atmosphericPressure = GetLerpedFloatAtTime(lerpFactor, current.atmosphericPressure, next.atmosphericPressure, current.weatherInitialDuration);

        return dataToReturn;
    }

    public WeatherData GenerateWeatherDatas(WeatherData current, WeatherData target, float startOffset, float currentStep)
    {
        float remainingSeconds = current.weatherInitialDuration - startOffset;
        WeatherData dataToReturn = new WeatherData();
        dataToReturn.weatherType = current.weatherType;
        dataToReturn.windDirection = current.windDirection;

        // Interpolation classique sans débordement
        dataToReturn.humidity = GetLerpedFloatAtTime(remainingSeconds, current.humidity, target.humidity, current.weatherInitialDuration);
        dataToReturn.airTemperature = GetLerpedFloatAtTime(remainingSeconds, current.airTemperature, target.airTemperature, current.weatherInitialDuration);
        dataToReturn.waterTemperature = GetLerpedFloatAtTime(remainingSeconds, current.waterTemperature, target.waterTemperature, current.weatherInitialDuration);
        dataToReturn.windSpeed = GetLerpedFloatAtTime(remainingSeconds, current.windSpeed, target.windSpeed, current.weatherInitialDuration);
        dataToReturn.windOrientationValue = GetLerpedFloatAtTime(remainingSeconds, current.windOrientationValue, target.windOrientationValue, current.weatherInitialDuration);
        dataToReturn.atmosphericPressure = GetLerpedFloatAtTime(remainingSeconds, current.atmosphericPressure, target.atmosphericPressure, current.weatherInitialDuration);

        return dataToReturn;
    }


    

    public int GetWeatherIndexForecastFromTotalSeconds(float currentStep, List<WeatherData> weatherForecast)
    {
        // Parcourir la liste des prévisions météo pour trouver celle correspondant à totalSeconds
        for (int i = 0; i < weatherForecast.Count; i++)
        {
            //TROUVER LA METEO A PAR EXEMPLE 16S, 416S
            // Obtenir le startAtTime et la durée de l'événement météo actuel
            float startAt = weatherForecast[i].startAtTime;
            float endAt = startAt + weatherForecast[i].weatherInitialDuration;

            // Si totalSeconds est compris entre startAt et endAt, on a trouvé la bonne météo
            if (currentStep >= startAt && currentStep < endAt)
            {
                return i;
            }
        }

        // Retourner -1 si aucun événement météo n'a été trouvé pour le totalSeconds donné
        return -1;
    }

    public WeatherData GetWeatherForecastFromTotalSeconds(float totalSeconds, List<WeatherData> weatherForecast)
    {
        float countSeconts = 0f;

        for(int i = 0; i < weatherForecast.Count; i++)
        {
            countSeconts += weatherForecast[i].weatherInitialDuration;

            if(countSeconts >= totalSeconds)
            {
                return weatherForecast[i];
            }
        }

        return new WeatherData();
    }

    public List<DayWeather> CalculateWeatherFor(List<WeatherData> forecasts, float dayDuration, float startOffset, float mainOffset)
    {
        List<DayWeather> days = new List<DayWeather>();
        List<WeatherData> weatherForecast = forecasts;
        int currentDay = 0;

        DayWeather newDayDatas = new DayWeather();
        newDayDatas.weatherEvents = new List<WeatherData>();
        float totalDayDuration = 0f;

        for (int i = 0; i < weatherForecast.Count; i++)
        {
            float weatherDurationRemaining = weatherForecast[i].weatherInitialDuration;

            while (weatherDurationRemaining > 0)
            {
                float durationToAdd = Mathf.Min(weatherDurationRemaining, dayDuration - totalDayDuration);

                WeatherData data = weatherForecast[i];

                if (i + 1 < weatherForecast.Count)
                {
                    //générer matin midi soir à partir des 400s et en matchant avec la prochaine météo :)
                    //le matin midi, soir tombent toujours à la même heure compte tenant des offsets

                    //le premier est généré avec un offset
                    if (i == 0)
                    {
                        //period peut être morning, midday, evening...
                        //data = GenerateWeatherData(weatherForecast[i], weatherForecast[i + 1], startOffset);
                        //data.weatherInitialDuration = weatherForecast[i].weatherInitialDuration;
                        data.weatherClampedDuration = durationToAdd;
                    }
                    else
                    {
                        //data = GenerateWeatherData(weatherForecast[i], weatherForecast[i + 1], mainOffset);
                        //data.weatherInitialDuration = weatherForecast[i].weatherInitialDuration;
                        data.weatherClampedDuration = durationToAdd;
                    }
                }

                newDayDatas.dayNumber = currentDay;
                newDayDatas.weatherEvents.Add(data);

                if (newDayDatas.weatherEvents.Count > 1)
                {
                    for (int j = 1; j < newDayDatas.weatherEvents.Count; j++)
                    {
                        WeatherData previousData = newDayDatas.weatherEvents[j - 1];
                        WeatherData currentData = newDayDatas.weatherEvents[j];

                        // Effectuer un lerp entre les valeurs de humidity des deux météos
                        //currentData.humidity = GetLerpedFloatAtTime(currentData.weatherInitialDuration -  mainOffset)
                        //currentData.humidity = f;
                        newDayDatas.weatherEvents[j] = currentData;

                    }

                    /*WeatherData duplicatedData = newDayDatas.weatherEvents[newDayDatas.weatherEvents.Count - 1];
                    // modifier la durée de la données dupliquée si elle dépasse les 400s
                    duplicatedData.humidity = 150f;
                    newDayDatas.weatherEvents[newDayDatas.weatherEvents.Count - 1] = duplicatedData;*/
                }

                totalDayDuration += durationToAdd;
                weatherDurationRemaining -= durationToAdd;

                if (totalDayDuration >= dayDuration)
                {
                    days.Add(newDayDatas);
                    currentDay++;
                    newDayDatas = new DayWeather(currentDay);
                    newDayDatas.weatherEvents = new List<WeatherData>();
                    totalDayDuration = 0f;
                }
            }
        }


        //CENSE ETRE UNREACHEABLE LOGIQUEMENT
        // Ajout de la dernière journée si elle contient des événements météo
        if (newDayDatas.weatherEvents.Count > 0)
        {
            days.Add(newDayDatas);
        }

        // Ajout de jours supplémentaires pour atteindre 31 jours
        while (days.Count < 31)
        {
            DayWeather newDay = new DayWeather(days.Count);
            newDay.weatherEvents = new List<WeatherData>();
            days.Add(newDay);
        }

        return days;
    }

    public List<DayWeather> CalculateWeatherFor2(List<WeatherData> forecasts, float dayDuration, float startOffset, float mainOffset)
    {
        List<DayWeather> days = new List<DayWeather>();
        List<WeatherData> weatherForecast = forecasts;
        int currentDay = 0;

        DayWeather newDayDatas = new DayWeather();
        newDayDatas.weatherEvents = new List<WeatherData>();
        float totalDayDuration = 0f;

        for (int i = 0; i < weatherForecast.Count; i++)
        {
            float weatherDurationRemaining = weatherForecast[i].weatherInitialDuration;

            while (weatherDurationRemaining > 0)
            {
                float durationToAdd = Mathf.Min(weatherDurationRemaining, dayDuration - totalDayDuration);

                WeatherData data = weatherForecast[i];

                if (i + 1 < weatherForecast.Count)
                {
                    //générer matin midi soir à partir des 400s et en matchant avec la prochaine météo :)
                    //le matin midi, soir tombent toujours à la même heure compte tenant des offsets

                    //le premier est généré avec un offset
                    if (i == 0)
                    {
                        //period peut être morning, midday, evening...
                        data = GenerateWeatherData(weatherForecast[i], weatherForecast[i + 1], startOffset);
                        data.weatherInitialDuration = weatherForecast[i].weatherInitialDuration;
                        data.weatherClampedDuration = durationToAdd;
                        Debug.Log(data.humidity);
                    }
                    else
                    {
                        data = GenerateWeatherData(weatherForecast[i], weatherForecast[i + 1], mainOffset);
                        data.weatherInitialDuration = weatherForecast[i].weatherInitialDuration;
                        data.weatherClampedDuration = durationToAdd;

                        data.humidity = 150f;
                        Debug.Log(data.humidity);
                        //Debug.Log(period.humidity);
                    }
                }

                if (newDayDatas.weatherEvents.Count == 0)
                {
                    newDayDatas.dayNumber = currentDay;
                    newDayDatas.weatherEvents.Add(data);
                }
                else
                {
                    // Si il y a déjà une WeatherData dans le DayWeather, on crée un nouveau DayWeather
                    days.Add(newDayDatas);
                    currentDay++;
                    newDayDatas = new DayWeather(currentDay);
                    newDayDatas.weatherEvents = new List<WeatherData>();
                    newDayDatas.dayNumber = currentDay;
                    newDayDatas.weatherEvents.Add(data);
                }

                totalDayDuration += durationToAdd;
                weatherDurationRemaining -= durationToAdd;

                if (totalDayDuration >= dayDuration)
                {
                    totalDayDuration = 0f;
                }
            }
        }

        // Ajout de la dernière journée si elle contient des événements météo
        if (newDayDatas.weatherEvents.Count > 0)
        {
            days.Add(newDayDatas);
        }

        // Ajout de jours supplémentaires pour atteindre 31 jours
        while (days.Count < 31)
        {
            DayWeather newDay = new DayWeather(days.Count);
            newDay.weatherEvents = new List<WeatherData>();
            days.Add(newDay);
        }

        return days;
    }

    public WeatherData GenerateWeatherData(WeatherData current, WeatherData target, float offset)
    {
        WeatherData dataToReturn = new WeatherData();
        dataToReturn.weatherType = current.weatherType;
        dataToReturn.windDirection = current.windDirection;
        dataToReturn.humidity = GetLerpedFloatAtTime(current.weatherInitialDuration - offset, current.humidity, target.humidity, current.weatherInitialDuration);
        dataToReturn.airTemperature = GetLerpedFloatAtTime(current.weatherInitialDuration - offset, current.airTemperature, target.airTemperature, current.weatherInitialDuration);
        dataToReturn.waterTemperature = GetLerpedFloatAtTime(current.weatherInitialDuration - offset, current.waterTemperature, target.waterTemperature, current.weatherInitialDuration);
        dataToReturn.windSpeed = GetLerpedFloatAtTime(current.weatherInitialDuration - offset, current.windSpeed, target.windSpeed, current.weatherInitialDuration);
        dataToReturn.windOrientationValue = GetLerpedFloatAtTime(current.weatherInitialDuration - offset, current.windOrientationValue, target.windOrientationValue, current.weatherInitialDuration);
        dataToReturn.atmosphericPressure = GetLerpedFloatAtTime(current.weatherInitialDuration - offset, current.atmosphericPressure, target.atmosphericPressure, current.weatherInitialDuration);
        //ENCORE PRECISER DIRECTION DU VENT, TYPE DE METEOS ECT
        return dataToReturn;
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
