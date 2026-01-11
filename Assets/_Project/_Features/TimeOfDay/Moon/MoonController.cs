using LightHouse.Features.TimeOfDay.TimeCore;
using System;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

namespace LightHouse.Features.TimeOfDay.Moon
{
    public class MoonController : MonoBehaviour, ITimeCycleObserver
    {
        [Header("References")]
        [SerializeField] private Light _moonLight;
        [SerializeField] private HDAdditionalLightData _lightData;

        [Header("Orbital Parameters")]
        [Tooltip("Inclinaison orbitale par jour (x=pitch, y=yaw, z=roll de l’orbite).")]
        [SerializeField] private Vector3 _orbitDirectionEachDay = new Vector3(-30f, 0f, 0f);
        [Tooltip("Durée du cycle lunaire en jours de jeu.")]
        [SerializeField] private float _orbitCycleDays = 29.5f;

        [Header("Initial Orientation")]
        [Tooltip("Décalage de phase orbitale en degrés au jour 0 (0..360).")]
        [SerializeField, Range(0f, 360f)] private float _initialOrbitalPhaseDeg = 0f;
        [Tooltip("Roulement (roll) de la lune pour orienter le terminateur/flare.")]
        [SerializeField, Range(-180f, 180f)] private float _moonRollDeg = 0f;
        [Tooltip("Axe 'up' de référence pour l’orientation (garde Vector3.up sauf si tu as un monde incliné).")]
        [SerializeField] private Vector3 _worldUp = Vector3.up;

        [Header("Light Settings")]
        [SerializeField] private float _moonMaxIntensity = 41000f;
        [SerializeField] private float _maxEarthShineIntensity = 500f;
        [SerializeField] private float _moonFlareSize = 14f;
        [SerializeField] private float _moonFlareFallOff = 6f;
        [SerializeField] private float _moonMaxFlareMultiplier = 0.3f;

        [Header("Fade Timings (hours)")]
        [SerializeField] private float _fadeInStartHour = 18f;
        [SerializeField] private float _fadeInEndHour = 20f;
        [SerializeField] private float _fadeOutStartHour = 4f;
        [SerializeField] private float _fadeOutEndHour = 5.9f;

        public Light MoonLight => _moonLight;

        private void Awake()
        {
            TimeHandlerData.OnTimeSegmentChanged += TimeHandlerData_OnTimeSegmentChanged;
        }

        private void Start()
        {
            // Applique l’orientation initiale cohérente avec l’offset de phase.
            _initialOrbitalPhaseDeg = UnityEngine.Random.Range(0, 360f);
            UpdateMoonRotation();
            // ❌ Ne PAS remettre une rotation arbitraire ici, sinon elle sera perdue.
            // _moonLight.transform.rotation = ...
        }

        private void OnDestroy()
        {
            TimeHandlerData.OnTimeSegmentChanged -= TimeHandlerData_OnTimeSegmentChanged;
        }

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
            h %= 24f;
            if (h < 0f) h += 24f;
            return h;
        }

        private static bool InRangeWrap(float t, float start, float end)
        {
            if (start <= end) return t >= start && t < end;
            return t >= start || t < end;
        }

        private void ApplyFadeAndLighting(float time)
        {
            time = Normalize24(time);

            bool inFadeIn = InRangeWrap(time, _fadeInStartHour, _fadeInEndHour);
            bool inFull = InRangeWrap(time, _fadeInEndHour, _fadeOutStartHour);
            bool inFadeOut = InRangeWrap(time, _fadeOutStartHour, _fadeOutEndHour);

            float t = 0f;
            if (inFadeIn) t = Mathf.InverseLerp(_fadeInStartHour, _fadeInEndHour, time);
            else if (inFull) t = 1f;
            else if (inFadeOut) t = 1f - Mathf.InverseLerp(_fadeOutStartHour, _fadeOutEndHour, time);

            _moonLight.intensity = t * _moonMaxIntensity;
            _moonLight.enabled = (inFadeIn || inFull || inFadeOut);

            _lightData.earthshine = t * _maxEarthShineIntensity;
            _lightData.flareFalloff = t * _moonFlareFallOff;
            _lightData.flareSize = t * _moonFlareSize;
            _lightData.flareMultiplier = t * _moonMaxFlareMultiplier;
        }

        private void UpdateMoonRotation()
        {
            // Phase 0..1 sur le cycle
            float dayRatio = (TimeHandlerData.CurrentDay % _orbitCycleDays) / _orbitCycleDays;

            // Angle orbital = offset initial + progression
            float orbitalAngle = _initialOrbitalPhaseDeg + dayRatio * 360f;

            // Direction orbitale : on applique d’abord l’inclinaison d’orbite, puis on tourne autour de l’axe 'up'
            Quaternion orbitTilt = Quaternion.Euler(_orbitDirectionEachDay);
            Quaternion orbitAround = Quaternion.AngleAxis(orbitalAngle, _worldUp);

            Vector3 orbitalDirection = orbitAround * (orbitTilt * Vector3.forward);

            // Orientation de la lumière : regarde vers le centre (négatif de la direction), avec un 'up' défini
            Quaternion look = Quaternion.LookRotation(-orbitalDirection, _worldUp);

            // Applique un roll supplémentaire si tu veux orienter le terminateur/flare
            _moonLight.transform.rotation = look * Quaternion.Euler(0f, 0f, _moonRollDeg);
        }

        public float GetLunarPhase()
        {
            return (TimeHandlerData.CurrentDay % _orbitCycleDays) / _orbitCycleDays;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (_moonLight != null)
                UpdateMoonRotation();
        }
#endif
    }
}
