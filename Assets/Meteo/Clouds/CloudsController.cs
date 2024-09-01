using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

public class CloudsController : MonoBehaviour
{
    //Si le prochain jour il y'a une tempete on met des éclairs au loin
    //puis quand y'a la tempete y'a les éclairs en mode VENER
    [SerializeField] private Volume _cloudsVolume;
    [SerializeField, Tooltip("Pour que les nuages bougent plus vite")] private float _cloudSpeedMultiplier = 3f;
    private VolumetricClouds _cachedClouds;
    private VisualEnvironment _cachedEnvironment;

    [SerializeField] private CustomEvent_WeatherType _onWeatherChanged;

    private WeatherManager _weatherManager;

    private WindSpeedParameter _currentWindSpeed;
    private WindParameter.WindParamaterValue _currentWindParameter;

    [SerializeField] private float _transitionCloudsDuration = 2.5f;

    private CloudSettings _oldCloudSettings;
    [SerializeField] private CloudSettings _calmSettings;
    [SerializeField] private CloudSettings _windySettings;
    [SerializeField] private CloudSettings _sunnySettings;
    [SerializeField] private CloudSettings _stormySettings;
    [SerializeField] private CloudSettings _rainySettings;

    public VisualEnvironment visualEnvironment;

    private void Awake()
    {
        _cloudsVolume.sharedProfile.TryGet(out _cachedClouds);
        _cloudsVolume.sharedProfile.TryGet(out _cachedEnvironment);

        _cloudsVolume.sharedProfile.TryGet(out visualEnvironment);

        _onWeatherChanged.handle += _onWeatherChanged_handle;

        _currentWindSpeed = new WindSpeedParameter(0f, WindParameter.WindOverrideMode.Custom);
        _currentWindParameter = _currentWindSpeed.value;
    }

    private void OnDestroy()
    {
        _onWeatherChanged.handle -= _onWeatherChanged_handle;
    }

    private void _onWeatherChanged_handle(WeatherType obj)
    {
        CloudSettings newCloudSettings = null;
        switch (obj)
        {
            case WeatherType.Calm:
                newCloudSettings = _calmSettings;
                break;
            case WeatherType.Storm:
                newCloudSettings = _stormySettings;
                break;
            case WeatherType.Windy:
                newCloudSettings = _windySettings;
                break;
            case WeatherType.Rainy:
                newCloudSettings = _rainySettings;
                break;
            case WeatherType.Sunny:
                newCloudSettings = _sunnySettings;
                break;
        }
        if(_oldCloudSettings == null)
        {
            _oldCloudSettings = _calmSettings;
        }

        LerpCloudSettings(_oldCloudSettings, newCloudSettings, _transitionCloudsDuration);
        _oldCloudSettings = newCloudSettings;
    }

    private void LerpCloudSettings(CloudSettings startSettings, CloudSettings endSettings, float duration)
    {
        _lerpTime = 0f;

        StartCoroutine(LerpCloudSettingsCoroutine(startSettings, endSettings, duration));
    }

    public static AnimationCurve LerpAnimationCurve(AnimationCurve startCurve, AnimationCurve endCurve, float t)
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

    private IEnumerator LerpCloudSettingsCoroutine(CloudSettings startSettings, CloudSettings endSettings, float duration)
    {
        while (_lerpTime < duration)
        {
            float t = _lerpTime / duration;
            t = t * t * (3f - 2f * t); // Smoothstep function

            // Interpoler les valeurs des paramètres de nuages
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

            _lerpTime += Time.deltaTime;
            yield return null;
        }
    }

    private void Start()
    {
        _weatherManager = WeatherManager.Instance;
    }

    private void Update()
    {
        _currentWindParameter.customValue = _weatherManager.WindSpeed * _cloudSpeedMultiplier;
        _currentWindSpeed.Override(_currentWindParameter);
        _cachedClouds.globalWindSpeed.SetValue(_currentWindSpeed);
    }

    private float _lerpTime = 0f;
}