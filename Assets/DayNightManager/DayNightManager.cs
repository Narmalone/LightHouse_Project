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

    [SerializeField] private float _speedMultiplier;

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
        SetSpeedWithStateDuration(gameSettings.DayCycleDuration.DurationScale);
        UpdateDayDisplay();
        _readyMorning = true;
        _lightTransform = _sunLight.transform;
        SetTime(_homeTime);
    }

    private void Update()
    {
        if (_isDayUpdating == false) return;

        var value = CurrentTime + Time.deltaTime / 3600 * _speedMultiplier * _initialSpeedMultiplier * GameManager.GlobalSpeedTime;
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
        if (currentDay >= gameSettings.TotalDays)
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
        timeUntil /= _speedMultiplier * _initialSpeedMultiplier * GameManager.GlobalSpeedTime;

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

    public float TimeTo(float fromHour, float fromMinits, float fromSeconds, int fromDay, int toDay, float toHour)
    {
        // Convertir le temps de départ en secondes depuis le début de la journée
        float fromTimeInSeconds = (fromHour * 3600f) + (fromMinits * 60f) + fromSeconds;

        // Convertir l'heure cible en secondes depuis le début de la journée
        float toTimeInSeconds = toHour * 3600f;

        // Calculer le nombre de jours d'écart
        int dayDifference = toDay - fromDay;

        // Si on est le même jour mais que l'heure cible est avant l'heure actuelle
        if (dayDifference == 0 && toTimeInSeconds < fromTimeInSeconds)
        {
            // Ajouter une journée entière en secondes (puisque toHour est passé au lendemain)
            toTimeInSeconds += 24 * 3600f;
        }

        // Calculer la différence de temps en secondes
        float timeDifferenceInSeconds = toTimeInSeconds - fromTimeInSeconds;

        // Si on passe plusieurs jours
        if (dayDifference > 0)
        {
            // Ajouter les jours supplémentaires en secondes
            timeDifferenceInSeconds += dayDifference * 24 * 3600f; // Chaque jour a 24 heures (en secondes)
        }

        // Prendre en compte la vitesse du cycle jour-nuit (si nécessaire)
        timeDifferenceInSeconds /= _speedMultiplier * _initialSpeedMultiplier * GameManager.GlobalSpeedTime;

        return timeDifferenceInSeconds;
    }
}