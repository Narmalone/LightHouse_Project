using System.Reflection;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;
using UnityEngine.Rendering.HighDefinition;

namespace LightHouse.Game.DayNightSystem
{
    public class MoonController : MonoBehaviour
    {
        public Light moonLight;
        public Vector3 orbitBaseDirection = new Vector3(-30f, 0f, 0f);
        public float orbitCycleDays = 29.5f;

        private TimeManager timeManager;

        public float maxEarthShineIntensity = 500.0f;

        private HDAdditionalLightData lightData;

        private void Start()
        {
            lightData = moonLight.GetComponent<HDAdditionalLightData>(); // CelestialBody est un MonoBehaviour non exposé
            timeManager = FindFirstObjectByType<TimeManager>();
            if (timeManager != null)
            {
                timeManager.OnTimeSegmentChanged += HandleSegmentChanged;
                UpdateMoonRotation(); // force la première rotation
            }
        }

        private void HandleSegmentChanged(TimeOfDaySegment segment)
        {
            moonLight.enabled = timeManager.currentTime >= 18f || timeManager.currentTime <= 6f;
            if (segment == TimeOfDaySegment.Midday)
            {
                UpdateMoonRotation();
            }
        }

        private void UpdateMoonRotation()
        {
            if (timeManager == null) return;

            // Ratio du cycle lunaire (0 → 1)
            float dayRatio = (timeManager.currentDay % orbitCycleDays) / orbitCycleDays;

            // Angle d'orbite lunaire autour de Y (360° sur 29.5 jours)
            float orbitalAngle = dayRatio * 360f;

            // Direction opposée au soleil = on inverse le vecteur
            Vector3 orbitalDirection = Quaternion.Euler(orbitBaseDirection.x, orbitalAngle, 0f) * Vector3.forward;

            // Appliquer direction au light
            moonLight.transform.rotation = Quaternion.LookRotation(-orbitalDirection, Vector3.up);
        }

        public float moonMaxIntensity = 0.3f;
        public float moonFlareSize = 14.0f;
        public float moonFlareFallOff = 6.0f;
        public float moonMaxFlareMultiplier = 0.3f;
        public float earthShine = 300f;
        public float fadeInStart = 18f;
        public float fadeInEnd = 20f;
        public float fadeOutStart = 4f;
        public float fadeOutEnd = 5.9f;

        private void Update()
        {
            if (timeManager == null || moonLight == null)
                return;

            float t = 0f;

            if (timeManager.currentTime >= fadeInStart && timeManager.currentTime <= fadeInEnd)
            {
                t = Mathf.InverseLerp(fadeInStart, fadeInEnd, timeManager.currentTime);
            }
            else if ((timeManager.currentTime > fadeInEnd && timeManager.currentTime <= 24f) ||
                     (timeManager.currentTime >= 0f && timeManager.currentTime < fadeOutStart))
            {
                t = 1f;
            }
            else if (timeManager.currentTime >= fadeOutStart && timeManager.currentTime <= fadeOutEnd)
            {
                t = 1f - Mathf.InverseLerp(fadeOutStart, fadeOutEnd, timeManager.currentTime); // ✅ Inversion
            }
            else
            {
                t = 0f;
            }

            moonLight.intensity = t * moonMaxIntensity;
            lightData.earthshine = t * maxEarthShineIntensity;
            lightData.flareFalloff = t * moonFlareFallOff;
            lightData.flareSize = t * moonFlareSize;
            lightData.flareMultiplier = t * moonMaxFlareMultiplier;
            moonLight.enabled = moonLight.intensity > 0.01f;
        }


        public float GetLunarPhase()
        {
            return ((float)timeManager.currentDay % orbitCycleDays) / orbitCycleDays;
        }

        private void OnDestroy()
        {
            if (timeManager != null)
                timeManager.OnTimeSegmentChanged -= HandleSegmentChanged;
        }
    }
}
