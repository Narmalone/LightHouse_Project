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

        private void Start()
        {
            UpdateMoonRotation(); //Force first rotation
        }

        #endregion

        #region Time Cycle

        public void OnTimeChanged(float timeOfDay)
        {
            if (_moonLight == null || _lightData == null)
                return;
            ApplyFadeAndLighting(timeOfDay);
        }

        private void ApplyFadeAndLighting(float time)
        {
            float t = 0f;

            if (time >= _fadeInStartHour && time <= _fadeInEndHour)
            {
                // Fade in ex: 18-20h
                t = Mathf.InverseLerp(_fadeInStartHour, _fadeInEndHour, time);
            }
            else if ((time > _fadeInEndHour && time <= 24f) || (time >= 0f && time < _fadeOutStartHour))
            {
                // Full intensity ex: 20h - 4h
                t = 1f;
            }
            else if (time >= _fadeOutStartHour && time <= _fadeOutEndHour)
            {
                // Fade out ex: 4h-6h
                t = 1f - Mathf.InverseLerp(_fadeOutStartHour, _fadeOutEndHour, time);
            }
            else
            {
                //No moon during day but overlaping is possible
                t = 0f;
            }

            _moonLight.intensity = t * _moonMaxIntensity;
            _moonLight.enabled = _moonLight.intensity > 0.01f;

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
