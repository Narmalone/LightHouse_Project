using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;


public class WeatherForDaysManager : Singleton<WeatherForDaysManager>
{
    #region STRUCTS
    [System.Serializable]
    public struct PeriodData
    {
        public WeatherData data;
        public float Hour;
        public float Minits;
        public float Seconds;
        public int Day;
    }
    #endregion

    #region SERIALIZED FIELDS
    [SerializeField] private GameSettings gameSettings;

    [Header("Weather Manager")]
    [SerializeField] private WeatherManager _weatherManager;
    [SerializeField] private DayNightManager _dayNightManager;

    [SerializeField] private CustomEvent_WeatherType _onWeatherGenerated;
    [SerializeField] private CustomEvent _onDaysWeatherInitialized;
    [SerializeField] private CustomEvent _middnightReached;

    [Header("Forecast Settings")]
    [SerializeField] private DayNightSettings _dayNightSettings;

    [Header("START OFFSETS IN SECONDS FROM GAME START")]
    public float StartoffSetMorning;
    public float StartoffSetMidDay;
    public float StartoffSetEvening;

    [Header("ALL NEXTS X HOURS IN SECONDS")]
    public List<float> MorningAtTime = new List<float>();
    public List<float> MiddayAtTime = new List<float>();
    public List<float> EveningAtTime = new List<float>();
    public List<float> MiddnightAtTime = new List<float>();

    [Header("Weathers Separated by Days")]
    public List<PeriodData> WeathersInDays = new List<PeriodData>();

    [Header("VALUES FOR")]
    public List<WeatherData> MorningX = new();
    public List<WeatherData> MiddaysX = new();
    public List<WeatherData> EveningsX = new();
    public List<WeatherData> MiddnightX = new();
    #endregion

    #region PRIVATE FIELDS

    private bool _isInitialized = false;
    #endregion

    #region MONO'S CALLBACKS

    protected override void Awake()
    {
        base.Awake();
        if(_onDaysWeatherInitialized != null)
            _onWeatherGenerated.handle += _onWeatherUpdate_handle;
        if(_middnightReached != null)
            _middnightReached.handle += _middnightReached_handle;
    }

    private void OnDestroy()
    {
        _onWeatherGenerated.handle -= _onWeatherUpdate_handle;
        _middnightReached.handle -= _middnightReached_handle;
    }

    #endregion
    //la station attend un relevé météo entre 6h et 18h et à partir du moment ou c'est dans la fourchette
    //on considère que c'est bon
    //TOUT LES CHAMPS COMPLETES AVANT D'ENVOYER
    #region DELEGATES

    private void _middnightReached_handle()
    {
        //
    }

    private void _onWeatherUpdate_handle(WeatherType obj)
    {
        if (!_isInitialized)
        {
            // Calcul des offsets de départ
            StartoffSetMorning = _dayNightManager.TimeUntil(_dayNightSettings.MorningStartHour);
            StartoffSetMidDay = _dayNightManager.TimeUntil(_dayNightSettings.MiddayStartHour);
            StartoffSetEvening = _dayNightManager.TimeUntil(_dayNightSettings.EveningStartHour);

            // Calcul des temps des différentes périodes
            float dayDuration = gameSettings.DayCycleDuration.DurationInSeconds;
            MorningAtTime = GetStepsAtTime(StartoffSetMorning, gameSettings.TotalDays, dayDuration);
            MiddayAtTime = GetStepsAtTime(StartoffSetMidDay, gameSettings.TotalDays, dayDuration);
            EveningAtTime = GetStepsAtTime(StartoffSetEvening, gameSettings.TotalDays, dayDuration);

            float startOffsetMidnight = _dayNightManager.TimeUntil(24f);
            MiddnightAtTime = GetStepsAtTime(startOffsetMidnight, gameSettings.TotalDays, dayDuration);

            // Calcul des météos pour chaque période
            WeathersInDays = GetWeathersInDays(_weatherManager.weatherForecast, _dayNightManager._startAtHour, dayDuration);

            for (int i = 0; i < gameSettings.TotalDays; i++)
            {
                MorningX.Add(GetWeatherAtTime(i, _dayNightSettings.MorningStartHour, MorningAtTime));
                MiddaysX.Add(GetWeatherAtTime(i, _dayNightSettings.MiddayStartHour, MiddayAtTime));
                EveningsX.Add(GetWeatherAtTime(i, _dayNightSettings.EveningStartHour, EveningAtTime));
                MiddnightX.Add(GetWeatherAtTime(i, 24f, MiddnightAtTime));
            }

            if(_dayNightManager._startAtHour > _dayNightSettings.EveningStartHour)
            {
                MorningX.RemoveAt(0);
                MiddaysX.RemoveAt(0);
                EveningsX.RemoveAt(0);
            }
            else if(_dayNightManager._startAtHour > _dayNightSettings.MiddayStartHour)
            {
                MorningX.RemoveAt(0);
                MiddaysX.RemoveAt(0);
            }
            else if (_dayNightManager._startAtHour > _dayNightSettings.MorningStartHour)
            {
                MorningX.RemoveAt(0);
            }

            _isInitialized = true;
            _onDaysWeatherInitialized?.Raise();
        }
    }


