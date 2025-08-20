using LightHouse.Game.DayNightSystem;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

namespace LightHouse.Game.Rendering
{
    public class LightingProfileManager : MonoBehaviour, ITimeCycleObserver
    {
        #region References

        [Header("Controllers")]
        [SerializeField] private SunController _sunController;
        [SerializeField] private MoonController _moonController;
        [SerializeField] private Volume _globalVolume;

        #endregion

        #region Profiles

        [Header("Profiles")]
        [SerializeField] private LightingProfile _nightProfile;
        [SerializeField] private LightingProfile _morningProfile;
        [SerializeField] private LightingProfile _dayProfile;
        [SerializeField] private LightingProfile _eveningProfile;

        #endregion

        #region Sun

        [Header("Sun Control")]
        [SerializeField] private float _sunFadeStart = 19f;
        [SerializeField] private float _sunFadeEnd = 20f;
        [SerializeField] private Color _sunFadeColor = Color.black;
        [SerializeField] private float _sunFadeIntensityTarget = 0f;

        #endregion

        #region Volume Components & Privates

        private TimeManager _timeManager;

        private Fog _fog;
        private Exposure _exposure;
        private PhysicallyBasedSky _pbSky;
        private ColorAdjustments _colorAdjustments;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            _sunController.OnSunLightToggled += Sun_OnSunLightToggled;
            _timeManager = FindFirstObjectByType<TimeManager>();
            if (_timeManager != null)
            {
                _timeManager.RegisterObserver(this);
                _timeManager.RegisterObserver(_sunController);
                _timeManager.RegisterObserver(_moonController);
            }
        }

        private void Start()
        {
            _globalVolume.profile.TryGet(out _fog);
            _globalVolume.profile.TryGet(out _exposure);
            _globalVolume.profile.TryGet(out _pbSky);
            _globalVolume.profile.TryGet(out _colorAdjustments);
        }

        private void OnDestroy()
        {
            _sunController.OnSunLightToggled -= Sun_OnSunLightToggled;
            if (_timeManager != null)
            {
                _timeManager.UnregisterObserver(this);
                _timeManager.UnregisterObserver(_sunController);
                _timeManager.UnregisterObserver(_moonController);
            }
        }

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
            LightingProfile from = null;
            LightingProfile to = null;
            float t = 0f;

            if (timeOfDay >= 6f && timeOfDay < 9f)
            {
                from = _morningProfile;
                to = _dayProfile;
                t = (timeOfDay - 6f) / 3f;
            }
            else if (timeOfDay >= 9f && timeOfDay < 17f)
            {
                from = _dayProfile;
                to = _dayProfile;
                t = 0f;
            }
            else if (timeOfDay >= 17f && timeOfDay < 20f)
            {
                from = _dayProfile;
                to = _eveningProfile;
                t = (timeOfDay - 17f) / 3f;
            }
            else if (timeOfDay >= 20f && timeOfDay < 24f)
            {
                from = _eveningProfile;
                to = _nightProfile;
                t = (timeOfDay - 20f) / 4f;
            }
            else if (timeOfDay >= 0f && timeOfDay < 6f)
            {
                from = _nightProfile;
                to = _morningProfile;
                t = timeOfDay / 6;
            }

            bool useManualSunFade = timeOfDay >= _sunFadeStart && timeOfDay <= _sunFadeEnd;

            if (useManualSunFade)
            {
                float fadeT = Mathf.InverseLerp(_sunFadeStart, _sunFadeEnd, timeOfDay);
                ApplySunFadeOut(fadeT);
            }
            else
            {
                ApplyInterpolatedProfilesToSun(from, to, t);
            }

