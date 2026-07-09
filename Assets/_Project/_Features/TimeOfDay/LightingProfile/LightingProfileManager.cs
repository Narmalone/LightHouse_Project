using LightHouse.Features.Weather;
using LightHouse.Features.TimeOfDay.Sun;
using LightHouse.Features.TimeOfDay.Moon;
using LightHouse.Features.TimeOfDay.TimeCore;

using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;
using System.Collections.Generic;


namespace LightHouse.Features.TimeOfDay.Lighting
{
    // Un "snapshot" numérique pour faire les lerps sans toucher à l'engine à chaque étape
    struct ResolvedLighting
    {
        // Sun
        public Color SunColor; public float SunIntensity; public float SunTemperature;
        public float FlareIntensity; public float FlareScale;

        // Exposure
        public float Exposure; public float Compensation;

        // Fog
        public Color FogTint; public float BaseHeight; public float MaxHeight;
        public float MeanFreePath; public float MaxFogDistance; public Color Albedo;
        public bool VolumetricFog; public FogDenoisingMode DenoisingMode;
        public float GIDimmer;

        // Sky
        public Color GroundTint; public Color HorizonTint; public Color ZenithTint;
        public float HorizonZenithShift; public float AerosolDensity;
        public Color AerosolTint; public float AerosolMaxAltitude;

        // Color Adjustments
        public float PostExposure; public float Contrast; public float Saturation;
    }

    [System.Serializable]
    public class WeatherProfileSet
    {
        public LightingProfile Night;
        public LightingProfile Morning;
        public LightingProfile Midday;
        public LightingProfile Evening;

        public LightingProfile Get(TimeOfDaySegment seg) => seg switch
        {
            TimeOfDaySegment.Night => Night,
            TimeOfDaySegment.Morning => Morning,
            TimeOfDaySegment.Midday => Midday,
            TimeOfDaySegment.Evening => Evening,
            _ => Midday
        };

        public LightingProfile[] AsArray() => new[] { Night, Morning, Midday, Evening };
    }

    public class LightingProfileManager : MonoBehaviour, ITimeCycleObserver
    {
        #region References

        [Header("Controllers")]
        [SerializeField] private SunController _sunController;
        [SerializeField] private MoonController _moonController;
        [SerializeField] private Volume _globalVolume;

        public SunController SunController => _sunController;
        public MoonController MoonController => _moonController;

        #endregion

        #region Profiles (Météo → 4 profils temps de jour)

        public AYellowpaper.SerializedCollections.SerializedDictionary<WeatherType, WeatherProfileSet> WeatherProfiles;

        // Cache: Weather → array indexable par (int)TimeOfDaySegment
        private readonly Dictionary<WeatherType, LightingProfile[]> _cache = new();

        #endregion

        #region Day segment timings

        [Header("Blend Windows (24h)")]
        [SerializeField] private Vector2 _nightToMorning = new Vector2(6f, 9f);
        [SerializeField] private Vector2 _morningToMidday = new Vector2(9f, 12f);
        [SerializeField] private Vector2 _middayToEvening = new Vector2(15f, 18f);
        [SerializeField] private Vector2 _eveningToNight = new Vector2(21f, 23f);

        [Header("External Overrides")]
        [Range(-5f, 8f)][SerializeField] private float _additionalExposure = 0f;
        public float AdditionalExposure => _additionalExposure;

        public void SetAdditionalExposure(float ev)
        {
            _additionalExposure = Mathf.Clamp(ev, -5f, 8f);
        }

        public void AddToAdditionalExposure(float delta)
        {
            SetAdditionalExposure(_additionalExposure + delta);
        }

        public void ClearAdditionalExposure() => _additionalExposure = 0f;

        [Header("Transition Shapes")]
        [SerializeField] private AnimationCurve _tNightToMorning = AnimationCurve.EaseInOut(0, 0, 1, 1);
        [SerializeField] private AnimationCurve _tMorningToMidday = AnimationCurve.EaseInOut(0, 0, 1, 1);
        [SerializeField] private AnimationCurve _tMiddayToEvening = AnimationCurve.EaseInOut(0, 0, 1, 1);
        [SerializeField] private AnimationCurve _tEveningToNight = AnimationCurve.EaseInOut(0, 0, 1, 1);

