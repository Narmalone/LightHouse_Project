using LightHouse.Features.Weather;
using LightHouse.Features.TimeOfDay.Sun;
using LightHouse.Features.TimeOfDay.Moon;
using LightHouse.Features.TimeOfDay.TimeCore;

using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;
using System;

namespace LightHouse.Features.TimeOfDay.Lighting
{
    /// <summary>
    /// Snapshot numérique interpolable, indépendant du moteur de rendu.
    /// Toute la logique de lerp/moteur passe par cette struct : le reste du
    /// manager ne touche jamais directement un Volume component pendant les calculs.
    /// </summary>
    internal struct ResolvedLighting
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

        public static ResolvedLighting FromProfile(LightingProfile p)
        {
            return new ResolvedLighting
            {
                SunColor = p.sunColor,
                SunIntensity = p.sunIntensity,
                SunTemperature = p.temperature,
                FlareIntensity = p.FlareIntensity,
                FlareScale = p.FlareScale,

                Exposure = p.Exposure,
                Compensation = p.Compensation,

                FogTint = p.Tint,
                BaseHeight = p.BaseHeight,
                MaxHeight = p.MaximumHeight,
                MeanFreePath = p.FogAttenuationDistance,
                MaxFogDistance = p.MaxFogDistance,
                Albedo = p.Albedo,
                VolumetricFog = p.VolumetricFog,
                DenoisingMode = p.DenoisingMode,
                GIDimmer = p.GIDimmer,

                GroundTint = p.GroundTint,
                HorizonTint = p.HorizonTint,
                ZenithTint = p.ZenithTint,
                HorizonZenithShift = p.HorizonZenithShift,
                AerosolDensity = p.AerosolDensity,
                AerosolTint = p.AerosolTint,
                AerosolMaxAltitude = p.AerosolMaximumAltitude,

                PostExposure = p.PostExposure,
                Contrast = p.Contrasts,
                Saturation = p.Saturation
            };
        }

