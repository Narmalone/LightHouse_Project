using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class DayNightManager : MonoBehaviour
{
    public enum DayState
    {
        MORNING,
        MID_DAY,
        EVENING,
        MID_NIGHT
    }


    [Header("State")]
    [SerializeField] private DayState _state;

    [Header("Components")]
    [SerializeField] private Light _sunLight;
    [SerializeField] private Light _moonLight;
    [SerializeField] private Material _skyBox;
    [SerializeField] private TextMeshProUGUI _dayCount;
    [SerializeField] private TextMeshProUGUI _timeDisplay;

    [Header("Event")]
    [SerializeField] private CustomEvent _eventStartTimeCycle;
    [SerializeField] private CustomEvent _eventMorning;
    [SerializeField] private CustomEvent _eventMidDay;
    [SerializeField] private CustomEvent _eventEvening;
    [SerializeField] private CustomEvent _eventMidNight;
    [SerializeField] private CustomEvent_Float _eventSetTime;
    [SerializeField] private CustomEvent _onWeatherLoaded;

    [Header("Color")]
    [SerializeField] private Gradient _colorSunOverTime;
    [SerializeField] private Gradient _colorSkyBoxOverTime;
    [SerializeField] private Gradient _colorFogOverTime;

    [Header("Color Environment")]
    [SerializeField] private Gradient _skyColorOverTime;
    [SerializeField] private Gradient _equatorOverTime;
    [SerializeField] private Gradient _groundFogOverTime; 

    [Header("Fog")]
    [SerializeField] private AnimationCurve _sunRotationSpeed;
    [SerializeField] private AnimationCurve _sunIntensity;
    [SerializeField] private AnimationCurve _fogAmount;

    [Header("Time")]
    [SerializeField, Range(0, 24)] public float _homeTime;
    [SerializeField] private int currentDay = 0;

    [Header("Stats")]
    [SerializeField] private Vector3 _startOrientation;
    [SerializeField] private Vector3 _sunOrientation;
    [SerializeField, Tooltip("In Hour"), Range(0, 24)] public float _currentTime;
    [SerializeField, Range(0, 100), ConsoleVariable("TimeSpeed"), ConsoleCategory("Gameplay")] public float _initialSpeedMultiplier;
    [SerializeField] public bool _isDayUpdating;
    [SerializeField] private bool _debug;

    private float _speedMultiplier;

    private bool _readyMorning;
    private bool _readyMidday;
    private bool _readyEvening;
    private bool _readyMidnight;

    private Transform _lightTransform;
    private GameSettings gameSettings;


    public DayState State
    {
        get { return _state; }
        set { _state = value; }
    }

    public float TimeBeforeMidday
    {
        get
        {
            if (CurrentTime < 12)
            {
                return 12 - CurrentTime;
            }
            else
            {
                return 12 - (CurrentTime - 12);
            }
        }
    }

    public float TimeBeforeMiddayInSeconds
    {
        get
        {
            float timeBeforeMidday = TimeBeforeMidday;
            return timeBeforeMidday * 3600;
        }
    }

    private float CurrentTime
    {
        get { return _currentTime; }
        set
        {
            _currentTime = value % 24;

            switch (value)
            {
                case > 6 when _readyMorning:
                    _readyMorning = false;
                    _readyMidday = true;
                    _eventMorning.Raise();
                    State = DayState.MORNING;
                    break;
                case > 12 when _readyMidday:
                    _readyMidday = false;
                    _readyEvening = true;
                    _eventMidDay.Raise();
                    State = DayState.MID_DAY;
                    break;
                case > 18 when _readyEvening:
                    _readyEvening = false;
                    _readyMidnight = true;
                    _eventEvening.Raise();
                    State = DayState.EVENING;
                    break;
                case < 1 when _readyMidnight:
                    _readyMidnight = false;
                    _readyMorning = true;
                    _eventMidNight.Raise();
                    State = DayState.MID_NIGHT;
                    AddDay();
                    break;
            }
        }
    }

    private void OnValidate()
    {
        if (_debug == false) return;

        CurrentTime = _currentTime;
        _lightTransform = _sunLight.transform;
        UpdateStats();
    }

    private void Awake()
    {
        _eventStartTimeCycle.handle += OnStartTimeCycle;
        _eventSetTime.handle += OnSetTime;
        _onWeatherLoaded.handle += _onWeatherLoaded_handle;
    }

    private void _onWeatherLoaded_handle()
    {
        _isDayUpdating = true;
    }

    private void OnDestroy()
    {
        _eventStartTimeCycle.handle -= OnStartTimeCycle;
        _eventSetTime.handle -= OnSetTime;
        _onWeatherLoaded.handle -= _onWeatherLoaded_handle;
    }

    private void Start()
    {
        gameSettings = GameManager.Instance.gameSettings;
        SetSpeedWithStateDuration(gameSettings.DayCycleDuration.Duration);
        UpdateDayDisplay();
        _readyMorning = true;
        _lightTransform = _sunLight.transform;
        SetTime(_homeTime);
    }

    private void Update()
    {
        if (_isDayUpdating == false) return;

        var value = CurrentTime + Time.deltaTime / 3600 * _speedMultiplier * _initialSpeedMultiplier;

        if (value < CurrentTime) return;

        CurrentTime = value;

        UpdateStats();
    }

    private void UpdateStats()
    {
        UpdateTimeDisplay();

        var percent = CurrentTime / 24;

        _lightTransform.eulerAngles = _startOrientation + _sunOrientation * 360 * percent * _sunRotationSpeed.Evaluate(percent);

        _sunLight.color = _colorSunOverTime.Evaluate(percent);
        _sunLight.intensity = _sunIntensity.Evaluate(percent);

        return;

        //Fog
        RenderSettings.ambientSkyColor = _skyColorOverTime.Evaluate(percent);
        RenderSettings.ambientEquatorColor = _equatorOverTime.Evaluate(percent);
        RenderSettings.ambientGroundColor = _groundFogOverTime.Evaluate(percent);
        RenderSettings.fogColor = _colorFogOverTime.Evaluate(percent);
        RenderSettings.fogDensity = _fogAmount.Evaluate(percent);
    }

    private void SetSpeedWithStateDuration(float duration)
    {
        _speedMultiplier = duration;
    }
    public void SetTime(float time)
    {
        if (CurrentTime > time) AddDay();
        CurrentTime = time;
        UpdateStats();
    }

    private void UpdateTimeDisplay()
    {
        TimeSpan timeSpan = TimeSpan.FromSeconds(CurrentTime*3600);
        _timeDisplay.text = string.Format("{0:D2}:{1:D2}:{2:D2}", timeSpan.Hours, timeSpan.Minutes, timeSpan.Seconds);
    }

    private void UpdateDayDisplay()
    {
        _dayCount.text = $"Day: {currentDay}";
    }

    private void OnSetTime(float value)
    {
        SetTime(value);
    }

    private void OnStartTimeCycle()
    {
        _isDayUpdating = true;
    }
    private void AddDay()
    {
        currentDay++;
        if(currentDay >= gameSettings.TotalDays)
        {
            Debug.Log("LE NB DE JOUR MAX A ETE COMPTE");
        }
        UpdateDayDisplay();
    }

    public float TimeUntil(float targetHour)
    {
        float currentTime = CurrentTime;
        float timeUntil = 0;

        if (currentTime < targetHour)
        {
            timeUntil = (targetHour - currentTime) * 3600;
        }
        else
        {
            timeUntil = (24 - currentTime + targetHour) * 3600;
        }

        // Prendre en compte la vitesse du cycle jour-nuit
        timeUntil /= _speedMultiplier * _initialSpeedMultiplier;

        return timeUntil;
    }

    public float TimeTo(float fromHour, float toHour)
    {
        // Normaliser les heures pour qu'elles soient comprises entre 0 et 24
        fromHour = fromHour % 24;
        toHour = toHour % 24;

        // Calculer la différence en heures
        float hourDiff = toHour - fromHour;

        // Si la différence est négative, cela signifie que nous devons passer à la journée suivante
        if (hourDiff < 0)
        {
            hourDiff += 24;
        }

        // Convertir la différence en heures en secondes
        float timeTo = hourDiff * 3600;

        // Prendre en compte la vitesse du cycle jour-nuit
        timeTo /= _speedMultiplier * _initialSpeedMultiplier;

        return timeTo;
    }

    public string TimeUntilEvent(float seconds)
    {
        float timeUntil = seconds;

        // Prendre en compte la vitesse du cycle jour-nuit
        timeUntil *= _speedMultiplier * _initialSpeedMultiplier;

        // Calculer les jours, heures et minutes
        int days = (int)(timeUntil / (3600 * 24));
        int hours = (int)((timeUntil % (3600 * 24)) / 3600);
        int minutes = (int)((timeUntil % 3600) / 60);

        // Retourner le temps restant sous forme de chaîne
        return $"{days} jours, {hours} heures et {minutes} minutes";
    }

    [System.Serializable]
    public struct PeriodData
    {
        public WeatherData data;
        public float Hour;
        public float Minits;
        public float Seconds;
        public int Day;
    }

    public List<PeriodData> periodDatas = new List<PeriodData>();
    public void AfficherHeureMeteo(List<WeatherData> weatherDatas, float homeTime)
    {
        float tempsTotalJournee = 400f; // Temps total d'une journée en secondes

        foreach (WeatherData weatherData in weatherDatas)
        {
            PeriodData newPeriodData = new PeriodData();
            // Calculer le jour, l'heure, les minutes et les secondes où la météo sera lancée
            int jour = (int)(weatherData.startAtTime / tempsTotalJournee);
            float heure = (weatherData.startAtTime % tempsTotalJournee) / tempsTotalJournee * 24;
            newPeriodData.Day = jour;
            heure += homeTime; // Ajouter l'offset du homeTime
            if (heure >= 24)
            {
                heure -= 24;
                jour++;
            }
            int heures = (int)heure;
            int minutes = (int)((heure - heures) * 60);
            int secondes = (int)(((heure - heures) * 60 - minutes) * 60);

            newPeriodData.data = weatherData;
            newPeriodData.Hour = heure;
            newPeriodData.Minits = minutes;
            newPeriodData.Seconds = secondes;

            periodDatas.Add(newPeriodData);
            // Afficher le jour, l'heure, les minutes et les secondes où la météo sera lancée
            //Debug.Log($"La météo {weatherData.weatherType} sera lancée le jour {jour + 1} à {heures:D2}:{minutes:D2}:{secondes:D2} en jeu.");
        }
    }

    int GetTotalTimeInSeconds(PeriodData periodData)
    {
        int totalTimeInSeconds = periodData.Day * 24 * 60 * 60; // days to seconds
        totalTimeInSeconds += (int)periodData.Hour * 60 * 60; // hours to seconds
        totalTimeInSeconds += (int)periodData.Minits * 60; // minutes to seconds
        totalTimeInSeconds += (int)periodData.Seconds; // seconds
        return totalTimeInSeconds;
    }

    public List<PeriodData> identifiedDatas;
    public PeriodData deMeteo;
    public PeriodData versMeteo;
    public WeatherData GetWeatherAtTime(int day, float hour)
    {
        // Convert the hour to seconds since the start of the day
        float targetTime = (int)hour * 400 + (hour % 1) * (400 / 24);

        identifiedDatas = new List<PeriodData>();
        identifiedDatas = periodDatas.Where(x => x.Day == day).ToList();
        List<int> indexes = new List<int>();
        for(int i = 0; i < identifiedDatas.Count; i++)
        {
            indexes.Add(periodDatas.IndexOf(identifiedDatas[i]));
        }

        //cela veut dire qu'une ancienne météo va couvrir toute la  journée !
        //IL FAUT DONC FAIRE ATTENTION AVEC L'INDEX PAS BETEMENT METTRE LE DAY
        //ON RECUPERE L'INDEX DE LA METEO PRECEDENTE, si on a rien on regarde si on a un jour précédant
        //ou jour suivant

        if (identifiedDatas.Count <= 0)
        {
            /*PeriodData previous;
            PeriodData nextData;
            List<int> caca = new List<int>();

            previous = periodDatas.Find(x => x.Day < day);
            nextData = periodDatas.Find(x => x.Day > day);*/

            PeriodData previous = new PeriodData();
            PeriodData nextData = new PeriodData();

            // Trouver le jour précédent immédiatement
            for (int i = day - 1; i >= 0; i--)
            {
                deMeteo = periodDatas.Find(x => x.Day == i);
            }

            // Trouver le jour suivant immédiatement
            for (int i = day + 1; i < gameSettings.TotalDays; i++)
            {
                versMeteo = periodDatas.Find(x => x.Day == i);
            }

            //SI VERS METEO EST EMPTY C QUON EST A LA FIN ET DONC IL FAUT PRENDRE LA DERNIERE METEO OU VOIR EN FONCTION

            //deMeteo = periodDatas[day - 1]; //si on met j 31 ça marchera pas car on a genre 19 / 20 élément dans la liste ça dépend
            //de la durée totale de une météo donc enfait on veut l'index ou le jour correspond
            //versMeteo = periodDatas[day + 1];

            Debug.Log("aucune météo dans le temps donnée assignation de period datas");
        }
        else
        {
            deMeteo = periodDatas[indexes[0]];
            if (indexes[0] + 1 < periodDatas.Count)
            {
                versMeteo = periodDatas[indexes[0] + 1];
            }
            
        }


        // Print the two weather periods that the target time falls between
        //Debug.Log("Entre météo du j: " + prevPeriodData.Day + ":" + prevPeriodData.Hour + "h - " + prevPeriodData.data.weatherType);
        //Debug.Log("Et météo du j: " + nextPeriodData.Day + ":" + nextPeriodData.Hour + "h - " + nextPeriodData.data.weatherType);


        // Calculate the time fraction between the two PeriodData elements
        float timeFraction = (targetTime - deMeteo.Hour * (400 / 24) - deMeteo.Minits * (400 / 24 / 60) - deMeteo.Seconds) /
                       (versMeteo.Hour * (400 / 24) + versMeteo.Minits * (400 / 24 / 60) + versMeteo.Seconds -
                        deMeteo.Hour * (400 / 24) - deMeteo.Minits * (400 / 24 / 60) - deMeteo.Seconds);

        // Interpolate the weather data using the time fraction
        WeatherData interpolatedWeatherData = new WeatherData();
        interpolatedWeatherData.weatherType = deMeteo.data.weatherType;
        interpolatedWeatherData.airTemperature = Mathf.Lerp(deMeteo.data.airTemperature, versMeteo.data.airTemperature, timeFraction);
        interpolatedWeatherData.waterTemperature = Mathf.Lerp(deMeteo.data.waterTemperature, versMeteo.data.waterTemperature, timeFraction);
        interpolatedWeatherData.windSpeed = Mathf.Lerp(deMeteo.data.windSpeed, versMeteo.data.windSpeed, timeFraction);
        interpolatedWeatherData.windOrientationValue = Mathf.Lerp(deMeteo.data.windOrientationValue, versMeteo.data.windOrientationValue, timeFraction);
        interpolatedWeatherData.humidity = Mathf.Lerp(deMeteo.data.humidity, versMeteo.data.humidity, timeFraction);

        return interpolatedWeatherData;
    }
    /*
        private float Remap(float value, float oldRangeMin, float oldRangeMax, float newRangeMin, float newRangeMax)
        {
            return newRangeMin + (value - oldRangeMin) * (newRangeMax - newRangeMin) / (oldRangeMax - oldRangeMin);
        }*/
}