        #endregion

        #region Sun fade

        [Header("Sun Fade Override")]
        [SerializeField] private float _sunFadeStart = 19f;
        [SerializeField] private float _sunFadeEnd = 20f;
        [SerializeField] private Color _sunFadeColor = Color.black;
        [SerializeField] private float _sunFadeIntensityTarget = 0f;

        #endregion

        #region Weather blend

        [Header("Weather Blend")]
        [SerializeField] private float _weatherBlendDuration = 2f;
        private WeatherType _activeWeather;
        private WeatherType _targetWeather;
        private float _weatherBlend = -1f; // <0 = pas de blend en cours

        #endregion

        #region Volume Components & Privates

        private TimeManager _timeManager;
        private Fog _fog; private Exposure _exposure; private PhysicallyBasedSky _pbSky;
        private ColorAdjustments _colorAdjustments;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            _sunController.OnSunLightToggled += Sun_OnSunLightToggled;
            TimeHandlerData.OnTimeChanged += OnTimeChanged;

            RebuildCache();

            // Choisis une météo par défaut si besoin
            if (WeatherProfiles.Count > 0)
            {
                foreach (var kv in WeatherProfiles) { _activeWeather = kv.Key; break; }
                _targetWeather = _activeWeather;
            }
        }

        private void Start()
        {
            _globalVolume.profile.TryGet(out _fog);
            _globalVolume.profile.TryGet(out _exposure);
            _globalVolume.profile.TryGet(out _pbSky);
            _globalVolume.profile.TryGet(out _colorAdjustments);
        }

        private void Update()
        {
            // Avance le blend météo si en cours
            if (_weatherBlend >= 0f && _weatherBlendDuration > 0f)
            {
                _weatherBlend += Time.deltaTime / _weatherBlendDuration;
                if (_weatherBlend >= 1f)
                {
                    _activeWeather = _targetWeather;
                    _weatherBlend = -1f;
                }
            }
        }

        private void OnDestroy()
        {
            _sunController.OnSunLightToggled -= Sun_OnSunLightToggled;
            TimeHandlerData.OnTimeChanged -= OnTimeChanged;
        }

        private void OnValidate() => RebuildCache();

        private void RebuildCache()
        {
            _cache.Clear();
            if (WeatherProfiles == null) return;
            foreach (var kv in WeatherProfiles)
                _cache[kv.Key] = kv.Value?.AsArray();
        }

        #endregion

        #region Public API

        // À appeler depuis ton système météo
        public void SetWeather(WeatherType newWeather)
        {
            if (newWeather.Equals(_activeWeather)) return;
            if (!_cache.ContainsKey(newWeather)) { Debug.LogWarning($"No profiles for weather: {newWeather}"); return; }

            _targetWeather = newWeather;
            _weatherBlend = 0f; // démarre le crossfade météo
        }

        #endregion

        #region Event Handlers

        private void Sun_OnSunLightToggled(bool isEnabled)
        {
            _moonController.MoonLight.shadows = isEnabled ? LightShadows.None : LightShadows.Soft;
        }

        #endregion

        #region Time Cycle
        private int _lastFrame = -1;

        public void OnTimeChanged(float timeOfDay)
        {
            if (_lastFrame == Time.frameCount)
                Debug.LogWarning($"Deux appels la même frame ! time={timeOfDay}");

            _lastFrame = Time.frameCount;
            Debug.Log(timeOfDay);
            // 1) Résoudre profil “intra-météo” pour météo active
            var a = EvaluateResolved(_activeWeather, timeOfDay);

            // 2) Si cross-weather en cours, résoudre aussi la cible et faire un 2e lerp
            ResolvedLighting final = a;
            if (_weatherBlend >= 0f && _targetWeather.Equals(_activeWeather) == false)
            {
                var b = EvaluateResolved(_targetWeather, timeOfDay);
                float wt = Mathf.Clamp01(_weatherBlendDuration <= 0f ? 1f : _weatherBlend);
                final = LerpResolved(a, b, wt);
            }

            // 3) Sun fade manuel si dans la fenêtre
            bool useManualSunFade = timeOfDay >= _sunFadeStart && timeOfDay <= _sunFadeEnd;
            if (useManualSunFade)
            {
                float fadeT = Mathf.InverseLerp(_sunFadeStart, _sunFadeEnd, timeOfDay);
                ApplySunFadeOut(ref final, fadeT);
            }

            // 4) Appliquer au rendu
            ApplyResolved(final);
        }

