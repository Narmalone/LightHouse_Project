using LightHouse.Core.Player;
using LightHouse.Core.Services;
using LightHouse.Features.Weather;
using UnityEngine;

namespace LightHouse.Core.Audio
{
    public class WindowController : MonoBehaviour
    {
        [Header("Audio")]
        public SO_AudioCue creakingWoodCue;       // loop discret continu
        public SO_AudioCue creakScreamWoodCue;    // one-shot gros craquement

        [Header("Window State")]
        [Range(0f, 1f)] public float openness = 0.5f; // 0 = fermée, 1 = ouverte
        [Range(0f, 130f)] public float defaultWindSpeedIfNoWeatherData = 50f; // 0 = fermée, 1 = ouverte

        [Header("Vent -> Volume (loop)")]
        public float windAtMin = 0f;
        public float windAtMax = 130f;
        public AnimationCurve windToVolumeCurve = AnimationCurve.Linear(0, 0, 1, 1);
        public float volumeSmoothing = 10f;

        [Header("Pitch (léger, optionnel)")]
        public float pitchMin = 0.98f;
        public float pitchMax = 1.06f;

        [Header("Gros craquement (one-shot)")]
        public bool enableScreamCreak = true;
        public float windAtExtreme = 200f;
        public float screamEveryMin = 4f;
        public float screamEveryMax = 10f;
        [Range(0f, 2f)] public float screamVolume = 1.0f;
        [Range(0f, 0.3f)] public float screamPitchJitterSemitones = 0.08f;

        private IAudioHandle _loopHandle;
        private float _volRuntime = 0f;
        private float _screamTimer = -1f;

        // ──────────────────────────────────────────────────────────────────────

        void Start()
        {
            if (ServiceLocator.Audio != null && creakingWoodCue != null)
            {
                _loopHandle = ServiceLocator.Audio.PlayAt(creakingWoodCue, transform.position);
                _loopHandle?.SetVolume(0f);
            }

            float wind = CurrentWind();
            ApplyWindToLoop(wind, true);

            _screamTimer = -1f;
        }

        void Update()
        {
            float wind = CurrentWind();

            ApplyWindToLoop(wind, false);
            UpdateScream(wind);

            RegisterToEnvironment(); // 🔥 lien avec le système global
        }

        // ──────────────────────────────────────────────────────────────────────

        private float CurrentWind()
        {
            return (WeatherHandlerData.CurrentWeather != null)
                ? WeatherHandlerData.CurrentWeather.WindSpeed
                : defaultWindSpeedIfNoWeatherData;
        }

        // ── Loop: volume + pitch ──────────────────────────────────────────────
        private void ApplyWindToLoop(float wind, bool immediate)
        {
            if (_loopHandle == null) return;

            float t = Mathf.Clamp01(Mathf.InverseLerp(windAtMin, windAtMax, wind));
            float targetVol = Mathf.Clamp01(windToVolumeCurve.Evaluate(t));

            if (immediate || volumeSmoothing <= 0f)
                _volRuntime = targetVol;
            else
                _volRuntime = Mathf.Lerp(_volRuntime, targetVol, 1f - Mathf.Exp(-volumeSmoothing * Time.deltaTime));

            _loopHandle.SetVolume(_volRuntime);

            float pitch = Mathf.Lerp(pitchMin, pitchMax, t);
            _loopHandle.SetPitch(pitch);
        }

        // ── Scream ────────────────────────────────────────────────────────────
        private void UpdateScream(float wind)
        {
            if (!enableScreamCreak || creakScreamWoodCue == null || ServiceLocator.Audio == null)
                return;

            if (wind >= windAtExtreme)
            {
                if (_screamTimer < 0f)
                    _screamTimer = Random.Range(screamEveryMin, screamEveryMax);

                _screamTimer -= Time.deltaTime;

                if (_screamTimer <= 0f)
                {
                    FireScream();
                    _screamTimer = Random.Range(screamEveryMin, screamEveryMax);
                }
            }
            else
            {
                _screamTimer = -1f;
            }
        }

        private void FireScream()
        {
            var h = ServiceLocator.Audio.PlayAt(creakScreamWoodCue, transform.position);
            if (h == null) return;

            h.SetVolume(screamVolume);

            if (screamPitchJitterSemitones > 0f)
            {
                float semi = Random.Range(-screamPitchJitterSemitones, screamPitchJitterSemitones);
                float factor = Mathf.Pow(2f, semi / 12f);
                h.SetPitch(factor);
            }
        }

        // ── GLOBAL LINK (IMPORTANT) ───────────────────────────────────────────
        private void RegisterToEnvironment()
        {
            if (AudioEnvironmentController.Instance == null) return;

            AudioEnvironmentController.Instance.RegisterExposure(openness);
        }
    }
}