    #endregion

    #region GET / CALCULATIONS FUNCTIONS
    public List<float> GetStepsAtTime(float initOffset, int totalDays, float dayDycleDuration)
    {
        List<float> steps = new List<float>();  
        float step = dayDycleDuration; // Durée d'une journée en secondes
        float currentStep = initOffset;

        // Stocker les temps précis des "matins" dans MorningAtTime
        for (int i = 0; i < totalDays; i++)
        {
            steps.Add(currentStep);
            currentStep += step;
        }
        return steps;
    }
    public List<PeriodData> GetWeathersInDays(List<WeatherData> weatherDatas, float homeTime, float dayCycleDuration)
    {
        List<PeriodData> wethersInDays = new List<PeriodData>();
        float tempsTotalJournee = dayCycleDuration; // Temps total d'une journée en secondes

        foreach (WeatherData weatherData in weatherDatas)
        {
            PeriodData newPeriodData = new PeriodData();
            // Calculer le jour, l'heure, les minutes et les secondes où la météo sera lancée
            int jour = (int)(weatherData.startAtTime / tempsTotalJournee);
            float heure = (weatherData.startAtTime % tempsTotalJournee) / tempsTotalJournee * 24;
            heure += homeTime; // Ajouter l'offset du homeTime
            if (heure >= 24)
            {
                heure -= 24;
                jour++;
            }
            int heures = (int)heure;
            int minutes = (int)((heure - heures) * 60);
            int secondes = (int)(((heure - heures) * 60 - minutes) * 60);

            newPeriodData.Day = jour;
            newPeriodData.data = weatherData;
            newPeriodData.Hour = heures;
            newPeriodData.Minits = minutes;
            newPeriodData.Seconds = secondes;

            wethersInDays.Add(newPeriodData);
        }
        return wethersInDays;
    }

