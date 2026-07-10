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
        [Tooltip("Inclinaison orbitale par jour (x=pitch, y=yaw, z=roll de l'orbite).")]
        [SerializeField] private Vector3 _orbitDirectionEachDay = new Vector3(-30f, 0f, 0f);
        [Tooltip("Durée du cycle lunaire en jours de jeu.")]
        [SerializeField] private float _orbitCycleDays = 29.5f;

        [Header("Initial Orientation")]
        [Tooltip("Décalage de phase orbitale en degrés au jour 0 (0..360).")]
        [SerializeField, Range(0f, 360f)] private float _initialOrbitalPhaseDeg = 0f;
        [Tooltip("Roulement (roll) de la lune pour orienter le terminateur/flare.")]
        [SerializeField, Range(-180f, 180f)] private float _moonRollDeg = 0f;
        [Tooltip("Axe 'up' de référence pour l'orientation (garde Vector3.up sauf si tu as un monde incliné).")]
        [SerializeField] private Vector3 _worldUp = Vector3.up;

        [Header("Light Settings")]
        [SerializeField] private float _moonMaxIntensity = 41000f;
        [SerializeField] private float _maxEarthShineIntensity = 500f;
        [SerializeField] private float _moonFlareSize = 14f;
        [SerializeField] private float _moonFlareFallOff = 6f;
        [SerializeField] private float _moonMaxFlareMultiplier = 0.3f;

        [Header("Fade Timings (hours)")]
        [Tooltip("Ces heures définissent AUSSI l'arc de déplacement de la lune dans le ciel (lever = " +
                 "début du fade in, coucher = fin du fade out) : une seule source de vérité, pas de " +
                 "risque de désync entre \"quand elle est visible\" et \"où elle se trouve\".")]
        [SerializeField] private float _fadeInStartHour = 18f;
        [SerializeField] private float _fadeInEndHour = 20f;
        [SerializeField] private float _fadeOutStartHour = 4f;
        [SerializeField] private float _fadeOutEndHour = 5.9f;

        public Light MoonLight => _moonLight;

        /// <summary>true si c'est actuellement la lune qui porte les ombres (jamais vrai en même temps que le soleil).</summary>
        public bool OwnsShadows => _appliedShadowActive ?? false;

        // Même pattern anti-flicker que SunController : on ne réécrit .shadows que si la valeur cible
        // diffère réellement de ce qui est déjà appliqué, jamais en aveugle à chaque frame.
        private bool? _appliedShadowActive;

        private void Awake()
        {
            TimeHandlerData.OnTimeChanged += OnTimeChanged;
        }

        private void OnDestroy()
        {
            TimeHandlerData.OnTimeChanged -= OnTimeChanged;
        }

        private void Start()
        {
            _initialOrbitalPhaseDeg = UnityEngine.Random.Range(0f, 360f);
        }

        public void OnTimeChanged(float timeOfDay)
        {
            if (_moonLight == null || _lightData == null)
                return;

            timeOfDay = Normalize24(timeOfDay);

            ApplyFadeAndLighting(timeOfDay);
            UpdateMoonRotation(timeOfDay);
        }

        #region Ombres (piloté depuis l'extérieur par SunController.OnShadowOwnershipChanged)

        /// <summary>
        /// Active/désactive les ombres de la lune. À appeler depuis l'extérieur (LightingProfileManager,
        /// abonné à SunController.OnShadowOwnershipChanged) avec l'inverse de ce que porte le soleil :
        /// jamais les deux actifs en même temps par construction, puisque c'est le MÊME booléen inversé
        /// qui pilote les deux côtés.
        /// </summary>
        public void SetShadowActive(bool active)
        {
            if (_appliedShadowActive.HasValue && _appliedShadowActive.Value == active) return;
            _appliedShadowActive = active;
            _moonLight.shadows = active ? LightShadows.Soft : LightShadows.None;
        }

        #endregion

        #region Fade (intensité, earthshine, flare)

        private void ApplyFadeAndLighting(float time)
        {
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

        #endregion

        #region Rotation (phase lente jour-par-jour + arc de la nuit courante)

        /// <summary>
        /// AVANT : ne se recalculait qu'une fois par jour (au passage du segment à Midday), donc la
        /// lune restait figée sur une position fixe toute la nuit au lieu de traverser le ciel.
        /// MAINTENANT : recalculée à chaque frame (comme le soleil), en combinant :
        ///  - la phase orbitale lente (_initialOrbitalPhaseDeg + dérive sur _orbitCycleDays jours),
        ///    qui simule le décalage du cycle lunaire de ~29.5 jours ;
        ///  - un arc lever→coucher CONTINU pendant la nuit courante (même principe que le soleil le
        ///    jour), basé sur les mêmes heures que le fade (_fadeInStartHour → _fadeOutEndHour).
        /// </summary>
        private void UpdateMoonRotation(float timeOfDay)
        {
            float dayRatio = (TimeHandlerData.CurrentDay % _orbitCycleDays) / _orbitCycleDays;
            float basePhaseAngle = _initialOrbitalPhaseDeg + dayRatio * 360f;

            float arcAngle = 0f;
            if (InArcWindow(timeOfDay, out float arcT))
                arcAngle = Mathf.Lerp(0f, 180f, arcT);

            float orbitalAngle = basePhaseAngle + arcAngle;

            Quaternion orbitTilt = Quaternion.Euler(_orbitDirectionEachDay);
            Quaternion orbitAround = Quaternion.AngleAxis(orbitalAngle, _worldUp);

            Vector3 orbitalDirection = orbitAround * (orbitTilt * Vector3.forward);
            Quaternion look = Quaternion.LookRotation(-orbitalDirection, _worldUp);

            _moonLight.transform.rotation = look * Quaternion.Euler(0f, 0f, _moonRollDeg);
        }

        /// <summary>
        /// t (0..1) de progression dans la fenêtre lever→coucher (_fadeInStartHour → _fadeOutEndHour,
        /// avec wrap minuit). false si on est en dehors (journée, lune pas visible de toute façon).
        /// </summary>
        private bool InArcWindow(float time, out float t)
        {
            float start = Normalize24(_fadeInStartHour);
            float end = Normalize24(_fadeOutEndHour);
            float len = ArcLength(start, end);

            if (len <= 1e-4f || !InRangeWrap(time, start, end))
            {
                t = 0f;
                return false;
            }

            float pos = ArcLength(start, time);
            t = Mathf.Clamp01(pos / len);
            return true;
        }

        public float GetLunarPhase()
        {
            return (TimeHandlerData.CurrentDay % _orbitCycleDays) / _orbitCycleDays;
        }

        #endregion

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (_moonLight != null)
                UpdateMoonRotation(TimeHandlerData.CurrentTime);
        }
#endif

        // --------- Helpers (mêmes que SunController, dupliqués localement pour rester symétrique
        // et lisible côte à côte plutôt que de dépendre d'un utilitaire partagé) ---------
        private static float Normalize24(float h) { h %= 24f; if (h < 0f) h += 24f; return h; }

        private static bool InRangeWrap(float t, float start, float end)
        {
            start = Normalize24(start); end = Normalize24(end); t = Normalize24(t);
            if (start <= end) return t >= start && t < end;
            return t >= start || t < end;
        }

        private static float ArcLength(float start, float end)
        {
            start = Normalize24(start); end = Normalize24(end);
            float d = end - start; if (d < 0f) d += 24f; return d;
        }
    }
}