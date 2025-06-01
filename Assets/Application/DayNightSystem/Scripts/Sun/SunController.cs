using UnityEngine;

namespace LightHouse.Game.DayNightSystem
{
    public class SunController : MonoBehaviour, ITimeCycleObserver
    {
        public Light sunLight;

        [Tooltip("Angle de rotation du soleil à midi (vertical)")]
        public Vector3 noonDirection = new Vector3(50f, 0f, 0f);

        [Header("Lumière du soleil")]
        public Gradient sunColorGradient;
        public AnimationCurve intensityFactorCurve; // de 0 (nuit) à 1 (midi)

        [Tooltip("Intensité max à midi (en lux)")]
        public float maxIntensityLux = 100000f;

        public void OnTimeChanged(float timeOfDay)
        {
            float normalizedTime = timeOfDay / 24f;
            float sunAngle = normalizedTime * 360f - 90f;
            sunLight.transform.rotation = Quaternion.Euler(sunAngle, noonDirection.y, noonDirection.z);

            sunLight.color = sunColorGradient.Evaluate(normalizedTime);
            float factor = intensityFactorCurve.Evaluate(normalizedTime);
            sunLight.intensity = maxIntensityLux * factor;
        }

        private void Start()
        {
            FindFirstObjectByType<TimeManager>().RegisterObserver(this);
        }
    }
}
