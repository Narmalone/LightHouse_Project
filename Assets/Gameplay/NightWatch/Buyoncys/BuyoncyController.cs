using LightHouse.Features.Sonar.Core;
using LightHouse.Features.Nightwatch;
using LightHouse.Features.Weather;
using LightHouse.Features.TimeOfDay.TimeCore;

using System;
using UnityEngine;
using LightHouse.Core.Utilities;
using UnityEngine.Rendering.HighDefinition;


namespace LightHouse.Features.Buyoncies
{
    /// <summary>
    /// Contrôleur d'une bouée (Buyoncy) dans le monde.
    /// Gère son état de vie/mort, sa détection sonar et sa réaction à l'environnement.
    /// </summary>
    public class BuyoncyController : MonoBehaviour, ISonarable
    {
        #region Serialized Fields

        [Header("References")]
        [SerializeField] private Rigidbody _rigidbody;
        [SerializeField] private WeatherConfigDatabase _weatherDatabase;
        [SerializeField] private SO_NightWatchConfiguration _nightWatchConfig;
        [SerializeField] private Light _lifeLight;
        [SerializeField] private FloaterControllers _floaterController;

        [Header("Lifetime Settings")]
        [SerializeField] private float _minInitialLifetime = 60f;
        [SerializeField] private float _maxInitialLifetime = 350f;

        [Header("Colors")]
        [SerializeField] private Color _aliveColor = Color.green;
        [SerializeField] private Color _deadColor = Color.red;

        [Header("Sonar Display")]
        [field: SerializeField] public Color DotColor { get; set; }
        [field: SerializeField] public Vector2 DotSize { get; set; }
        [field: SerializeField] public Sprite DotSprite { get; set; }
        [field: SerializeField] public string SonarInfo { get; set; }

        #endregion

        #region Public Properties

        public int BuyoncyID { get; set; } = -1;
        public int UniqueID { get; set; }
        public bool IsDetectedBySonar { get; set; }
        public Vector3 Position => _rigidbody.position;
        public Vector3 RotationAngles => _rigidbody.transform.eulerAngles;

        public float CurrentSpeed { get; private set; } = 1.0f;
        public float CurrentLifeTime { get; private set; }
        public bool IsAlive => CurrentLifeTime > 0f;
        public bool HasBeenRepairedToday { get; set; } = false;

        public string Name => gameObject.name;

        #endregion

        #region Events

        public event Action<BuyoncyController> OnBroken;
        public event Action OnRepaired;

#pragma warning disable
        public Action ForceDotUpdate { get; set; }

        #endregion

        #region Private Fields

        private Timer _timer;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            InitializeTimer();
            RegisterEvents();
            SonarHandlerData.Register(this);
            _lifeLight.color = _aliveColor;
            _timer.StartTimer();
            SonarInfo = $"#{BuyoncyID:D2}";
        }

        public void UpdateFromManager()
        {
            if (HasBeenRepairedToday) return;
            _timer.Tick(Time.deltaTime, CurrentSpeed);
            CurrentLifeTime = _timer.GetTimeRemaining();
        }

        private void OnDestroy()
        {
            UnregisterEvents();
            SonarHandlerData.Unregister(this);
        }

        #endregion

        #region Initialization

        private void InitializeTimer()
        {
            _timer = new Timer(GetRandomLifetime());
            CurrentLifeTime = _timer.GetTimeRemaining();
            _timer.OnTimerComplete += OnTimerCompleted;
        }

        public void Initialize(WaterSurface surface)
        {
            _floaterController.SetWaterSurface(surface);
        }

        private void RegisterEvents()
        {
            WeatherHandlerData.OnWeatherTypeChanged += OnWeatherChanged;
            TimeHandlerData.OnTimeSegmentChanged += OnTimeSegmentChanged;
        }

        private void UnregisterEvents()
        {
            WeatherHandlerData.OnWeatherTypeChanged -= OnWeatherChanged;
            TimeHandlerData.OnTimeSegmentChanged -= OnTimeSegmentChanged;
        }

        #endregion

        #region Event Handlers

        private void OnWeatherChanged(WeatherType type)
        {
            var weatherParameters = _weatherDatabase.GetDefinition(type);
            CurrentSpeed = weatherParameters.DangerLevel;
        }

        private void OnTimeSegmentChanged(TimeOfDaySegment segment)
        {
            if (segment == TimeOfDaySegment.Morning)
            {
                if (!IsAlive)
                    Repair();

                HasBeenRepairedToday = false;
            }
        }

        private void OnTimerCompleted()
        {
            BreakDown();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Casse la bouée et déclenche l’événement OnBroken.
        /// </summary>
        public void BreakDown()
        {
            OnBroken?.Invoke(this);
            _lifeLight.color = _deadColor;
        }

        /// <summary>
        /// Répare la bouée et redémarre son cycle de vie.
        /// </summary>
        public void Repair()
        {
            _timer.ResetTimer(GetRandomLifetime());
            OnRepaired?.Invoke();
            _lifeLight.color = _aliveColor;
        }

        public void SetColor(Color targetColor, bool useAliveColor = false, bool useDeadColor = false)
        {
            Color selectedColor = targetColor;
            if(useAliveColor)
                selectedColor = _aliveColor;
            else if (useDeadColor)
                selectedColor = _deadColor;
            else
                selectedColor = targetColor;
        }

        #endregion

        #region Utility

        private float GetRandomLifetime()
        {
            return UnityEngine.Random.Range(_minInitialLifetime, _maxInitialLifetime);
        }

        #endregion
    }
}
