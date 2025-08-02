using LightHouse.Game.Computer.NightWatch.Sonar;
using LightHouse.Game.DayNightSystem;
using LightHouse.Weather;
using System;
using UnityEngine;

public class BuyoncyController : MonoBehaviour, ISonarable
{
    [SerializeField] private Rigidbody _rb;
    [SerializeField] private WeatherConfigDatabase _weatherDefinitionDatabase;
    [SerializeField] private NightWatchConfiguration _nightWatchConfig;
    [SerializeField] private float _minimumInitialLifeTime = 60f;
    [SerializeField] private float _maximumInitialLifeTime = 350f;

    [SerializeField] private Color _aliveColor = Color.green;
    [SerializeField] private Color _deadColor = Color.red;
    [SerializeField] private Light _lifeLight;
    public int BuyoncyID = -1;

    public event Action<BuyoncyController> OnBroken;
    public event Action OnRepaired;
    public event Action ForceDotUpdate;

    public float CurrentSpeed { get; set; } = 1.0f;

    public string Name => this.gameObject.name;

    public int UniqueID { get; set; }
    public bool IsDetectedBySonar { get ; set; }

    public Vector3 Position => _rb.position;

    public Vector3 RotationAngles => _rb.transform.eulerAngles;

    [field: SerializeField] public Color DotColor { get; set; }
    [field: SerializeField] public Vector2 DotSize { get; set; }
    [field: SerializeField] public Sprite DotSprite { get; set; }
    [field: SerializeField] public string SonarInfo { get; set; }

    private Timer _timer;
    public float CurrentLifeTime;

    public bool IsAlive => CurrentLifeTime > 0f;

    private void Awake()
    {
        _timer = new Timer(GetRandomLifeTime());
        CurrentLifeTime = _timer.GetTimeRemaining();
        _timer.OnTimerComplete += OnTimerCompleted;
        WeatherHandlerData.OnWeatherTypeChanged += OnWeatherChanged;
        SonarHandlerData.Register(this);
        TimeHandlerData.OnTimeSegmentChanged += OnTimeSegmentChanged;
    }

    private void OnTimeSegmentChanged(TimeOfDaySegment segment)
    {
        if(!IsAlive)
        {
            Repaired();
        }
    }

    private void OnDestroy()
    {
        WeatherHandlerData.OnWeatherTypeChanged -= OnWeatherChanged;
        SonarHandlerData.Unregister(this);
    }

    private void OnWeatherChanged(WeatherType type)
    {
        var weatherParameters = _weatherDefinitionDatabase.GetDefinition(type);
        CurrentSpeed = weatherParameters.DangerLevel;
    }

    private void Start()
    {
        _timer.StartTimer();
        _lifeLight.color = _aliveColor;
        SonarInfo = "#" + BuyoncyID.ToString("D2");
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
        OnBroken?.Invoke(this);
        _lifeLight.color = _deadColor;
    }

    public void Repaired()
    {
        _timer.ResetTimer(GetRandomLifeTime());
        OnRepaired?.Invoke();
        _lifeLight.color = _aliveColor;
    }
}
