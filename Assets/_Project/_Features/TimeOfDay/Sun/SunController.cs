using LightHouse.Features.TimeOfDay.TimeCore;
using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

namespace LightHouse.Features.TimeOfDay.Sun
{
    public class SunController : MonoBehaviour, ITimeCycleObserver
    {
        [Header("References")]
        [SerializeField] private Light _sunLight;
        [SerializeField] private LensFlareComponentSRP _lens;
        [SerializeField] private HDAdditionalLightData _lightData;

        [Header("Sun Times (24h)")]
        [Tooltip("Heure du lever (0..24)")]
        public float SunMorningShow = 6f;
        [Tooltip("Heure du coucher (0..24)")]
        public float SunNightDisapear = 21f;

        [Header("Orientation latérale (Y/Z)")]
        [SerializeField] private float _yaw = 0f;   // orientation horizontale
        [SerializeField] private float _roll = 0f;  // éventuel tilt

        [Header("Shadows")]
        [Tooltip("Nombre d'heures avant la fin du segment jour/nuit courant pendant lesquelles on coupe " +
                 "les ombres du soleil (prépare la transition vers la lune, évite des ombres bizarres " +
                 "pendant le fade). C'était en dur (2f) avant, exposé ici pour pouvoir l'ajuster.")]
        [SerializeField] private float _shadowCutoffBeforeSegmentEnd = 2f;

        /// <summary>Le soleil vient d'être allumé/éteint (isDay a changé). Narratif/gameplay général.</summary>
        public event Action<bool> OnSunLightToggled;

        /// <summary>
        /// Le soleil vient de prendre ou perdre la responsabilité des ombres (true = soleil, false =
        /// à quelqu'un d'autre, typiquement la lune, de gérer ses propres ombres). C'est LE signal
        /// à utiliser pour coordonner soleil/lune : contrairement à OnSunLightToggled (qui suit juste
        /// isDay), celui-ci suit le vrai instant où le soleil arrête/reprend de vouloir des ombres
        /// (cf _shadowCutoffBeforeSegmentEnd), donc il est aligné sur le moment réel du handoff.
        /// </summary>
        public event Action<bool> OnShadowOwnershipChanged;

        private bool _previousSunState;

        public Light SunLight => _sunLight;
        public LensFlareComponentSRP SunLens => _lens;
        public HDAdditionalLightData LightData => _lightData;

        /// <summary>true si c'est actuellement le soleil qui porte les ombres (jamais vrai en même temps que la lune).</summary>
        public bool OwnsShadows => _appliedShadowOwnership ?? false;

        // --- État réellement appliqué au moteur, pour ne JAMAIS réécrire une propriété avec la même
        // valeur qu'elle a déjà. C'est le cœur du fix : avant, .enabled / .shadows / lens.enabled
        // étaient réassignés à CHAQUE frame (OnTimeChanged part de TimeManager.Update(), donc chaque
        // frame), même quand rien ne changeait. Selon la version d'HDRP, réécrire .shadows en boucle
        // peut forcer un retraitement interne de la shadow map à chaque frame -> flicker visuel.
        // Maintenant, on ne touche au moteur QUE quand la valeur cible diffère de ce qui est déjà là.
        // Même logique reprise dans MoonController pour ses propres ombres.
        private bool _hasAppliedEnabled;
        private bool _appliedEnabled;
        private bool? _appliedShadowOwnership;
        private bool? _appliedLensEnabled;

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
            _previousSunState = _sunLight.enabled;
            OnSunLightToggled?.Invoke(_previousSunState);
        }

