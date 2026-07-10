using KinematicCharacterController; // ReadOnlyAttribute (déjà dans le projet, Assets/Plugins/KinematicCharacterController/Core)
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
    /// Snapshot numérique interpolable, indépendant du moteur de rendu. Toute la logique de lerp
    /// passe par cette struct : le reste du manager ne touche un Volume/Light QUE dans ApplyResolved.
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
        /// Unique point de vérité pour la correspondance segment -> profil. Ne jamais indexer par
        /// (int)TimeOfDaySegment : l'ordre de déclaration de l'enum n'est pas garanti stable, un simple
        /// réordonnancement casserait silencieusement tous les blends sans crash.
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
    /// Fenêtre de transition entre deux segments de la journée (ex: Night -> Morning entre 5h et 7h).
    /// En dehors de toutes les fenêtres, on est sur un "plateau" (le segment "To" de la dernière
    /// transition passée).
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

    /// <summary>
    /// REPART DE ZÉRO — étape 2 : les lerps.
    ///
    /// Deux axes, jamais mélangés dans une seule formule :
    ///
    ///  1) HEURE DE LA JOURNÉE (toujours actif) : blend continu entre 2 des 4 profils du
    ///     <see cref="CurrentSet"/> sélectionné (cf étape 1), piloté par <see cref="_transitions"/>.
    ///     C'est un pur Lerp(a, b, t) où t vient de GetBlendSegments : déterministe, pas de timer,
    ///     pas d'état à maintenir entre les frames.
    ///
    ///  2) MÉTÉO (seulement quand elle change) : PAS de blend analytique entre deux météos en
    ///     parallèle. Un seul crossfade linéaire à la fois (timer 0->1 sur _weatherCrossfadeDuration),
    ///     depuis la dernière valeur réellement affichée vers la nouvelle cible (qui elle-même suit
    ///     l'heure en continu via l'axe 1). Une transition à la fois, faite pour être simple à suivre.
    /// </summary>
    public class LightingProfileManager : MonoBehaviour
    {
        #region References

        [Header("Controllers")]
        [SerializeField] private SunController _sunController;
        [SerializeField] private MoonController _moonController;
        [SerializeField] private Volume _globalVolume;

        #endregion

        #region Profiles (Météo -> 4 profils temps de jour)

        [Header("Profiles per weather")]
        public AYellowpaper.SerializedCollections.SerializedDictionary<WeatherType, WeatherProfileSet> WeatherProfiles;

        [Tooltip("Météo utilisée quand WeatherProfiles n'a pas d'entrée (ou un set incomplet) pour la météo demandée.")]
        [SerializeField] private WeatherType _fallbackWeather = WeatherType.Sunny;

        /// <summary>Le set actuellement sélectionné pour la météo courante. Null si rien trouvé (cas rare, cf fallback).</summary>
        public WeatherProfileSet CurrentSet { get; private set; }

        #endregion

        #region Time-of-day transitions (axe 1)

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

        #endregion

        #region External Overrides

        [Header("External Overrides")]
        [Range(-5f, 8f)][SerializeField] private float _additionalExposure = 0f;
        public float AdditionalExposure => _additionalExposure;

        public void SetAdditionalExposure(float ev) => _additionalExposure = Mathf.Clamp(ev, -5f, 8f);
        public void AddToAdditionalExposure(float delta) => SetAdditionalExposure(_additionalExposure + delta);
        public void ClearAdditionalExposure() => _additionalExposure = 0f;

        #endregion

        #region Weather crossfade (axe 2)

        [Header("Weather Crossfade")]
        [Tooltip("Durée (s) du fondu quand la météo affichée change. 0 = instantané (utile pour débug).")]
        [SerializeField] private float _weatherCrossfadeDuration = 4f;

        /// <summary>Météo forcée manuellement (debug / narratif). Prioritaire sur la météo réelle si définie.</summary>
        private WeatherType? _weatherOverride;

        private bool _hasActiveWeather;

        private ResolvedLighting _fromState;
        private ResolvedLighting _toState;
        private float _crossfadeT = 1f;
        private ResolvedLighting _lastApplied;

        #endregion

        #region Sun fade

        [Header("Sun Fade Override")]
        [SerializeField] private float _sunFadeStart = 19f;
        [SerializeField] private float _sunFadeEnd = 20f;
        [SerializeField] private Color _sunFadeColor = Color.black;
        [SerializeField] private float _sunFadeIntensityTarget = 0f;

        #endregion

        #region Debug

        [Header("Debug (lecture seule, à observer en Play mode)")]
        [SerializeField, ReadOnly] private WeatherType _debugCurrentWeather;
        [SerializeField, ReadOnly] private bool _debugSetFound;
        [SerializeField, ReadOnly] private bool _debugSetComplete;
        [SerializeField, ReadOnly] private float _debugCrossfadeT;

        #endregion

        #region Volume Components

        private Fog _fog; private Exposure _exposure; private PhysicallyBasedSky _pbSky;
        private ColorAdjustments _colorAdjustments;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            WeatherHandlerData.OnWeatherTypeChanged += OnWeatherTypeChanged;
            TimeHandlerData.OnTimeChanged += OnTimeChanged;

            if (_sunController != null)
                _sunController.OnShadowOwnershipChanged += Sun_OnShadowOwnershipChanged;
        }

        private void Start()
        {
            if (_globalVolume != null && _globalVolume.profile != null)
            {
                _globalVolume.profile.TryGet(out _fog);
                _globalVolume.profile.TryGet(out _exposure);
                _globalVolume.profile.TryGet(out _pbSky);
                _globalVolume.profile.TryGet(out _colorAdjustments);
            }

            // WeatherManager a déjà tourné son Awake avant notre Start (Unity garantit tous les Awake
            // avant tous les Start), donc WeatherHandlerData.CurrentWeather est déjà valide ici.
            WeatherType initial = _weatherOverride ?? (WeatherHandlerData.CurrentWeather?.WeatherType ?? _fallbackWeather);
            SelectWeatherSet(initial);
        }

        private void Update()
        {
            if (!_hasActiveWeather) return;

            _crossfadeT = _weatherCrossfadeDuration <= 0f
                ? 1f
                : Mathf.Min(1f, _crossfadeT + Time.deltaTime / _weatherCrossfadeDuration);

            _debugCrossfadeT = _crossfadeT;

            _lastApplied = ResolvedLighting.Lerp(_fromState, _toState, _crossfadeT);
            ApplyResolved(_lastApplied);
        }

        private void OnDestroy()
        {
            WeatherHandlerData.OnWeatherTypeChanged -= OnWeatherTypeChanged;
            TimeHandlerData.OnTimeChanged -= OnTimeChanged;

            if (_sunController != null)
                _sunController.OnShadowOwnershipChanged -= Sun_OnShadowOwnershipChanged;
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

        public void OverrideWeather(WeatherType weather)
        {
            _weatherOverride = weather;
            SelectWeatherSet(weather); // déclenche le crossfade tout de suite, pas d'attente du prochain vrai changement météo
        }

        public void ClearWeatherOverride()
        {
            _weatherOverride = null;
            SelectWeatherSet(WeatherHandlerData.CurrentWeather?.WeatherType ?? _fallbackWeather);
        }

        #endregion

        #region Weather selection (étape 1, inchangé dans son principe)

        private void OnWeatherTypeChanged(WeatherType newWeather)
        {
            if (_weatherOverride.HasValue) return; // override manuel prioritaire, on ignore la météo réelle
            SelectWeatherSet(newWeather);
        }

        /// <summary>
        /// Le soleil vient de prendre/perdre les ombres (cf SunController.OnShadowOwnershipChanged).
        /// La lune reçoit exactement l'inverse via sa propre méthode (qui a son propre filtre anti-
        /// réécriture) : jamais les deux actifs en même temps, par construction du signal partagé.
        /// </summary>
        private void Sun_OnShadowOwnershipChanged(bool sunOwnsShadows)
        {
            if (_moonController != null)
                _moonController.SetShadowActive(!sunOwnsShadows);
        }

        /// <summary>
        /// Change la météo "active" pour le crossfade. Ne calcule PAS encore le lighting résolu :
        /// ça, c'est le rôle d'OnTimeChanged (axe 1), qui tourne de toute façon chaque frame.
        /// On se contente ici de mémoriser que la météo a changé, pour qu'OnTimeChanged sache qu'il
        /// doit démarrer un nouveau crossfade au prochain calcul.
        /// </summary>
        private void SelectWeatherSet(WeatherType weather)
        {
            CurrentSet = SelectProfileSet(weather, out bool found);

            _debugCurrentWeather = weather;
            _debugSetFound = found;
            _debugSetComplete = found && CurrentSet.IsComplete;

            if (!found)
                Debug.LogWarning($"[LightingProfileManager] Aucune entrée dans WeatherProfiles pour la météo '{weather}'.");
            else if (!CurrentSet.IsComplete)
                Debug.LogWarning($"[LightingProfileManager] Le set pour '{weather}' existe mais n'a pas ses 4 profils renseignés.");

            // On force un nouveau crossfade au prochain OnTimeChanged en repartant de _lastApplied.
            _hasActiveWeather = false;
        }

        private WeatherProfileSet SelectProfileSet(WeatherType weather, out bool found)
        {
            found = WeatherProfiles != null && WeatherProfiles.TryGetValue(weather, out var set) && set != null && set.IsComplete;
            if (found) return WeatherProfiles[weather];

            // Fallback : si la météo demandée n'a pas ses 4 profils, on retombe sur _fallbackWeather
            // (une seule fois, pas de récursion infinie si le fallback lui-même est mal configuré).
            if (weather != _fallbackWeather
                && WeatherProfiles != null
                && WeatherProfiles.TryGetValue(_fallbackWeather, out var fallbackSet)
                && fallbackSet != null && fallbackSet.IsComplete)
            {
                Debug.LogWarning($"[LightingProfileManager] Fallback sur '{_fallbackWeather}' pour la météo '{weather}'.");
                found = true;
                return fallbackSet;
            }

            return null;
        }

        #endregion

        #region Time Cycle (axe 1) + déclenchement du crossfade météo (axe 2)

        public void OnTimeChanged(float timeOfDay)
        {
            if (CurrentSet == null) return; // rien à résoudre tant qu'aucune météo n'a de set valide

            var target = ResolveTimeOnly(CurrentSet, timeOfDay);

            bool useManualSunFade = timeOfDay >= _sunFadeStart && timeOfDay <= _sunFadeEnd;
            if (useManualSunFade)
            {
                float fadeT = Mathf.InverseLerp(_sunFadeStart, _sunFadeEnd, timeOfDay);
                ApplySunFadeOut(ref target, fadeT);
            }

            if (!_hasActiveWeather)
            {
                // Nouvelle météo (ou tout premier frame) : démarre UN crossfade depuis ce qui est
                // réellement affiché à l'écran (rien à afficher au tout premier frame -> on part
                // directement de la cible, pas de fondu depuis du vide).
                _fromState = _hasEverApplied ? _lastApplied : target;
                _hasActiveWeather = true;
                _crossfadeT = 0f;
            }

            // Que ce soit un nouveau crossfade ou la continuation du précédent, la cible suit l'heure
            // de la journée en continu (axe 1, toujours actif).
            _toState = target;
        }

        private bool _hasEverApplied;

        #endregion

        #region Resolve (axe 1 uniquement : heure de la journée, pour un set déjà sélectionné)

        private ResolvedLighting ResolveTimeOnly(WeatherProfileSet set, float time)
        {
            GetBlendSegments(time, out var fromSeg, out var toSeg, out float t);

            var from = set.Get(fromSeg) ?? set.Midday;
            var to = set.Get(toSeg) ?? set.Midday;

            if (from == null || to == null)
            {
                Debug.LogWarning("[LightingProfileManager] Profil manquant dans le set courant malgré IsComplete == true. Vérifie les assets.");
                return _hasEverApplied ? _lastApplied : default;
            }

            return ResolvedLighting.Lerp(ResolvedLighting.FromProfile(from), ResolvedLighting.FromProfile(to), t);
        }

        private static float Normalize24(float h)
        {
            h %= 24f; if (h < 0f) h += 24f; return h; // [0,24)
        }

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

            // 2) Sinon, plateau = segment "To" de la dernière transition dont on a dépassé la fin.
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

            from = to = TimeOfDaySegment.Midday; t = 0f;
        }

        private static bool InRangeNoWrap(float t, float start, float end) => t >= start && t < end;

        private static bool InRangeWrap(float t, float start, float end)
        {
            start = Normalize24(start); end = Normalize24(end); t = Normalize24(t);
            if (Mathf.Approximately(start, end)) return false;
            if (start < end) return t >= start && t < end;
            return t >= start || t < end;
        }

        private void ApplySunFadeOut(ref ResolvedLighting r, float t)
        {
            r.SunColor = Color.Lerp(r.SunColor, _sunFadeColor, t);
            r.SunIntensity = Mathf.Lerp(r.SunIntensity, _sunFadeIntensityTarget, t);
            r.FlareIntensity = Mathf.Lerp(r.FlareIntensity, 0f, t);
            r.FlareScale = Mathf.Lerp(r.FlareScale, 0f, t);
        }

        #endregion

        #region Apply (écrit sur le moteur — la seule région qui touche Light/Volume)

        private void ApplyResolved(ResolvedLighting r)
        {
            _hasEverApplied = true;

            // --- Sun --- (couleur/intensité/flare uniquement : rotation, enabled, shadows restent
            // gérés par SunController lui-même, cf le fix anti-flicker)
            if (_sunController != null && _sunController.SunLight != null)
            {
                _sunController.SunLight.color = r.SunColor;
                _sunController.SunLight.intensity = r.SunIntensity;
                _sunController.SunLight.colorTemperature = r.SunTemperature;
            }
            if (_sunController != null && _sunController.SunLens != null)
            {
                _sunController.SunLens.intensity = r.FlareIntensity;
                _sunController.SunLens.scale = r.FlareScale;
            }

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