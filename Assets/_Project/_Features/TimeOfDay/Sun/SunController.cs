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

        public event Action<bool> OnSunLightToggled;
        private bool _previousSunState;

        public Light SunLight => _sunLight;
        public LensFlareComponentSRP SunLens => _lens;
        public HDAdditionalLightData LightData => _lightData;

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

            // 4) Applique la rotation (X contrôlé par l’heure, Y/Z réglables)
            _sunLight.transform.rotation = Quaternion.Euler(angleX, _yaw, _roll);

            // 5) Etat ON/OFF strict aligné sur jour/nuit
            bool newSunState = isDay;
            if (newSunState != _previousSunState)
            {
                OnSunLightToggled?.Invoke(newSunState);
                _previousSunState = newSunState;
            }
            _sunLight.enabled = isDay;

            bool test = InRangeWrap(timeOfDay, segStart, segEnd - 2f);
            _sunLight.shadows = test ? LightShadows.Soft : LightShadows.None;
            _lens.enabled = isDay;
        }

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