        public void OnTimeChanged(float timeOfDay)
        {
            timeOfDay = Normalize24(timeOfDay);

            // 1) Découpe jour/nuit (supporte aussi le cas lever > coucher si besoin)
            bool isDay = InRangeWrap(timeOfDay, SunMorningShow, SunNightDisapear);

            // 2) t (0..1) dans le segment courant
            float segStart = isDay ? SunMorningShow : SunNightDisapear;
            float segEnd = isDay ? SunNightDisapear : SunMorningShow; // wrap la nuit
            float segLen = ArcLength(segStart, segEnd);               // durée en heures
            float segPos = ArcLength(segStart, timeOfDay);
            float t = segLen > 1e-4f ? Mathf.Clamp01(segPos / segLen) : 0f;

            // 3) Angle X
            //    - Jour  : 0  → 180
            //    - Nuit  : 180→ 360 (continue pour revenir à 0 au lever)
            float angleX = isDay
                ? Mathf.Lerp(0f, 180f, t)
                : Mathf.Lerp(180f, 360f, t);

            // 4) Rotation : ÇA, ça doit tourner chaque frame, c'est ce qui fait bouger le soleil.
            //    Rien à voir avec le bug de flicker (le Transform n'a pas ce genre de coût caché).
            _sunLight.transform.rotation = Quaternion.Euler(angleX, _yaw, _roll);

            // 5) Tout le reste : calculé chaque frame, mais ÉCRIT sur le moteur seulement si ça change.
            ApplySunEnabled(isDay);

            // Le soleil "veut" des ombres tant qu'il est levé ET pas dans sa fenêtre de cutoff avant
            // la fin du segment (prépare le handoff vers la lune en douceur, cf tooltip du champ).
            bool wantsShadows = isDay && InRangeWrap(timeOfDay, segStart, segEnd - _shadowCutoffBeforeSegmentEnd);
            ApplyShadowOwnership(wantsShadows);

            ApplyLensEnabled(isDay);
        }

        #region Application moteur (écriture uniquement si la valeur change réellement)

        private void ApplySunEnabled(bool isDay)
        {
            // L'event reste narratif : il prévient les autres systèmes (ex: la lune qui doit prendre
            // le relais des ombres) UNIQUEMENT au moment du vrai changement, jamais en continu.
            if (isDay != _previousSunState)
            {
                OnSunLightToggled?.Invoke(isDay);
                _previousSunState = isDay;
            }

            if (_hasAppliedEnabled && _appliedEnabled == isDay) return;
            _hasAppliedEnabled = true;
            _appliedEnabled = isDay;
            _sunLight.enabled = isDay;
        }

        /// <summary>
        /// Bascule la propriété des ombres. C'est la SEULE méthode qui écrit _sunLight.shadows et qui
        /// notifie OnShadowOwnershipChanged — les deux passent par le même filtre "écrit/notifie
        /// seulement si ça change réellement", donc jamais de double-notification ni de réécriture
        /// inutile même si OnTimeChanged rappelle ça avec la même valeur 60 fois par seconde.
        /// </summary>
        private void ApplyShadowOwnership(bool ownsShadows)
        {
            if (_appliedShadowOwnership.HasValue && _appliedShadowOwnership.Value == ownsShadows) return;
            _appliedShadowOwnership = ownsShadows;
            _sunLight.shadows = ownsShadows ? LightShadows.Soft : LightShadows.None;
            OnShadowOwnershipChanged?.Invoke(ownsShadows);
        }

        private void ApplyLensEnabled(bool enabled)
        {
            if (_appliedLensEnabled.HasValue && _appliedLensEnabled.Value == enabled) return;
            _appliedLensEnabled = enabled;
            _lens.enabled = enabled;
        }

        #endregion

        // --------- Helpers ---------
        private static float Normalize24(float h) { h %= 24f; if (h < 0f) h += 24f; return h; }
        private static bool InRangeWrap(float t, float start, float end)
        {
            // [start, end) modulo 24
            start = Normalize24(start); end = Normalize24(end); t = Normalize24(t);
            if (start <= end) return t >= start && t < end;
            return t >= start || t < end; // cas wrap (ex: 22→6)
        }
        private static float ArcLength(float start, float end)
        {
            // longueur (heures) de start vers end en avançant modulo 24
            start = Normalize24(start); end = Normalize24(end);
            float d = end - start; if (d < 0f) d += 24f; return d;
        }
    }
}