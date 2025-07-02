using UnityEngine;
using UnityEngine.Rendering;
using System;

namespace LightHouse.Game.DayNightSystem
{
    public class SunController : MonoBehaviour, ITimeCycleObserver
    {
        public Light sunLight;
        public LensFlareComponentSRP Lens;
        [Tooltip("Angle de rotation du soleil à midi (vertical)")]
        public Vector3 noonDirection = new Vector3(50f, 0f, 0f);

        public event Action<bool> OnSunLightToggled; // Événement lancé quand sunLight.enabled change

        private bool previousSunState;

        public void OnTimeChanged(float timeOfDay)
        {
            float normalizedTime = timeOfDay / 24f;
            float sunAngle = normalizedTime * 360f - 90f;
            sunLight.transform.rotation = Quaternion.Euler(sunAngle, noonDirection.y, noonDirection.z);

            bool newSunState = timeOfDay > 6 && timeOfDay < 18;

            if (newSunState != previousSunState)
            {
                OnSunLightToggled?.Invoke(newSunState); //Lancer l'événement
                previousSunState = newSunState;
            }

            sunLight.enabled = newSunState;
            Lens.enabled = newSunState;
        }

        private void Start()
        {
            FindFirstObjectByType<TimeManager>().RegisterObserver(this);
            previousSunState = sunLight.enabled;
            OnSunLightToggled?.Invoke(previousSunState);
            Debug.Log(previousSunState);
        }
    }
}
