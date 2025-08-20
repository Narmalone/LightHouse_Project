using UnityEngine;
using UnityEngine.Rendering;
using System;
using UnityEngine.Rendering.HighDefinition;

namespace LightHouse.Game.DayNightSystem
{
    public class SunController : MonoBehaviour, ITimeCycleObserver
    {
        #region Inspector

        [Header("References")]
        [SerializeField] private Light _sunLight;
        [SerializeField] private LensFlareComponentSRP _lens;
        [SerializeField] private HDAdditionalLightData _lightData;

        [Header("Sun Rotation")]
        [Tooltip("Angle de rotation du soleil à midi (vertical)")]
        [SerializeField] private Vector3 _noonDirection = new Vector3(50f, 0f, 0f);

        #endregion

        #region Events

        /// <summary>
        /// Appelé quand l'état actif du soleil change (enabled/disabled).
        /// </summary>
        public event Action<bool> OnSunLightToggled;

        #endregion

        #region Private

        private bool _previousSunState;

        #endregion

        #region Properties

        public Light SunLight => _sunLight;
        public LensFlareComponentSRP Lens => _lens;
        public HDAdditionalLightData LightData => _lightData;

        #endregion

        #region Unity Callbacks

        private void Start()
        {
            _previousSunState = _sunLight.enabled;
            OnSunLightToggled?.Invoke(_previousSunState);
        }

        #endregion

        #region Time Cycle

        public void RegisterToTimeManager(TimeManager timeManager)
        {
            timeManager.RegisterObserver(this);
        }

        public void UnregisterFromTimeManager(TimeManager timeManager)
        {
            timeManager.RegisterObserver(this);
        }

        public void OnTimeChanged(float timeOfDay)
        {
            float normalizedTime = timeOfDay / 24f;
            float sunAngle = normalizedTime * 360f - 90f;
            _sunLight.transform.rotation = Quaternion.Euler(sunAngle, _noonDirection.y, _noonDirection.z);

            bool newSunState = timeOfDay > 6f && timeOfDay < 18.15f;

            if (newSunState != _previousSunState)
            {
                OnSunLightToggled?.Invoke(newSunState);
                _previousSunState = newSunState;
            }

            _sunLight.enabled = newSunState;
            _lens.enabled = newSunState;
        }

        #endregion
    }
}