        #endregion

        #region Interpolation + Resolve

        private ResolvedLighting EvaluateResolved(WeatherType weather, float time)
        {
            var arr = _cache.TryGetValue(weather, out var val) ? val : null;
            if (arr == null)
            {
                Debug.LogWarning($"Weather '{weather}' has no cached profiles.");
                return default;
            }

            GetBlendSegments(time, out var fromSeg, out var toSeg, out float t);
            var from = arr[(int)fromSeg] ?? arr[(int)TimeOfDaySegment.Midday];
            var to = arr[(int)toSeg] ?? arr[(int)TimeOfDaySegment.Midday];

            //Debug.Log($"weather target: {to.name}");
            return ResolveInterpolated(from, to, t);
        }


        private static float Normalize24(float h)
        {
            h %= 24f; if (h < 0f) h += 24f; return h; // [0,24)
        }

        private static bool InRangeNoWrap(float t, float start, float end) // [start, end)
        {
            return t >= start && t < end;
        }


        private void GetBlendSegments(float time,
      out TimeOfDaySegment from, out TimeOfDaySegment to, out float t)
        {
            time = Normalize24(time);

            // 1) Night → Morning : 06–09
            if (InRangeNoWrap(time, _nightToMorning.x, _nightToMorning.y))
            {
                from = TimeOfDaySegment.Night; to = TimeOfDaySegment.Morning;
                t = Mathf.InverseLerp(_nightToMorning.x, _nightToMorning.y, time);
                t = _tNightToMorning.Evaluate(t);
                return;
            }

            // 2) Morning → Midday : 09–12
            if (InRangeNoWrap(time, _morningToMidday.x, _morningToMidday.y))
            {
                from = TimeOfDaySegment.Morning; to = TimeOfDaySegment.Midday;
                t = Mathf.InverseLerp(_morningToMidday.x, _morningToMidday.y, time);
                t = _tMorningToMidday.Evaluate(t);
                return;
            }

            // 3) Midday → Evening : 15–18
            if (InRangeNoWrap(time, _middayToEvening.x, _middayToEvening.y))
            {
                from = TimeOfDaySegment.Midday; to = TimeOfDaySegment.Evening;
                t = Mathf.InverseLerp(_middayToEvening.x, _middayToEvening.y, time);
                t = _tMiddayToEvening.Evaluate(t);
                return;
            }

            // 4) Evening → Night : 21–23
            if (InRangeNoWrap(time, _eveningToNight.x, _eveningToNight.y))
            {
                from = TimeOfDaySegment.Evening; to = TimeOfDaySegment.Night;
                t = Mathf.InverseLerp(_eveningToNight.x, _eveningToNight.y, time);
                t = _tEveningToNight.Evaluate(t);
                return;
            }

            // --- PLATEAUX ---
            // Night plateau: [23–24) ∪ [0–6)
            if (time >= _eveningToNight.y || time < _nightToMorning.x)
            {
                from = TimeOfDaySegment.Night; to = TimeOfDaySegment.Night; t = 0f; return;
            }

            // Morning plateau: [9–9) n'existe pas (fenêtre comblée), mais si tu changes les fenêtres :
            if (time >= _nightToMorning.y && time < _morningToMidday.x)
            {
                from = TimeOfDaySegment.Morning; to = TimeOfDaySegment.Morning; t = 0f; return;
            }

            // Midday plateau: [12–15)
            if (time >= _morningToMidday.y && time < _middayToEvening.x)
            {
                from = TimeOfDaySegment.Midday; to = TimeOfDaySegment.Midday; t = 0f; return;
            }

            // Evening plateau: [18–21)
            if (time >= _middayToEvening.y && time < _eveningToNight.x)
            {
                from = TimeOfDaySegment.Evening; to = TimeOfDaySegment.Evening; t = 0f; return;
            }

            // fallback (ne devrait pas arriver)
            from = TimeOfDaySegment.Midday; to = TimeOfDaySegment.Midday; t = 0f;
        }

