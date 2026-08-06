using KinematicCharacterController; // ReadOnlyAttribute (déjà dans le projet, Assets/Plugins/KinematicCharacterController/Core)
using LightHouse.Features.Weather;
using LightHouse.Features.TimeOfDay.Sun;
using LightHouse.Features.TimeOfDay.Moon;
using LightHouse.Features.TimeOfDay.TimeCore;

using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

namespace LightHouse.Features.TimeOfDay.Lighting
{
    /// <summary>
    /// 4 presets (Night/Morning/Midday/Evening) pour une météo donnée.
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
        /// (int)TimeOfDaySegment : l'ordre de déclaration de l'enum n'est pas garanti stable.
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
    /// v4 : on garde le principe simple de v3 (un seul Lerp continu du Volume/Light vers un preset
    /// cible, pas de timer, pas de machine à états) et on ajoute juste le choix du BON preset parmi
    /// les 4 (Night/Morning/Midday/Evening) de la météo courante, selon l'heure.
    ///
    /// Pas de fenêtres de transition avec courbes ni de blend explicite entre 2 segments : on
    /// sélectionne un segment (fonction pure de l'heure, sans état), et c'est le Lerp continu de
    /// LerpTowardPreset qui, à chaque frame, rapproche le lighting affiché du preset du segment
    /// courant. Quand l'heure fait basculer le segment, la cible change d'un coup mais le lighting
    /// affiché continue de la suivre en douceur (même mécanisme que pour la météo) : pas besoin d'un
    /// deuxième système de blend séparé.
    /// </summary>
    public class LightingProfileManager : MonoBehaviour
    {
        #region References

        [Header("Controllers")]
        [SerializeField] private SunController _sunController;
        [SerializeField] private MoonController _moonController;
        [SerializeField] private Volume _globalVolume;

        #endregion

        #region Presets (4 par météo)

        [Header("4 presets (Night/Morning/Midday/Evening) par météo")]
        [SerializeField] private AYellowpaper.SerializedCollections.SerializedDictionary<WeatherType, WeatherProfileSet> _weatherPresets;

        [Tooltip("Set utilisé quand _weatherPresets n'a pas d'entrée (ou un set incomplet) pour la météo demandée.")]
        [SerializeField] private WeatherType _fallbackWeather = WeatherType.Sunny;

        #endregion

        #region Segments horaires (fonction pure heure -> segment, aucun état)

        [Header("Heures de début de chaque segment (0..24)")]
        [SerializeField] private float _morningStart = 5f;
        [SerializeField] private float _middayStart = 9f;
        [SerializeField] private float _eveningStart = 18f;
        [SerializeField] private float _nightStart = 21f;

        #endregion

        #region Lerp speed

        [Header("Lerp")]
        [Tooltip("Vitesse du lerp vers le preset courant (par seconde).")]
        [SerializeField] private float _lerpSpeed = 1.5f;

        #endregion

        #region External Overrides

        [Header("External Overrides")]
        [Range(-5f, 8f)][SerializeField] private float _additionalExposure = 0f;
        public float AdditionalExposure => _additionalExposure;

        public void SetAdditionalExposure(float ev) => _additionalExposure = Mathf.Clamp(ev, -5f, 8f);
        public void AddToAdditionalExposure(float delta) => SetAdditionalExposure(_additionalExposure + delta);
        public void ClearAdditionalExposure() => _additionalExposure = 0f;

        #endregion

        #region Météo courante

        private WeatherType? _weatherOverride;
        private WeatherType _currentWeather;
        private bool _hasWeather;
        private bool _hasAppliedOnce;

        private float _currentTimeOfDay;
        private bool _hasTime;

        #endregion

        #region Debug

        [Header("Debug (lecture seule)")]
        [SerializeField, ReadOnly] private WeatherType _debugCurrentWeather;
        [SerializeField, ReadOnly] private bool _debugPresetFound;
        [SerializeField, ReadOnly] private TimeOfDaySegment _debugCurrentSegment;

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

            _currentWeather = _weatherOverride ?? (WeatherHandlerData.CurrentWeather?.WeatherType ?? _fallbackWeather);
            _hasWeather = true;
        }

