using UnityEngine;

namespace LightHouse.Game.DayNightSystem
{
    public class SunController : MonoBehaviour, ITimeCycleObserver
    {
        public Light sunLight;

        [Tooltip("Angle de rotation du soleil à midi (vertical)")]
        public Vector3 noonDirection = new Vector3(50f, 0f, 0f);

        public void OnTimeChanged(float timeOfDay)
        {
            float normalizedTime = timeOfDay / 24f;
            float sunAngle = normalizedTime * 360f - 90f;
            sunLight.transform.rotation = Quaternion.Euler(sunAngle, noonDirection.y, noonDirection.z);
        }

        private void Start()
        {
            FindFirstObjectByType<TimeManager>().RegisterObserver(this);
        }
    }
}