        private static ResolvedLighting ResolveInterpolated(LightingProfile a, LightingProfile b, float t)
        {
            ResolvedLighting r;

            // Sun
            r.SunColor = Color.Lerp(a.sunColor, b.sunColor, t);
            r.SunIntensity = Mathf.Lerp(a.sunIntensity, b.sunIntensity, t);
            r.SunTemperature = Mathf.Lerp(a.temperature, b.temperature, t);
            r.FlareIntensity = Mathf.Lerp(a.FlareIntensity, b.FlareIntensity, t);
            r.FlareScale = Mathf.Lerp(a.FlareScale, b.FlareScale, t);

            // Exposure
            r.Exposure = Mathf.Lerp(a.Exposure, b.Exposure, t);
            r.Compensation = Mathf.Lerp(a.Compensation, b.Compensation, t);

            // Fog
            r.FogTint = Color.Lerp(a.Tint, b.Tint, t);
            r.BaseHeight = Mathf.Lerp(a.BaseHeight, b.BaseHeight, t);
            Debug.Log(a.name + " " + b.name);
            r.MaxHeight = Mathf.Lerp(a.MaximumHeight, b.MaximumHeight, t);
            r.MeanFreePath = Mathf.Lerp(a.FogAttenuationDistance, b.FogAttenuationDistance, t);
            r.MaxFogDistance = Mathf.Lerp(a.MaxFogDistance, b.MaxFogDistance, t);
            r.Albedo = Color.Lerp(a.Albedo, b.Albedo, t);
            r.VolumetricFog = (t < 0.5f ? a.VolumetricFog : b.VolumetricFog);
            r.DenoisingMode = (t < 0.5f ? a.DenoisingMode : b.DenoisingMode);
            r.GIDimmer = Mathf.Lerp(a.GIDimmer, b.GIDimmer, t);

            // Sky
            r.GroundTint = Color.Lerp(a.GroundTint, b.GroundTint, t);
            r.HorizonTint = Color.Lerp(a.HorizonTint, b.HorizonTint, t);
            r.ZenithTint = Color.Lerp(a.ZenithTint, b.ZenithTint, t);
            r.HorizonZenithShift = Mathf.Lerp(a.HorizonZenithShift, b.HorizonZenithShift, t);
            r.AerosolDensity = Mathf.Lerp(a.AerosolDensity, b.AerosolDensity, t);
            r.AerosolTint = Color.Lerp(a.AerosolTint, b.AerosolTint, t);
            r.AerosolMaxAltitude = Mathf.Lerp(a.AerosolMaximumAltitude, b.AerosolMaximumAltitude, t);

            // Color Adjustments
            r.PostExposure = Mathf.Lerp(a.PostExposure, b.PostExposure, t);
            r.Contrast = Mathf.Lerp(a.Contrasts, b.Contrasts, t);
            r.Saturation = Mathf.Lerp(a.Saturation, b.Saturation, t);

            return r;
        }