        private void Update()
        {
            if (!_hasWeather || !_hasTime) return;

            var segment = GetSegment(_currentTimeOfDay);
            var preset = SelectPreset(_currentWeather, segment, out bool found);
            _debugCurrentWeather = _currentWeather;
            _debugPresetFound = found;
            _debugCurrentSegment = segment;

            if (preset == null) return;

            // Premier frame utile : on saute directement sur le preset au lieu de lerper depuis
            // quoi que ce soit d'incohérent qui traînait dans l'asset de Volume (c'est ce qui
            // provoquait l'écran noir de quelques secondes au lancement).
            float t = _hasAppliedOnce ? Mathf.Clamp01(_lerpSpeed * Time.deltaTime) : 1f;
            LerpTowardPreset(preset, t);
            _hasAppliedOnce = true;
        }

        private void OnDestroy()
        {
            WeatherHandlerData.OnWeatherTypeChanged -= OnWeatherTypeChanged;
            TimeHandlerData.OnTimeChanged -= OnTimeChanged;

            if (_sunController != null)
                _sunController.OnShadowOwnershipChanged -= Sun_OnShadowOwnershipChanged;
        }

        #endregion

        #region Public API

        public void OverrideWeather(WeatherType weather)
        {
            _weatherOverride = weather;
            _currentWeather = weather;
            _hasWeather = true;
        }

        public void ClearWeatherOverride()
        {
            _weatherOverride = null;
            _currentWeather = WeatherHandlerData.CurrentWeather?.WeatherType ?? _fallbackWeather;
            _hasWeather = true;
        }

        #endregion

        #region Events

        private void OnWeatherTypeChanged(WeatherType newWeather)
        {
            if (_weatherOverride.HasValue) return; // override manuel prioritaire
            _currentWeather = newWeather;
            _hasWeather = true;
        }

        private void OnTimeChanged(float timeOfDay)
        {
            _currentTimeOfDay = timeOfDay;
            _hasTime = true;
        }

        /// <summary>
        /// Le soleil vient de prendre/perdre les ombres. La lune reçoit l'inverse.
        /// </summary>
        private void Sun_OnShadowOwnershipChanged(bool sunOwnsShadows)
        {
            if (_moonController != null)
                _moonController.SetShadowActive(!sunOwnsShadows);
        }

        #endregion

        #region Preset selection

        /// <summary>
        /// Fonction pure heure -> segment. Pas d'état, pas de fenêtres de transition : un simple
        /// enchaînement de seuils. Le lerp continu de Update() se charge de lisser visuellement le
        /// changement de cible quand l'heure fait basculer d'un segment à l'autre.
        /// </summary>
        private TimeOfDaySegment GetSegment(float time)
        {
            time %= 24f;
            if (time < 0f) time += 24f;

            if (time >= _nightStart || time < _morningStart) return TimeOfDaySegment.Night;
            if (time < _middayStart) return TimeOfDaySegment.Morning;
            if (time < _eveningStart) return TimeOfDaySegment.Midday;
            return TimeOfDaySegment.Evening;
        }

        private LightingProfile SelectPreset(WeatherType weather, TimeOfDaySegment segment, out bool found)
        {
            var set = SelectProfileSet(weather, out found);
            if (set == null) return null;

            var profile = set.Get(segment);
            if (profile == null)
            {
                Debug.LogWarning($"[LightingProfileManager] Le segment '{segment}' est vide pour la météo '{weather}' malgré IsComplete == true.");
                found = false;
            }
            return profile;
        }

        private WeatherProfileSet SelectProfileSet(WeatherType weather, out bool found)
        {
            found = _weatherPresets != null && _weatherPresets.TryGetValue(weather, out var set) && set != null && set.IsComplete;
            if (found) return _weatherPresets[weather];

            if (weather != _fallbackWeather
                && _weatherPresets != null
                && _weatherPresets.TryGetValue(_fallbackWeather, out var fallbackSet)
                && fallbackSet != null && fallbackSet.IsComplete)
            {
                found = true;
                return fallbackSet;
            }

            return null;
        }

        #endregion

        #region Lerp (écrit directement sur le moteur, aucune struct intermédiaire)