            ApplyInterpolatedProfile(from, to, t);
        }


        #endregion

        #region Interpolation Logic

        private void ApplyInterpolatedProfilesToSun(LightingProfile a, LightingProfile b, float t)
        {
            // --- Sun Light ---
            _sunController.SunLight.color = Color.Lerp(a.sunColor, b.sunColor, t);
            _sunController.SunLight.intensity = Mathf.Lerp(a.sunIntensity, b.sunIntensity, t);
            _sunController.SunLight.colorTemperature = Mathf.Lerp(a.temperature, b.temperature, t);
            _sunController.Lens.intensity = Mathf.Lerp(a.FlareIntensity, b.FlareIntensity, t);
            _sunController.Lens.scale = Mathf.Lerp(a.FlareScale, b.FlareScale, t);
        }

        private void ApplySunFadeOut(float t)
        {
            _sunController.SunLight.color = Color.Lerp(_sunController.SunLight.color, _sunFadeColor, t);
            _sunController.SunLight.intensity = Mathf.Lerp(_sunController.SunLight.intensity, _sunFadeIntensityTarget, t);
            _sunController.Lens.intensity = Mathf.Lerp(_sunController.Lens.intensity, 0f, t);
            _sunController.Lens.scale = Mathf.Lerp(_sunController.Lens.scale, 0f, t);
            _sunController.LightData.flareFalloff = Mathf.Lerp(_sunController.LightData.flareFalloff, 0f, t);
            _sunController.LightData.flareSize = Mathf.Lerp(_sunController.LightData.flareSize, 0f, t);
        }


        private void ApplyInterpolatedProfile(LightingProfile a, LightingProfile b, float t)
        {
            // --- Exposure ---
            if (_exposure != null)
            {
                _exposure.fixedExposure.value = Mathf.Lerp(a.Exposure, b.Exposure, t);
                _exposure.compensation.value = Mathf.Lerp(a.Compensation, b.Compensation, t);
            }

            // --- Fog ---
            if (_fog != null)
            {
                _fog.tint.value = Color.Lerp(a.Tint, b.Tint, t);
                _fog.baseHeight.value = Mathf.Lerp(a.BaseHeight, b.BaseHeight, t);
                _fog.maximumHeight.value = Mathf.Lerp(a.MaximumHeight, b.MaximumHeight, t);
                _fog.meanFreePath.value = Mathf.Lerp(a.FogAttenuationDistance, b.FogAttenuationDistance, t);
                _fog.maxFogDistance.value = Mathf.Lerp(a.MaxFogDistance, b.MaxFogDistance, t);
                _fog.albedo.value = Color.Lerp(a.Albedo, b.Albedo, t);
                _fog.enableVolumetricFog.value = b.VolumetricFog;
                _fog.denoisingMode.value = t < 0.5f ? a.DenoisingMode : b.DenoisingMode;
                _fog.globalLightProbeDimmer.value = Mathf.Lerp(a.GIDimmer, b.GIDimmer, t);
            }

            // --- Sky ---
            if (_pbSky != null)
            {
                _pbSky.groundTint.value = Color.Lerp(a.GroundTint, b.GroundTint, t);
                _pbSky.horizonTint.value = Color.Lerp(a.HorizonTint, b.HorizonTint, t);
                _pbSky.zenithTint.value = Color.Lerp(a.ZenithTint, b.ZenithTint, t);
                _pbSky.horizonZenithShift.value = Mathf.Lerp(a.HorizonZenithShift, b.HorizonZenithShift, t);
                _pbSky.aerosolDensity.value = Mathf.Lerp(a.AerosolDensity, b.AerosolDensity, t);
                _pbSky.aerosolTint.value = Color.Lerp(a.AerosolTint, b.AerosolTint, t);
                _pbSky.aerosolMaximumAltitude.value = Mathf.Lerp(a.AerosolMaximumAltitude, b.AerosolMaximumAltitude, t);
            }

            // --- Color Adjustments ---
            if (_colorAdjustments != null)
            {
                _colorAdjustments.postExposure.value = Mathf.Lerp(a.PostExposure, b.PostExposure, t);
                _colorAdjustments.contrast.value = Mathf.Lerp(a.Contrasts, b.Contrasts, t);
                _colorAdjustments.saturation.value = Mathf.Lerp(a.Saturation, b.Saturation, t);
            }
        }

        #endregion
    }
}