        private static ResolvedLighting LerpResolved(ResolvedLighting x, ResolvedLighting y, float t)
        {
            ResolvedLighting r;
            // Sun
            r.SunColor = Color.Lerp(x.SunColor, y.SunColor, t);
            r.SunIntensity = Mathf.Lerp(x.SunIntensity, y.SunIntensity, t);
            r.SunTemperature = Mathf.Lerp(x.SunTemperature, y.SunTemperature, t);
            r.FlareIntensity = Mathf.Lerp(x.FlareIntensity, y.FlareIntensity, t);
            r.FlareScale = Mathf.Lerp(x.FlareScale, y.FlareScale, t);

            // Exposure
            r.Exposure = Mathf.Lerp(x.Exposure, y.Exposure, t);
            r.Compensation = Mathf.Lerp(x.Compensation, y.Compensation, t);

            // Fog
            r.FogTint = Color.Lerp(x.FogTint, y.FogTint, t);
            r.BaseHeight = Mathf.Lerp(x.BaseHeight, y.BaseHeight, t);
            r.MaxHeight = Mathf.Lerp(x.MaxHeight, y.MaxHeight, t);
            r.MeanFreePath = Mathf.Lerp(x.MeanFreePath, y.MeanFreePath, t);
            r.MaxFogDistance = Mathf.Lerp(x.MaxFogDistance, y.MaxFogDistance, t);
            r.Albedo = Color.Lerp(x.Albedo, y.Albedo, t);
            r.VolumetricFog = (t < 0.5f ? x.VolumetricFog : y.VolumetricFog);
            r.DenoisingMode = (t < 0.5f ? x.DenoisingMode : y.DenoisingMode);
            r.GIDimmer = Mathf.Lerp(x.GIDimmer, y.GIDimmer, t);

            // Sky
            r.GroundTint = Color.Lerp(x.GroundTint, y.GroundTint, t);
            r.HorizonTint = Color.Lerp(x.HorizonTint, y.HorizonTint, t);
            r.ZenithTint = Color.Lerp(x.ZenithTint, y.ZenithTint, t);
            r.HorizonZenithShift = Mathf.Lerp(x.HorizonZenithShift, y.HorizonZenithShift, t);
            r.AerosolDensity = Mathf.Lerp(x.AerosolDensity, y.AerosolDensity, t);
            r.AerosolTint = Color.Lerp(x.AerosolTint, y.AerosolTint, t);
            r.AerosolMaxAltitude = Mathf.Lerp(x.AerosolMaxAltitude, y.AerosolMaxAltitude, t);

            // Color Adjustments
            r.PostExposure = Mathf.Lerp(x.PostExposure, y.PostExposure, t);
            r.Contrast = Mathf.Lerp(x.Contrast, y.Contrast, t);
            r.Saturation = Mathf.Lerp(x.Saturation, y.Saturation, t);

            return r;
        }

        private void ApplySunFadeOut(ref ResolvedLighting r, float t)
        {
            r.SunColor = Color.Lerp(r.SunColor, _sunFadeColor, t);
            r.SunIntensity = Mathf.Lerp(r.SunIntensity, _sunFadeIntensityTarget, t);
            r.FlareIntensity = Mathf.Lerp(r.FlareIntensity, 0f, t);
            r.FlareScale = Mathf.Lerp(r.FlareScale, 0f, t);
        }

        private void ApplyResolved(ResolvedLighting r)
        {
            // --- Sun ---
            _sunController.SunLight.color = r.SunColor;
            _sunController.SunLight.intensity = r.SunIntensity;
            _sunController.SunLight.colorTemperature = r.SunTemperature;
            _sunController.SunLens.intensity = r.FlareIntensity;
            _sunController.SunLens.scale = r.FlareScale;

            // --- Exposure ---
            if (_exposure != null)
            {
                _exposure.fixedExposure.value = r.Exposure + _additionalExposure;
                _exposure.compensation.value = r.Compensation;
            }

            // --- Fog ---
            if (_fog != null)
            {
                _fog.tint.value = r.FogTint;
                _fog.baseHeight.value = r.BaseHeight;
                _fog.maximumHeight.value = r.MaxHeight;
                _fog.meanFreePath.value = r.MeanFreePath;
                _fog.maxFogDistance.value = r.MaxFogDistance;
                _fog.albedo.value = r.Albedo;
                _fog.enableVolumetricFog.value = r.VolumetricFog;
                _fog.denoisingMode.value = r.DenoisingMode;
                _fog.globalLightProbeDimmer.value = r.GIDimmer;
            }

            // --- Sky ---
            if (_pbSky != null)
            {
                _pbSky.groundTint.value = r.GroundTint;
                _pbSky.horizonTint.value = r.HorizonTint;
                _pbSky.zenithTint.value = r.ZenithTint;
                _pbSky.horizonZenithShift.value = r.HorizonZenithShift;
                _pbSky.aerosolDensity.value = r.AerosolDensity;
                _pbSky.aerosolTint.value = r.AerosolTint;
                _pbSky.aerosolMaximumAltitude.value = r.AerosolMaxAltitude;
            }

            // --- Color Adjustments ---
            if (_colorAdjustments != null)
            {
                _colorAdjustments.postExposure.value = r.PostExposure;
                _colorAdjustments.contrast.value = r.Contrast;
                _colorAdjustments.saturation.value = r.Saturation;
            }
        }

        #endregion
    }
}
