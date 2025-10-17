using LightHouse.Game.DayNightSystem;
using LightHouse.Weather.Utils;
using System.Collections.Generic;
using UnityEngine;

namespace LightHouse.Weather
{
    /// <summary>
    /// Pilote l'état météo courant en interpolant entre deux segments de la timeline.
    /// </summary>
    public class WeatherManager : MonoBehaviour
    {
        #region Serialized Fields

        [Header("Config")]
        [SerializeField] private WeatherConfigDatabase _configDatabase;

        [Header("Timeline")]
        [SerializeField] public WeatherTimeline Timeline;

        [Header("Time")]
        [SerializeField] private TimeConfiguration _timeConfig;

        #endregion

        #region Runtime State

        [field: SerializeField] public WeatherData CurrentWeather { get; private set; }

        private WeatherData _from;
        [SerializeField] private WeatherData _to;

        public WeatherData FromWeather => _from;
        public WeatherData ToWeather => _to;

        public int _currentIndex;
        private float _secondsPerDay;
        private float _currentGameSeconds;
        private float _segmentStart;
        private float _segmentEnd;

        private WeatherType _lastEmittedType;

        /// <summary>Progression 0..1 à l'intérieur du segment courant.</summary>
        public float CurrentBlend =>
            Mathf.Clamp01((_currentGameSeconds - _segmentStart) / Mathf.Max(1e-5f, _from.DurationInSeconds));

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            _secondsPerDay = _timeConfig.RealSecondsPerGameDay;
            Timeline.GenerateTimeline(_configDatabase, _timeConfig);

            // Init segment 0
            _currentIndex = 0;
            if (Timeline.Weathers.Count < 2)
            {
                Debug.LogError("[WeatherManager] Timeline insuffisante (<2 segments).");
                return;
            }

            _from = Timeline.Weathers[_currentIndex];
            _to = Timeline.Weathers[_currentIndex + 1];
            _segmentStart = _from.StartTimeInSeconds;
            _segmentEnd = _segmentStart + _from.DurationInSeconds;

            // État initial cohérent
            _currentGameSeconds = ComputeCurrentGameSeconds();
            CurrentWeather = WeatherUtils.LerpWeatherData(_from, _to, CurrentBlend);
            _lastEmittedType = CurrentWeather.WeatherType;
            WeatherHandlerData.SetCurrentWeatherDatas(CurrentWeather);
        }

        private void Update()
        {
            _currentGameSeconds = ComputeCurrentGameSeconds();

            // ⚠️ Utiliser >= (frontières exactes)
            AdvanceSegmentIfNeeded();

            // Interpolation
            var t = CurrentBlend;
            var previousType = CurrentWeather.WeatherType;

            CurrentWeather = WeatherUtils.LerpWeatherData(_from, _to, t);

            // Notification de changement de type
            if (previousType != CurrentWeather.WeatherType)
            {
                _lastEmittedType = CurrentWeather.WeatherType;
                WeatherHandlerData.OnWeatherTypeChanged?.Invoke(_lastEmittedType);
            }

            // Pousse l'état courant (même si le type n'a pas changé)
            WeatherHandlerData.SetCurrentWeatherDatas(CurrentWeather);
        }

        private void OnDestroy()
        {
            Timeline.Weathers.Clear();
        }

        #endregion

        #region Core Logic

        /// <summary>Calcule les secondes de jeu courantes (jour+heure) de façon monotone.</summary>
        private float ComputeCurrentGameSeconds()
        {
            // CurrentTime supposé en heures [0..24)
            return TimeHandlerData.CurrentDay * _secondsPerDay
                 + (TimeHandlerData.CurrentTime / 24f) * _secondsPerDay;
        }

        /// <summary>Fait avancer l'index de segment jusqu'à contenir l'instant courant.</summary>
        private void AdvanceSegmentIfNeeded()
        {
            // Avance tant qu'on a dépassé la fin du segment courant
            // (>= pour capter la frontière exacte)
            while (_currentGameSeconds >= _segmentEnd && _currentIndex < Timeline.Weathers.Count - 2)
            {
                _currentIndex++;
                _from = Timeline.Weathers[_currentIndex];
                _to = Timeline.Weathers[_currentIndex + 1];

                _segmentStart = _from.StartTimeInSeconds;
                _segmentEnd = _segmentStart + _from.DurationInSeconds;
            }
        }

        #endregion
    }
}
