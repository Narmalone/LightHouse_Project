using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

public class CloudsController : MonoBehaviour
{
    [SerializeField] private Volume _cloudsVolume;
    [SerializeField] private float _cloudSpeedMultiplier = 3f;
    private VolumetricClouds _cachedClouds;
    private VisualEnvironment _cachedEnvironment;

    [SerializeField] private CustomEvent_WeatherType _onWeatherChanged;
    [SerializeField] private CustomEvent_WeatherType _onWeatherOverrideStart;

    private WeatherManager _weatherManager;

    private WindSpeedParameter _currentWindSpeed;
    private WindParameter.WindParamaterValue _currentWindParameter;

    [SerializeField] private float _transitionCloudsDuration = 25f;

    private CloudSettings _oldCloudSettings;
    private CloudSettings _targetCloudSettings;
    private float _transitionElapsedTime;

    [SerializeField] private CloudSettings _calmSettings;
    [SerializeField] private CloudSettings _windySettings;
    [SerializeField] private CloudSettings _sunnySettings;
    [SerializeField] private CloudSettings _stormySettings;
    [SerializeField] private CloudSettings _rainySettings;

    private void Awake()
    {
        _cloudsVolume.sharedProfile.TryGet(out _cachedClouds);
        _cloudsVolume.sharedProfile.TryGet(out _cachedEnvironment);

        _onWeatherChanged.handle += _onWeatherChanged_handle;
        _onWeatherOverrideStart.handle += _onWeatherOverrideStart_handle;

        _currentWindSpeed = new WindSpeedParameter(0f, WindParameter.WindOverrideMode.Custom);
        _currentWindParameter = _currentWindSpeed.value;
    }

    private void _onWeatherOverrideStart_handle(WeatherType obj)
    {
        // Sélectionner les paramètres de nuages en fonction du type de météo
        CloudSettings overrideSettings = GetCloudSettingsFromWeatherType(obj);

        // Appliquer instantanément les nouveaux paramètres de nuages sans transition (lerp)
        ApplyCloudSettingsDirectly(overrideSettings);

        // Ensuite, nous définissons les anciens paramètres pour la transition suivante
        _oldCloudSettings = overrideSettings;
        _transitionElapsedTime = 0f; // Réinitialiser le timer pour la prochaine transition
    }

    private void ApplyCloudSettingsDirectly(CloudSettings settings)
    {
        _cachedClouds.densityMultiplier.Override(settings.DensityMultiplier);
        _cachedClouds.densityCurve.Override(settings.DensityCurve);

        _cachedClouds.shapeFactor.Override(settings.ShapeFactor);
        _cachedClouds.shapeScale.Override(settings.ShapeScale);

        _cachedClouds.erosionFactor.Override(settings.ErosionFactor);
        _cachedClouds.erosionScale.Override(settings.ErosionScale);
        _cachedClouds.erosionCurve.Override(settings.ErosionCurve);
        _cachedClouds.ambientOcclusionCurve.Override(settings.AmbientOcclusionCurve);

        _cachedClouds.bottomAltitude.Override(settings.BottomAltitude);
        _cachedClouds.altitudeRange.Override(settings.AltitudeRange);

        _cachedClouds.shapeOffset.Override(settings.ShapeOffset);
        _cachedClouds.earthCurvature.Override(settings.EarthCurvature);
    }

    private void OnDestroy()
    {
        _onWeatherChanged.handle -= _onWeatherChanged_handle;
        _onWeatherOverrideStart.handle -= _onWeatherOverrideStart_handle;
    }

    private void _onWeatherChanged_handle(WeatherType obj)
    {
        _targetCloudSettings = GetCloudSettingsFromWeatherType(obj);

        if (_oldCloudSettings == null)
        {
            _oldCloudSettings = _calmSettings;
        }

        _transitionElapsedTime = 0f; // Reset transition timer when weather changes
    }

    private CloudSettings GetCloudSettingsFromWeatherType(WeatherType weatherType)
    {
        switch (weatherType)
        {
            case WeatherType.Calm:
                return _calmSettings;
            case WeatherType.Storm:
                return _stormySettings;
            case WeatherType.Windy:
                return _windySettings;
            case WeatherType.Rainy:
                return _rainySettings;
            case WeatherType.Sunny:
                return _sunnySettings;
            default:
                return _calmSettings;
        }
    }

