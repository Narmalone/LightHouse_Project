using UnityEngine;
using UnityEngine.Rendering;

namespace LightHouse.Game.DayNightSystem
{
    public class SunController : MonoBehaviour, ITimeCycleObserver
    {
        public Light sunLight;
        public LensFlareComponentSRP Lens;
        [Tooltip("Angle de rotation du soleil à midi (vertical)")]
        public Vector3 noonDirection = new Vector3(50f, 0f, 0f);

        public void OnTimeChanged(float timeOfDay)
        {
            float normalizedTime = timeOfDay / 24f;
            float sunAngle = normalizedTime * 360f - 90f;
            sunLight.transform.rotation = Quaternion.Euler(sunAngle, noonDirection.y, noonDirection.z);

            sunLight.enabled = timeOfDay > 6 && timeOfDay < 18;
            Lens.enabled = sunLight.enabled;
        }

        private void Start()
        {
            FindFirstObjectByType<TimeManager>().RegisterObserver(this);
        }
    }
}