        /// <summary>
        /// Unique fonction de lerp pour toute la struct (remplace les deux
        /// fonctions dupliquées ResolveInterpolated / LerpResolved du script d'origine).
        /// </summary>
        public static ResolvedLighting Lerp(ResolvedLighting a, ResolvedLighting b, float t)
        {
            return new ResolvedLighting
            {
                SunColor = Color.Lerp(a.SunColor, b.SunColor, t),
                SunIntensity = Mathf.Lerp(a.SunIntensity, b.SunIntensity, t),
                SunTemperature = Mathf.Lerp(a.SunTemperature, b.SunTemperature, t),
                FlareIntensity = Mathf.Lerp(a.FlareIntensity, b.FlareIntensity, t),
                FlareScale = Mathf.Lerp(a.FlareScale, b.FlareScale, t),

                Exposure = Mathf.Lerp(a.Exposure, b.Exposure, t),
                Compensation = Mathf.Lerp(a.Compensation, b.Compensation, t),

                FogTint = Color.Lerp(a.FogTint, b.FogTint, t),
                BaseHeight = Mathf.Lerp(a.BaseHeight, b.BaseHeight, t),
                MaxHeight = Mathf.Lerp(a.MaxHeight, b.MaxHeight, t),
                MeanFreePath = Mathf.Lerp(a.MeanFreePath, b.MeanFreePath, t),
                MaxFogDistance = Mathf.Lerp(a.MaxFogDistance, b.MaxFogDistance, t),
                Albedo = Color.Lerp(a.Albedo, b.Albedo, t),
                VolumetricFog = t < 0.5f ? a.VolumetricFog : b.VolumetricFog,
                DenoisingMode = t < 0.5f ? a.DenoisingMode : b.DenoisingMode,
                GIDimmer = Mathf.Lerp(a.GIDimmer, b.GIDimmer, t),

                GroundTint = Color.Lerp(a.GroundTint, b.GroundTint, t),
                HorizonTint = Color.Lerp(a.HorizonTint, b.HorizonTint, t),
                ZenithTint = Color.Lerp(a.ZenithTint, b.ZenithTint, t),
                HorizonZenithShift = Mathf.Lerp(a.HorizonZenithShift, b.HorizonZenithShift, t),
                AerosolDensity = Mathf.Lerp(a.AerosolDensity, b.AerosolDensity, t),
                AerosolTint = Color.Lerp(a.AerosolTint, b.AerosolTint, t),
                AerosolMaxAltitude = Mathf.Lerp(a.AerosolMaxAltitude, b.AerosolMaxAltitude, t),

                PostExposure = Mathf.Lerp(a.PostExposure, b.PostExposure, t),
                Contrast = Mathf.Lerp(a.Contrast, b.Contrast, t),
                Saturation = Mathf.Lerp(a.Saturation, b.Saturation, t)
            };
        }
    }

    /// <summary>
    /// 4 profils de lighting pour une météo donnée (un par segment de journée).
    /// </summary>
    [System.Serializable]
    public class WeatherProfileSet
    {
        public LightingProfile Night;
        public LightingProfile Morning;
        public LightingProfile Midday;
        public LightingProfile Evening;

        /// <summary>
        /// Unique point de vérité pour la correspondance segment -> profil.
        /// IMPORTANT : ne jamais indexer par (int)TimeOfDaySegment. L'ordre de
        /// déclaration de l'enum n'est pas garanti stable dans le temps ; un simple
        /// réordonnancement casserait silencieusement tous les blends (pas de crash,
        /// juste le mauvais profil appliqué). Le switch explicite est robuste à ça.
        /// </summary>
        public LightingProfile Get(TimeOfDaySegment seg) => seg switch
        {
            TimeOfDaySegment.Night => Night,
            TimeOfDaySegment.Morning => Morning,
            TimeOfDaySegment.Midday => Midday,
            TimeOfDaySegment.Evening => Evening,
            _ => Midday
        };

        public bool IsComplete => Night != null && Morning != null && Midday != null && Evening != null;
    }

    /// <summary>
    /// Fenêtre de transition entre deux segments de la journée
    /// (ex: Night -> Morning entre 5h et 7h). En dehors de toutes les fenêtres,
    /// on est sur un "plateau" (le segment "To" de la dernière transition passée).
    /// </summary>
    [System.Serializable]
    public struct SegmentTransition
    {
        public TimeOfDaySegment From;
        public TimeOfDaySegment To;

        [Tooltip("Heure de début / fin de la fenêtre de transition (0..24)")]
        public Vector2 Window;

        public AnimationCurve Curve;

        public readonly float Start => Window.x;
        public readonly float End => Window.y;
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

        #region Profiles (Météo -> 4 profils temps de jour)

        [Header("Profiles per weather")]
        public AYellowpaper.SerializedCollections.SerializedDictionary<WeatherType, WeatherProfileSet> WeatherProfiles;

        #endregion

        #region Time-of-day transitions

        [Header("Transitions Jour/Nuit (dans l'ordre chronologique)")]
        [SerializeField]
        private SegmentTransition[] _transitions = new SegmentTransition[]
        {
            new SegmentTransition
            {
                From = TimeOfDaySegment.Night, To = TimeOfDaySegment.Morning,
                Window = new Vector2(5f, 7f),
                Curve = AnimationCurve.EaseInOut(0, 0, 1, 1)
            },
            new SegmentTransition
            {
                From = TimeOfDaySegment.Morning, To = TimeOfDaySegment.Midday,
                Window = new Vector2(9f, 12f),
                Curve = AnimationCurve.EaseInOut(0, 0, 1, 1)
            },
            new SegmentTransition
            {
                From = TimeOfDaySegment.Midday, To = TimeOfDaySegment.Evening,
                Window = new Vector2(15f, 18f),
                Curve = AnimationCurve.EaseInOut(0, 0, 1, 1)
            },
            new SegmentTransition
            {
                From = TimeOfDaySegment.Evening, To = TimeOfDaySegment.Night,
                Window = new Vector2(21f, 23f),
                Curve = AnimationCurve.EaseInOut(0, 0, 1, 1)
            },
        };

        [Header("External Overrides")]
        [Range(-5f, 8f)][SerializeField] private float _additionalExposure = 0f;
        public float AdditionalExposure => _additionalExposure;

        public void SetAdditionalExposure(float ev) => _additionalExposure = Mathf.Clamp(ev, -5f, 8f);
        public void AddToAdditionalExposure(float delta) => SetAdditionalExposure(_additionalExposure + delta);
        public void ClearAdditionalExposure() => _additionalExposure = 0f;

        #endregion

        #region Sun fade

        [Header("Sun Fade Override")]
        [SerializeField] private float _sunFadeStart = 19f;
        [SerializeField] private float _sunFadeEnd = 20f;
        [SerializeField] private Color _sunFadeColor = Color.black;
        [SerializeField] private float _sunFadeIntensityTarget = 0f;

        #endregion

        #region Weather smoothing (continu, sans state machine à 2 météos)

        [Header("Weather Smoothing")]
        [Tooltip("Temps caractéristique (s) du lissage. 0 = application instantanée, sans transition.")]
        [SerializeField] private float _weatherSmoothTime = 2f;

        /// <summary>Météo forcée manuellement (debug / narratif). Prioritaire sur la météo réelle si définie.</summary>
        private WeatherType? _weatherOverride;

        private ResolvedLighting _smoothed;
        private bool _hasSmoothed;

        #endregion

        #region Volume Components & Privates

        private Fog _fog; private Exposure _exposure; private PhysicallyBasedSky _pbSky;
        private ColorAdjustments _colorAdjustments;

        private ResolvedLighting _instantTarget;
        private bool _hasInstantTarget;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            _sunController.OnSunLightToggled += Sun_OnSunLightToggled;
            TimeHandlerData.OnTimeChanged += OnTimeChanged;
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
            if (!_hasInstantTarget) return;

            if (!_hasSmoothed || _weatherSmoothTime <= 0f)
            {
                _smoothed = _instantTarget;
                _hasSmoothed = true;
            }
            else
            {
                // Lissage exponentiel indépendant du framerate (critically-damped-like).
                float rate = 1f - Mathf.Exp(-Time.deltaTime / _weatherSmoothTime);
                _smoothed = ResolvedLighting.Lerp(_smoothed, _instantTarget, rate);
            }

            ApplyResolved(_smoothed);
        }

        private void OnDestroy()
        {
            _sunController.OnSunLightToggled -= Sun_OnSunLightToggled;
            TimeHandlerData.OnTimeChanged -= OnTimeChanged;
        }

        private void OnValidate()
        {
            if (WeatherProfiles == null) return;
            foreach (var kv in WeatherProfiles)
            {
                if (kv.Value == null || !kv.Value.IsComplete)
                    Debug.LogWarning($"[LightingProfileManager] La météo '{kv.Key}' n'a pas ses 4 profils (Night/Morning/Midday/Evening) renseignés.");
            }
        }

        #endregion

        #region Public API

        /// <summary>
        /// Force une météo pour le lighting, indépendamment de la météo réelle du monde
        /// (debug, script narratif, etc.). Le lissage continu s'occupe de la transition.
        /// </summary>
        public void OverrideWeather(WeatherType weather) => _weatherOverride = weather;

        /// <summary>Annule le forçage et revient à la météo réelle du monde.</summary>
        public void ClearWeatherOverride() => _weatherOverride = null;

        #endregion

        #region Event Handlers

        private void Sun_OnSunLightToggled(bool isEnabled)
        {
            _moonController.MoonLight.shadows = isEnabled ? LightShadows.None : LightShadows.Soft;
        }

        #endregion

        #region Time Cycle

        public void OnTimeChanged(float timeOfDay)
        {
            // La météo utilisée est soit un override manuel, soit la météo réelle courante
            // du monde (WeatherManager la met à jour en continu, cf WeatherHandlerData).
            WeatherType currentWeather = _weatherOverride ?? (WeatherHandlerData.CurrentWeather?.WeatherType ?? default);

            var resolved = EvaluateResolved(currentWeather, timeOfDay);

            bool useManualSunFade = timeOfDay >= _sunFadeStart && timeOfDay <= _sunFadeEnd;
            if (useManualSunFade)
            {
                float fadeT = Mathf.InverseLerp(_sunFadeStart, _sunFadeEnd, timeOfDay);
                ApplySunFadeOut(ref resolved, fadeT);
            }

            _instantTarget = resolved;
            _hasInstantTarget = true;
        }

        #endregion

        #region Interpolation + Resolve

        private ResolvedLighting EvaluateResolved(WeatherType weather, float time)
        {
            if (WeatherProfiles == null || !WeatherProfiles.TryGetValue(weather, out var profileSet) || profileSet == null)
            {
                Debug.LogWarning($"[LightingProfileManager] Aucun profil pour la météo '{weather}'.");
                return _hasInstantTarget ? _instantTarget : default;
            }

            GetBlendSegments(time, out var fromSeg, out var toSeg, out float t);

            var from = profileSet.Get(fromSeg) ?? profileSet.Midday;
            var to = profileSet.Get(toSeg) ?? profileSet.Midday;

            if (from == null || to == null)
            {
                Debug.LogWarning($"[LightingProfileManager] Profils manquants pour la météo '{weather}' (segments {fromSeg}/{toSeg}).");
                return _hasInstantTarget ? _instantTarget : default;
            }

            return ResolvedLighting.Lerp(ResolvedLighting.FromProfile(from), ResolvedLighting.FromProfile(to), t);
        }

        private static float Normalize24(float h)
        {
            h %= 24f; if (h < 0f) h += 24f; return h; // [0,24)
        }

        /// <summary>
        /// Détermine dans quelle transition (ou plateau) se trouve l'heure donnée,
        /// en se basant sur la liste ordonnée de <see cref="_transitions"/>.
        /// Ajouter/modifier une fenêtre se fait uniquement dans l'inspecteur.
        /// </summary>
        private void GetBlendSegments(float time, out TimeOfDaySegment from, out TimeOfDaySegment to, out float t)
        {
            time = Normalize24(time);

            if (_transitions == null || _transitions.Length == 0)
            {
                from = to = TimeOfDaySegment.Midday; t = 0f; return;
            }

            // 1) Est-on dans une fenêtre de transition ?
            for (int i = 0; i < _transitions.Length; i++)
            {
                var tr = _transitions[i];
                if (InRangeNoWrap(time, tr.Start, tr.End))
                {
                    from = tr.From; to = tr.To;
                    float raw = Mathf.InverseLerp(tr.Start, tr.End, time);
                    t = tr.Curve != null ? tr.Curve.Evaluate(raw) : raw;
                    return;
                }
            }

            // 2) Sinon, plateau = segment "To" de la dernière transition dont on a
            // dépassé la fin, en cherchant dans l'ordre chronologique (avec wrap minuit).
            for (int i = 0; i < _transitions.Length; i++)
            {
                var current = _transitions[i];
                var next = _transitions[(i + 1) % _transitions.Length];

                if (InRangeWrap(time, current.End, next.Start))
                {
                    from = to = current.To;
                    t = 0f;
                    return;
                }
            }

            // Fallback (ne devrait pas arriver si les fenêtres couvrent tout le cycle)
            from = to = TimeOfDaySegment.Midday; t = 0f;
        }

        private static bool InRangeNoWrap(float t, float start, float end) => t >= start && t < end;

        private static bool InRangeWrap(float t, float start, float end) // [start, end) modulo 24
        {
            start = Normalize24(start); end = Normalize24(end); t = Normalize24(t);
            if (Mathf.Approximately(start, end)) return false;
            if (start < end) return t >= start && t < end;
            return t >= start || t < end; // wrap autour de minuit
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