    private void LerpCloudSettings(CloudSettings startSettings, CloudSettings endSettings, float t)
    {
        // Interpolation des valeurs des paramètres de nuages
        float densityMultiplier = Mathf.Lerp(startSettings.DensityMultiplier, endSettings.DensityMultiplier, t);
        AnimationCurve densityCurve = LerpAnimationCurve(startSettings.DensityCurve, endSettings.DensityCurve, t);

        float shapeFactor = Mathf.Lerp(startSettings.ShapeFactor, endSettings.ShapeFactor, t);
        float shapeScale = Mathf.Lerp(startSettings.ShapeScale, endSettings.ShapeScale, t);
        float erosionFactor = Mathf.Lerp(startSettings.ErosionFactor, endSettings.ErosionFactor, t);
        float erosionScale = Mathf.Lerp(startSettings.ErosionScale, endSettings.ErosionScale, t);
        AnimationCurve erosionCurve = LerpAnimationCurve(startSettings.ErosionCurve, endSettings.ErosionCurve, t);
        AnimationCurve ambientOcclusionCurve = LerpAnimationCurve(startSettings.AmbientOcclusionCurve, endSettings.AmbientOcclusionCurve, t);

        float bottomAltitude = Mathf.Lerp(startSettings.BottomAltitude, endSettings.BottomAltitude, t);
        float altitudeRange = Mathf.Lerp(startSettings.AltitudeRange, endSettings.AltitudeRange, t);

        Vector3 shapeOffset = Vector3.Lerp(startSettings.ShapeOffset, endSettings.ShapeOffset, t);
        float earthCurvature = Mathf.Lerp(startSettings.EarthCurvature, endSettings.EarthCurvature, t);

        // Appliquer les valeurs interpolées aux paramètres de nuages
        _cachedClouds.densityMultiplier.Override(densityMultiplier);
        _cachedClouds.densityCurve.Override(densityCurve);

        _cachedClouds.shapeFactor.Override(shapeFactor);
        _cachedClouds.shapeScale.Override(shapeScale);

        _cachedClouds.erosionFactor.Override(erosionFactor);
        _cachedClouds.erosionScale.Override(erosionScale);
        _cachedClouds.erosionCurve.Override(erosionCurve);
        _cachedClouds.ambientOcclusionCurve.Override(ambientOcclusionCurve);

        _cachedClouds.bottomAltitude.Override(bottomAltitude);
        _cachedClouds.altitudeRange.Override(altitudeRange);

        _cachedClouds.shapeOffset.Override(shapeOffset);
        _cachedClouds.earthCurvature.Override(earthCurvature);
    }

    private AnimationCurve LerpAnimationCurve(AnimationCurve startCurve, AnimationCurve endCurve, float t)
    {
        AnimationCurve resultCurve = new AnimationCurve();

        int numKeys = Mathf.Max(startCurve.keys.Length, endCurve.keys.Length);
        Keyframe[] keys = new Keyframe[numKeys];

        for (int i = 0; i < numKeys; i++)
        {
            float time = Mathf.Lerp(startCurve.keys[i % startCurve.keys.Length].time, endCurve.keys[i % endCurve.keys.Length].time, t);
            float value = Mathf.Lerp(startCurve.keys[i % startCurve.keys.Length].value, endCurve.keys[i % endCurve.keys.Length].value, t);
            float inTangent = Mathf.Lerp(startCurve.keys[i % startCurve.keys.Length].inTangent, endCurve.keys[i % endCurve.keys.Length].inTangent, t);
            float outTangent = Mathf.Lerp(startCurve.keys[i % startCurve.keys.Length].outTangent, endCurve.keys[i % endCurve.keys.Length].outTangent, t);

            keys[i] = new Keyframe(time, value, inTangent, outTangent);
        }

        resultCurve.keys = keys;
        return resultCurve;
    }

    private void Start()
    {
        _weatherManager = WeatherManager.Instance;
    }

    private void Update()
    {
        // Lerp constant vers la météo de demain
        float weatherDuration = _transitionCloudsDuration;
        if (_weatherManager != null)
        {
            WeatherType tomorrowWeatherType = _weatherManager.nextWeather.weatherType;
            weatherDuration = _weatherManager.currentWeather.weatherInitialDuration;
            _targetCloudSettings = GetCloudSettingsFromWeatherType(tomorrowWeatherType);
        }

        if (_targetCloudSettings != null && _oldCloudSettings != null)
        {
            _transitionElapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(_transitionElapsedTime / weatherDuration);
            t = t * t * (3f - 2f * t); // Smoothstep

            // Interpoler les paramètres de nuages directement dans l'Update
            LerpCloudSettings(_oldCloudSettings, _targetCloudSettings, t);

            // Si la transition est terminée, actualiser les anciens paramètres
            if (t >= 1f)
            {
                _oldCloudSettings = _targetCloudSettings;
            }
        }

        // Mise à jour du vent en fonction de la météo
        _currentWindParameter.customValue = _weatherManager.WindSpeed * _cloudSpeedMultiplier;
        _currentWindSpeed.Override(_currentWindParameter);
        _cachedClouds.globalWindSpeed.SetValue(_currentWindSpeed);
    }
}
