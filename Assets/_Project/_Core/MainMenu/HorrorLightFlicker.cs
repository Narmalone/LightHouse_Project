using LightHouse.Core.Audio;
using LightHouse.Core.Services;
using UnityEngine;

namespace LightHouse.Features.Horror
{
    /// <summary>
    /// Gère un effet de lumière instable avec bruit continu + flickers aléatoires (ambiance horreur).
    /// </summary>
    public class HorrorLightFlicker : MonoBehaviour
    {
        #region ===== Dependencies =====

        [SerializeField] private Light _targetLight;

        #endregion

        #region ===== Settings =====

        [Header("Base Noise")]
        [SerializeField] private float _baseIntensity = 1f;
        [SerializeField] private float _noiseAmplitude = 0.2f;
        [SerializeField] private float _noiseSpeed = 1f;

        [Header("Flicker")]
        [SerializeField] private float _minFlickerInterval = 8f;
        [SerializeField] private float _maxFlickerInterval = 20f;
        [SerializeField] private float _flickerDuration = 0.08f;
        [SerializeField] private float _flickerIntensityFactor = 0.3f;

        [Header("Double Flicker")]
        [Range(0f, 1f)][SerializeField] private float _doubleFlickerChance = 0.3f;
        [SerializeField] private float _delayBetweenDoubleFlicker = 0.05f;

        [Header("Audio")]
        [SerializeField] private SO_AudioCue _flickerSound;
        [SerializeField] private SO_AudioCue _ambianceLight;

        #endregion

        #region ===== State =====

        private IAudioHandle _ambianceHandle;
        private IAudioHandle _flickerHandle;

        private float _timer;
        private float _nextFlickerTime;

        private int _remainingFlickers;

        private bool _isFlickering;
        private bool _isWaitingBetweenFlickers;

        #endregion

        #region ===== Unity Lifecycle =====

        private void Start()
        {
            Initialize();
        }

        private void Update()
        {
            float dt = Time.deltaTime;
            float time = Time.time;

            TryStartFlicker(time);
            UpdateFlicker(dt);

            if (IsBusy())
                return;

            ApplyNoise(time);
        }

        #endregion

        #region ===== Initialization =====

        private void Initialize()
        {
            if (_targetLight == null)
                _targetLight = GetComponent<Light>();

            ScheduleNextFlicker();
            PlayAmbiance();
        }

        #endregion

        #region ===== Flicker Flow =====

        private void TryStartFlicker(float time)
        {
            if (_isFlickering || _isWaitingBetweenFlickers)
                return;

            if (time < _nextFlickerTime)
                return;

            _remainingFlickers = Random.value < _doubleFlickerChance ? 2 : 1;
            StartFlicker();
        }

        private void UpdateFlicker(float dt)
        {
            if (_isFlickering)
            {
                UpdateActiveFlicker(dt);
                return;
            }

            if (_isWaitingBetweenFlickers)
            {
                UpdateDelayBetweenFlickers(dt);
            }
        }

        private void UpdateActiveFlicker(float dt)
        {
            _timer -= dt;
            _targetLight.intensity = _baseIntensity * _flickerIntensityFactor;

            if (_timer > 0f) return;

            _remainingFlickers--;

            if (_remainingFlickers > 0)
            {
                EnterDelayBetweenFlickers();
            }
            else
            {
                EndFlicker();
            }
        }

        private void UpdateDelayBetweenFlickers(float dt)
        {
            _timer -= dt;

            if (_timer > 0f) return;

            _isWaitingBetweenFlickers = false;
            StartFlicker();
        }

        private void StartFlicker()
        {
            _isFlickering = true;
            _timer = _flickerDuration;

            PlayFlickerSound();
        }

        private void EndFlicker()
        {
            _isFlickering = false;
            _isWaitingBetweenFlickers = false;

            ScheduleNextFlicker();
        }

        private void EnterDelayBetweenFlickers()
        {
            _isFlickering = false;
            _isWaitingBetweenFlickers = true;
            _timer = _delayBetweenDoubleFlicker;
        }

        private bool IsBusy()
        {
            return _isFlickering || _isWaitingBetweenFlickers;
        }

        #endregion

        #region ===== Noise =====

        private void ApplyNoise(float time)
        {
            float noise = Mathf.PerlinNoise(time * _noiseSpeed, 0f) * 2f - 1f;
            _targetLight.intensity = _baseIntensity + noise * _noiseAmplitude;
        }

        #endregion

        #region ===== Audio =====

        private void PlayAmbiance()
        {
            if (ServiceLocator.Audio == null || _ambianceLight == null)
                return;

            _ambianceHandle?.Stop();
            _ambianceHandle = ServiceLocator.Audio.PlayAt(_ambianceLight, transform.position);
        }

        private void PlayFlickerSound()
        {
            if (ServiceLocator.Audio == null || _flickerSound == null)
                return;

            _flickerHandle?.Stop();
            _flickerHandle = ServiceLocator.Audio.PlayAt(_flickerSound, transform.position);
        }

        #endregion

        #region ===== Scheduling =====

        private void ScheduleNextFlicker()
        {
            _nextFlickerTime = Time.time + Random.Range(_minFlickerInterval, _maxFlickerInterval);
        }

        #endregion
    }
}