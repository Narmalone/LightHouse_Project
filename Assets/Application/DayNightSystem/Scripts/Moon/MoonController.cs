using UnityEngine;

namespace LightHouse.Game.DayNightSystem
{
    public class MoonController : MonoBehaviour, ITimeCycleObserver
    {
        public Light moonLight;

        [Tooltip("Décalage en heures par rapport au soleil")]
        [HideInInspector] public float hourOffset = 12f;

        [Tooltip("Angle de base à minuit")]
        public Vector3 midnightDirection = new Vector3(-30f, 0f, 0f);

        private TimeManager timeManager;

        private void Start()
        {
            timeManager = FindFirstObjectByType<TimeManager>();
            if (timeManager != null)
            {
                // Calcule automatique de l'offset pour que la lune soit à l'opposé du soleil
                float sunTime = timeManager.currentTime;
                hourOffset = 12f; // Toujours 12h d'opposition pour une trajectoire naturelle
                timeManager.RegisterObserver(this);

                // Force un premier update
                OnTimeChanged(timeManager.currentTime);
            }
        }

        public void OnTimeChanged(float timeOfDay)
        {
            float shiftedTime = (timeOfDay + hourOffset + 24f) % 24f;
            float normalizedTime = shiftedTime / 24f;

            // Rotation de la lune
            float moonAngle = normalizedTime * 360f - 90f;
            moonLight.transform.rotation = Quaternion.Euler(moonAngle, midnightDirection.y, midnightDirection.z);

            // Activation : visible la nuit uniquement
            moonLight.enabled = timeOfDay > 18f || timeOfDay < 6f;
        }
    }
}
