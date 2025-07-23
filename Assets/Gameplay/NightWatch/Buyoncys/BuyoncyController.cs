using LightHouse.Game.DayNightSystem;
using LightHouse.Weather;
using System;
using UnityEngine;

public class BuyoncyController : MonoBehaviour
{
    [SerializeField] private WeatherConfigDatabase _weatherDefinitionDatabase;
    [SerializeField] private NightWatchConfiguration _nightWatchConfig;
    [SerializeField] private float _minimumInitialLifeTime = 60f;
    [SerializeField] private float _maximumInitialLifeTime = 350f;

    public event Action OnBroken;
    public event Action OnRepaired;

    public float CurrentSpeed { get; set; } = 1.0f;

    private Timer _timer;
    public float CurrentLifeTime;

    private void Awake()
    {
        _timer = new Timer(GetRandomLifeTime());
        _timer.OnTimerComplete += OnTimerCompleted;
        WeatherHandlerData.OnWeatherTypeChanged += OnWeatherChanged;
    }

    private void OnDestroy()
    {
        WeatherHandlerData.OnWeatherTypeChanged -= OnWeatherChanged;
    }

    private void OnWeatherChanged(WeatherType type)
    {
        var weatherParameters = _weatherDefinitionDatabase.GetDefinition(type);
        CurrentSpeed = weatherParameters.DangerLevel;
    }

    private void Start()
    {
        _timer.StartTimer();
    }

    private void Update()
    {
        if(TimeUtility.IsTimeInRange(TimeHandlerData.CurrentTime, _nightWatchConfig.BuyoncysDecayStartHour, _nightWatchConfig.BuyoncysDecayEndHour))
        {
            _timer.Tick(Time.deltaTime, CurrentSpeed);
            CurrentLifeTime = _timer.GetTimeRemaining();
        }
    }

    public float GetRandomLifeTime()
    {
        return UnityEngine.Random.Range(_minimumInitialLifeTime, _maximumInitialLifeTime);
    }

    private void OnTimerCompleted()
    {
        BreakDown();
    }

    public void BreakDown()
    {
        OnRepaired?.Invoke();        
    }

    public void Repaired()
    {
        _timer.ResetTimer(GetRandomLifeTime());
        OnRepaired?.Invoke();
    }
}
