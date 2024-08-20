using System;
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
    [SerializeField] private Material _skyBox;
    [SerializeField] private TextMeshProUGUI _dayCount;
    [SerializeField] private TextMeshProUGUI _timeDisplay;

    [Header("Event")]
    [SerializeField] private CustomEvent _eventStartTimeCycle;
    [SerializeField] private CustomEvent _eventMorning;
    [SerializeField] private CustomEvent _eventMidDay;
    [SerializeField] private CustomEvent _eventEvening;
    [SerializeField] private CustomEvent _eventMidNight;

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
    [SerializeField, Range(0, 24)] private float _homeTime;
    [SerializeField] private int currentDay = 0;

    [Header("Stats")]
    [SerializeField] private Vector3 _startOrientation;
    [SerializeField] private Vector3 _sunOrientation;
    [SerializeField, Tooltip("In Hour"), Range(0, 24)] private float _currentTime;
    [SerializeField, Range(0, 100), ConsoleVariable("TimeSpeed"), ConsoleCategory("Gameplay")] private float _initialSpeedMultiplier;
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
                    currentDay++;
                    UpdateDayDisplay();
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
    }

    private void OnDestroy()
    {
        _eventStartTimeCycle.handle -= OnStartTimeCycle;
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

    private void OnStartTimeCycle()
    {
        _isDayUpdating = true;
    }

    /*
        private float Remap(float value, float oldRangeMin, float oldRangeMax, float newRangeMin, float newRangeMax)
        {
            return newRangeMin + (value - oldRangeMin) * (newRangeMax - newRangeMin) / (oldRangeMax - oldRangeMin);
        }*/
}