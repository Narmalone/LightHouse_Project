using System;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

namespace LightHouse.Game.DayNightSystem
{
    public class MoonController : MonoBehaviour, ITimeCycleObserver
    {
        #region References

        [Header("References")]
        [SerializeField] private Light _moonLight;
        [SerializeField] private HDAdditionalLightData _lightData;

        #endregion

        #region Orbital Settings

        [Header("Orbital Parameters")]
        [SerializeField] private Vector3 _orbitDirectionEachDay = new Vector3(-30f, 0f, 0f);
        [SerializeField] private float _orbitCycleDays = 29.5f;

        #endregion

        #region Lighting Parameters

        [Header("Light Settings")]
        [SerializeField] private float _moonMaxIntensity = 41000f;
        [SerializeField] private float _maxEarthShineIntensity = 500f;
        [SerializeField] private float _moonFlareSize = 14f;
        [SerializeField] private float _moonFlareFallOff = 6f;
        [SerializeField] private float _moonMaxFlareMultiplier = 0.3f;

        #endregion

        #region Fade Timings

        [Header("Fade Timings (hours)")]
        [SerializeField] private float _fadeInStartHour = 18f;
        [SerializeField] private float _fadeInEndHour = 20f;
        [SerializeField] private float _fadeOutStartHour = 4f;
        [SerializeField] private float _fadeOutEndHour = 5.9f;

        #endregion

        #region Internal State

        #endregion

        #region Properties

        public Light MoonLight => _moonLight;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            TimeHandlerData.OnTimeSegmentChanged += TimeHandlerData_OnTimeSegmentChanged;
        }

        private void Start()
        {
            UpdateMoonRotation(); //Force first rotation
            _moonLight.transform.rotation = Quaternion.Euler(_moonLight.transform.rotation.eulerAngles.x, UnityEngine.Random.Range(90f, 270f), _moonLight.transform.rotation.eulerAngles.z);//Force a random yaw
        }

        private void OnDestroy()
        {
            TimeHandlerData.OnTimeSegmentChanged -= TimeHandlerData_OnTimeSegmentChanged;
        }

        #endregion

        #region Time Cycle

        private void TimeHandlerData_OnTimeSegmentChanged(TimeOfDaySegment segment)
        {
            if (segment != TimeOfDaySegment.Midday)
                return;
            GetLunarPhase();
            UpdateMoonRotation();
        }

        public void OnTimeChanged(float timeOfDay)
        {
            if (_moonLight == null || _lightData == null)
                return;
            ApplyFadeAndLighting(timeOfDay);
        }

        private static float Normalize24(float h)
        {
            // renvoie h dans [0,24)
            h %= 24f;
            if (h < 0f) h += 24f;
            return h;
        }

        private static bool InRangeWrap(float t, float start, float end)
        {
            // vrai si t ∈ [start, end) sur un cadran 24h, avec wrap si start > end
            if (start <= end) return t >= start && t < end;
            return t >= start || t < end; // ex: [22, 4)
        }

        private void ApplyFadeAndLighting(float time)
        {
            time = Normalize24(time);

            // Fenêtres (autorise wrap si jamais tu mets des heures qui traversent minuit)
            bool inFadeIn = InRangeWrap(time, _fadeInStartHour, _fadeInEndHour);
            bool inFull = InRangeWrap(time, _fadeInEndHour, _fadeOutStartHour);
            bool inFadeOut = InRangeWrap(time, _fadeOutStartHour, _fadeOutEndHour);

            float t = 0f; // 0..1
            if (inFadeIn)
            {
                float u = Mathf.InverseLerp(_fadeInStartHour, _fadeInEndHour, time);
                t = u;
            }
            else if (inFull)
            {
                t = 1f;
            }
            else if (inFadeOut)
            {
                float u = Mathf.InverseLerp(_fadeOutStartHour, _fadeOutEndHour, time);
                t = 1f - u;
            }
            else
            {
                t = 0f; // plein jour
            }

            // Évite le flicker “enable/disable” autour de 0 : laisse la light ON et joue sur l’intensité
            _moonLight.intensity = t * _moonMaxIntensity;

            // Option 1 (recommandé) : ne JAMAIS disable, juste mettre à ~0 (évite l’extinction brutale à minuit)
            //_moonLight.enabled = true;

            // Si tu tiens à éteindre en plein jour, fais-le sans seuils flottants :
            _moonLight.enabled = (inFadeIn || inFull || inFadeOut);

            _lightData.earthshine = t * _maxEarthShineIntensity;
            _lightData.flareFalloff = t * _moonFlareFallOff;
            _lightData.flareSize = t * _moonFlareSize;
            _lightData.flareMultiplier = t * _moonMaxFlareMultiplier;
        }

        #endregion

        #region Orbital Logic

        private void UpdateMoonRotation()
        {
            float dayRatio = (TimeHandlerData.CurrentDay % _orbitCycleDays) / _orbitCycleDays;
            float orbitalAngle = dayRatio * 360f;

            Vector3 orbitalDirection = Quaternion.Euler(_orbitDirectionEachDay.x, orbitalAngle, 0f) * Vector3.forward;
            _moonLight.transform.rotation = Quaternion.LookRotation(-orbitalDirection, Vector3.up);
        }

        #endregion

        #region Public API

        public float GetLunarPhase()
        {
            return (TimeHandlerData.CurrentDay % _orbitCycleDays) / _orbitCycleDays;
        }

        #endregion
    }
}
