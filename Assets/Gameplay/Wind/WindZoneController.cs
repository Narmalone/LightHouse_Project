using LightHouse.Features.Weather.Utils;
using System.Collections;
using UnityEngine;

namespace LightHouse.Features.Weather.Wind
{
    [RequireComponent(typeof(WindZone))]
    public class WindZoneController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private WindZone _windZone;

        [Header("Input")]
        [Tooltip("La donnée météo est-elle en m/s ? (sinon km/h)")]
        public bool inputIsMetersPerSecond = true;

        [Header("Mapping (km/h)")]
        [Tooltip("En-dessous = calme plat (t=0)")]
        public float windAtMinKmh = 0f;
        [Tooltip("Au-dessus = vent fort (t=1)")]
        public float windAtMaxKmh = 100f;

        [Header("Curves (t = 0..1)")]
        public AnimationCurve mainCurve = AnimationCurve.Linear(0, 0.10f, 1, 1.00f);
        public AnimationCurve turbulenceCurve = AnimationCurve.Linear(0, 0.10f, 1, 0.60f);
        public AnimationCurve pulseMagCurve = AnimationCurve.Linear(0, 0.00f, 1, 0.70f);
        public AnimationCurve pulseFreqCurve = AnimationCurve.Linear(0, 0.05f, 1, 0.35f);

        [Header("Heading")]
        public bool smoothHeading = true;
        public float turnSpeedDegPerSec = 180f; // 0 => snap

        // runtime (lissés)
        private float _rtMain, _rtTurb, _rtPulseMag, _rtPulseFreq;

        private bool _isInitializedTest = false;
        private void Start()
        {
            StartCoroutine(TestRoutine());
        }

        private IEnumerator TestRoutine()
        {
            yield return new WaitForSeconds(5f);
            _isInitializedTest = true;
        }

        void Update()
        {
            if (!_isInitializedTest) return;
            var w = WeatherHandlerData.CurrentWeather;
            if (w == null) return;
            if (_windZone == null) return;

            SetRotationByWindDirection(w.WindOrientation);
            SetWindForcesByWeather(w.WindSpeed);
        }

        // -------- Rotation : ne changer que Y, vers la direction du vent --------
        private void SetRotationByWindDirection(float windOrientationDegreeFrom)
        {
            float towardsDeg = WeatherUtils.FromToTowards(windOrientationDegreeFrom);
            Vector3 e = transform.eulerAngles;

            if (smoothHeading && turnSpeedDegPerSec > 0f)
            {
                float y = Mathf.MoveTowardsAngle(e.y, towardsDeg, turnSpeedDegPerSec * Time.deltaTime);
                transform.eulerAngles = new Vector3(e.x, y, e.z);
            }
            else
            {
                transform.eulerAngles = new Vector3(e.x, towardsDeg, e.z);
            }
        }

        // -------------------------- Forces / Pulses ---------------------------
        private void SetWindForcesByWeather(float windSpeedIn)
        {
            // Convertit vers km/h si l’input est en m/s
            float kmh = inputIsMetersPerSecond ? windSpeedIn * 3.6f : windSpeedIn;

            // t ∈ [0..1]
            float t = Mathf.Clamp01(Mathf.InverseLerp(windAtMinKmh, windAtMaxKmh, kmh));

            // Valeurs “cibles” issues des courbes
            float targetMain = Mathf.Max(0f, mainCurve.Evaluate(t));
            float targetTurb = Mathf.Max(0f, turbulenceCurve.Evaluate(t));
            float targetPulseMag = Mathf.Max(0f, pulseMagCurve.Evaluate(t));
            float targetPulseF = Mathf.Max(0f, pulseFreqCurve.Evaluate(t));

            _rtMain = targetMain;
            _rtTurb = targetTurb;
            _rtPulseMag = targetPulseMag;
            _rtPulseFreq = targetPulseF;

            // Application à la WindZone
            _windZone.windMain = _rtMain;
            _windZone.windTurbulence = _rtTurb;
            _windZone.windPulseMagnitude = _rtPulseMag;
            _windZone.windPulseFrequency = _rtPulseFreq;
        }
    }

}