        private void LerpTowardPreset(LightingProfile p, float t)
        {
            // --- Sun ---
            if (_sunController != null && _sunController.SunLight != null)
            {
                var light = _sunController.SunLight;
                light.color = Color.Lerp(light.color, p.sunColor, t);
                light.intensity = Mathf.Lerp(light.intensity, p.sunIntensity, t);
                light.colorTemperature = Mathf.Lerp(light.colorTemperature, p.temperature, t);
            }
            if (_sunController != null && _sunController.SunLens != null)
            {
                _sunController.SunLens.intensity = Mathf.Lerp(_sunController.SunLens.intensity, p.FlareIntensity, t);
                _sunController.SunLens.scale = Mathf.Lerp(_sunController.SunLens.scale, p.FlareScale, t);
            }

            // --- Exposure ---
            if (_exposure != null)
            {
                _exposure.fixedExposure.value = Mathf.Lerp(_exposure.fixedExposure.value, p.Exposure + _additionalExposure, t);
                _exposure.compensation.value = Mathf.Lerp(_exposure.compensation.value, p.Compensation, t);
                _exposure.fixedExposure.overrideState = true;
                _exposure.compensation.overrideState = true;
            }

            // --- Fog ---
            if (_fog != null)
            {
                _fog.tint.value = Color.Lerp(_fog.tint.value, p.Tint, t);
                _fog.baseHeight.value = Mathf.Lerp(_fog.baseHeight.value, p.BaseHeight, t);
                _fog.maximumHeight.value = Mathf.Lerp(_fog.maximumHeight.value, p.MaximumHeight, t);
                _fog.meanFreePath.value = Mathf.Lerp(_fog.meanFreePath.value, p.FogAttenuationDistance, t);
                _fog.maxFogDistance.value = Mathf.Lerp(_fog.maxFogDistance.value, p.MaxFogDistance, t);
                _fog.albedo.value = Color.Lerp(_fog.albedo.value, p.Albedo, t);
                _fog.enableVolumetricFog.value = p.VolumetricFog; // booléen : pas de lerp qui tienne, on applique direct
                _fog.denoisingMode.value = p.DenoisingMode;       // idem, enum
                _fog.globalLightProbeDimmer.value = Mathf.Lerp(_fog.globalLightProbeDimmer.value, p.GIDimmer, t);

                _fog.tint.overrideState = true;
                _fog.baseHeight.overrideState = true;
                _fog.maximumHeight.overrideState = true;
                _fog.meanFreePath.overrideState = true;
                _fog.maxFogDistance.overrideState = true;
                _fog.albedo.overrideState = true;
                _fog.enableVolumetricFog.overrideState = true;
                _fog.denoisingMode.overrideState = true;
                _fog.globalLightProbeDimmer.overrideState = true;
            }

            // --- Sky ---
            if (_pbSky != null)
            {
                _pbSky.groundTint.value = Color.Lerp(_pbSky.groundTint.value, p.GroundTint, t);
                _pbSky.horizonTint.value = Color.Lerp(_pbSky.horizonTint.value, p.HorizonTint, t);
                _pbSky.zenithTint.value = Color.Lerp(_pbSky.zenithTint.value, p.ZenithTint, t);
                _pbSky.horizonZenithShift.value = Mathf.Lerp(_pbSky.horizonZenithShift.value, p.HorizonZenithShift, t);
                _pbSky.aerosolDensity.value = Mathf.Lerp(_pbSky.aerosolDensity.value, p.AerosolDensity, t);
                _pbSky.aerosolTint.value = Color.Lerp(_pbSky.aerosolTint.value, p.AerosolTint, t);
                _pbSky.aerosolMaximumAltitude.value = Mathf.Lerp(_pbSky.aerosolMaximumAltitude.value, p.AerosolMaximumAltitude, t);

                _pbSky.groundTint.overrideState = true;
                _pbSky.horizonTint.overrideState = true;
                _pbSky.zenithTint.overrideState = true;
                _pbSky.horizonZenithShift.overrideState = true;
                _pbSky.aerosolDensity.overrideState = true;
                _pbSky.aerosolTint.overrideState = true;
                _pbSky.aerosolMaximumAltitude.overrideState = true;
            }

            // --- Color Adjustments ---
            if (_colorAdjustments != null)
            {
                _colorAdjustments.postExposure.value = Mathf.Lerp(_colorAdjustments.postExposure.value, p.PostExposure, t);
                _colorAdjustments.contrast.value = Mathf.Lerp(_colorAdjustments.contrast.value, p.Contrasts, t);
                _colorAdjustments.saturation.value = Mathf.Lerp(_colorAdjustments.saturation.value, p.Saturation, t);

                _colorAdjustments.postExposure.overrideState = true;
                _colorAdjustments.contrast.overrideState = true;
                _colorAdjustments.saturation.overrideState = true;
            }
        }

        #endregion
    }
}