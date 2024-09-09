using UnityEngine;
using System.Collections.Generic;
using System.Linq;


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
    [SerializeField] private CustomEvent _eventMidNight;

    [Header("Forecast Settings")]
    [SerializeField] private float morningStart = 6f;
    [SerializeField] private float middayStart = 12f;
    [SerializeField] private float eveningStart = 18f;

    [Header("START OFFSETS IN SECONDS FROM GAME START")]
    public float StartoffSetMorning;
    public float StartoffSetMidDay;
    public float StartoffSetEvening;

    [Header("ALL NEXTS X HOURS IN SECONDS")]
    public List<float> MorningAtTime = new List<float>();
    public List<float> MiddayAtTime = new List<float>();
    public List<float> EveningAtTime = new List<float>();

    [Header("Weathers Separated by Days")]
    public List<PeriodData> WeathersInDays = new List<PeriodData>();

    [Header("VALUES FOR")]
    public List<WeatherData> MorningX = new();
    public List<WeatherData> MiddaysX = new();
    public List<WeatherData> EveningsX = new();
    #endregion

    #region PRIVATE FIELDS

    private bool _isInitialized = false;
    #endregion

    #region MONO'S CALLBACKS

    protected override void Awake()
    {
        base.Awake();
        _onWeatherGenerated.handle += _onWeatherUpdate_handle;
    }

    private void OnDestroy()
    {
        _onWeatherGenerated.handle -= _onWeatherUpdate_handle;
    }

    #endregion

    #region DELEGATES

    private void _onWeatherUpdate_handle(WeatherType obj)
    {
        if (!_isInitialized)
        {
            //Calculer les offsets de départs en fonction de quand on commence lejeu
            StartoffSetMorning = _dayNightManager.TimeUntil(morningStart);
            StartoffSetMidDay = _dayNightManager.TimeUntil(middayStart);
            StartoffSetEvening = _dayNightManager.TimeUntil(eveningStart);

            //Calculer à quelles secondes après le début du jeu tombe X heure sur X jour
            MorningAtTime = GetStepsAtTime(StartoffSetMorning, gameSettings.TotalDays, gameSettings.DayCycleDuration.Seconds);
            MiddayAtTime = GetStepsAtTime(StartoffSetMidDay, gameSettings.TotalDays, gameSettings.DayCycleDuration.Seconds);
            EveningAtTime = GetStepsAtTime(StartoffSetEvening, gameSettings.TotalDays, gameSettings.DayCycleDuration.Seconds);

            //Calculer quelles météos tombent quels jours et à quelle heure
            WeathersInDays = GetWeathersInDays(_weatherManager.weatherForecast, _dayNightManager._homeTime, gameSettings.DayCycleDuration.Seconds);

            //Calculer et interpoler les météos aux heures voulus pour avoir une prévision météorologique
            for(int i = 0; i < gameSettings.TotalDays; i++)
            {
                MorningX.Add(GetWeatherAtTime(i, morningStart, MorningAtTime));
                MiddaysX.Add(GetWeatherAtTime(i, middayStart, MiddayAtTime));
                EveningsX.Add(GetWeatherAtTime(i, eveningStart, EveningAtTime));
            }

            _isInitialized = true;
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
            PeriodData previous = new PeriodData();
            PeriodData nextData = new PeriodData();

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

            // Si aucune météo n'est trouvée pour les jours précédents ou suivants
            if (noPreviousDayFound || noNextDayFound)
            {
                Debug.LogError("Erreur : Impossible de trouver une météo précédente ou suivante pour le jour spécifié. " + day);
                return default; // Gestion d'erreur, peut renvoyer une valeur par défaut
            }

            offset = _dayNightManager.TimeTo(fromMeteo.Hour, fromMeteo.Minits, fromMeteo.Seconds, fromMeteo.Day, day, hour);
            Debug.Log($"day {fromMeteo.Day}, à {toMeteo.Day}, pour {day} il y'aura un offset de: " + offset);
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
        return startValue + (endValue - startValue) * (1 - (timeRemaining / totalDuration));
    }

    #endregion
}