    public WeatherData GetWeatherAtTime(int day, float hour, List<float> timesOffset)
    {
        float startInSeconds = timesOffset[day];
        List<PeriodData> _identifiedDatas = new List<PeriodData>();
        _identifiedDatas = WeathersInDays.Where(x => x.Day == day).ToList();

        List<int> indexes = new List<int>();

        for (int i = 0; i < _identifiedDatas.Count; i++)
        {
            indexes.Add(WeathersInDays.IndexOf(_identifiedDatas[i]));
        }

        float offset = 0f;
        PeriodData fromMeteo = new PeriodData();
        PeriodData toMeteo = new PeriodData();

        if (_identifiedDatas.Count <= 0)
        {
            // Indicateur si aucun jour trouvé dans les périodes précédentes
            bool noPreviousDayFound = true;
            bool noNextDayFound = true;

            // Trouver le jour précédent immédiatement
            for (int i = day - 1; i >= 0; i--)
            {
                fromMeteo = WeathersInDays.Find(x => x.Day == i);
                if (fromMeteo.data.weatherInitialDuration != 0) // Si une météo est trouvée pour ce jour
                {
                    noPreviousDayFound = false;
                    break;
                }
            }

            // Trouver le jour suivant immédiatement
            for (int i = day + 1; i < gameSettings.TotalDays; i++)
            {
                toMeteo = WeathersInDays.Find(x => x.Day == i);
                if (toMeteo.data.weatherInitialDuration != 0) // Si une météo est trouvée pour ce jour
                {
                    noNextDayFound = false;
                    break;
                }
            }

            if (day == gameSettings.TotalDays || noNextDayFound)
            {
                toMeteo = WeathersInDays[0];
                noNextDayFound = false;
            }

            // Si aucune météo n'est trouvée pour les jours précédents ou suivants
            if (noPreviousDayFound || noNextDayFound)
            {
                //Debug.LogError("Erreur : Impossible de trouver une météo précédente ou suivante pour le jour spécifié. " + day);
                return default; // Gestion d'erreur, peut renvoyer une valeur par défaut
            }

            offset = _dayNightManager.TimeTo(fromMeteo.Hour, fromMeteo.Minits, fromMeteo.Seconds, fromMeteo.Day, day, hour);
            //Debug.Log($"day {fromMeteo.Day}, à {toMeteo.Day}, pour {day} il y'aura un offset de: " + offset);
            return InterpolateWeatherData(fromMeteo, toMeteo, offset);
        }
        else
        {

            //pas chercher en fonction des jours mais plutot du start at time
            fromMeteo = WeathersInDays[indexes[0]];
            if (indexes[0] + 1 < WeathersInDays.Count)
            {
                toMeteo = WeathersInDays[indexes[0] + 1];
            }

            if (fromMeteo.data.startAtTime > startInSeconds)
            {
                toMeteo = fromMeteo;
                fromMeteo = WeathersInDays[indexes[0] - 1];
            }

            if (day == gameSettings.TotalDays || day == WeathersInDays[WeathersInDays.Count - 1].Day)
            {
                toMeteo = WeathersInDays[0];
            }

            offset = _dayNightManager.TimeTo(fromMeteo.Hour, fromMeteo.Minits, fromMeteo.Seconds, fromMeteo.Day, day, hour);
        }

        return InterpolateWeatherData(fromMeteo, toMeteo, offset);
    }

    #endregion

    #region INTERPOLATED FUNCTIONS

    private WeatherData InterpolateWeatherData(PeriodData deMeteo, PeriodData versMeteo, float offset)
    {
        float timeRemaining = deMeteo.data.weatherInitialDuration - offset;

        WeatherData interpolatedWeatherData = new WeatherData
        {
            weatherInitialDuration = deMeteo.data.weatherInitialDuration,
            humidity = GetLerpedFloatAtTime(timeRemaining, deMeteo.data.humidity, versMeteo.data.humidity, deMeteo.data.weatherInitialDuration),
            airTemperature = GetLerpedFloatAtTime(timeRemaining, deMeteo.data.airTemperature, versMeteo.data.airTemperature, deMeteo.data.weatherInitialDuration),
            waterTemperature = GetLerpedFloatAtTime(timeRemaining, deMeteo.data.waterTemperature, versMeteo.data.waterTemperature, deMeteo.data.weatherInitialDuration),
            atmosphericPressure = GetLerpedFloatAtTime(timeRemaining, deMeteo.data.atmosphericPressure, versMeteo.data.atmosphericPressure, deMeteo.data.weatherInitialDuration),
            windSpeed = GetLerpedFloatAtTime(timeRemaining, deMeteo.data.windSpeed, versMeteo.data.windSpeed, deMeteo.data.weatherInitialDuration),
            windOrientationValue = GetLerpedFloatAtTime(timeRemaining, deMeteo.data.windOrientationValue, versMeteo.data.windOrientationValue, deMeteo.data.weatherInitialDuration),
            windDirection = _weatherManager.DetermineWindDirection(versMeteo.data.windOrientationValue),
            weatherType = deMeteo.data.weatherType
        };

        return interpolatedWeatherData;
    }

    //Start + (end - start) * (time / duration)
    private float GetLerpedFloatAtTime(float timeRemaining, float startValue, float endValue, float totalDuration)
    {
        return (float)Math.Round(startValue + (endValue - startValue) * (1 - (timeRemaining / totalDuration)), 2);
    }

    #endregion
}
