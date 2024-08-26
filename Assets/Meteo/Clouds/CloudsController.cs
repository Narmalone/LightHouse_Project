using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

public class CloudsController : MonoBehaviour
{
    //faire spawn une tempête trop stylé
    [SerializeField] private Volume _cloudsVolume;
    [SerializeField, Tooltip("Pour que les nuages bougent plus vite")] private float _cloudSpeedMultiplier = 3f;
    private VolumetricClouds _cachedClouds;
    private VisualEnvironment _cachedEnvironment;

    [SerializeField] private CustomEvent_WeatherType _onWeatherChanged;

    private WeatherManager _weatherManager;

    private WindSpeedParameter _currentWindSpeed;
    private WindParameter.WindParamaterValue _currentWindParameter;

    private void Awake()
    {
        _cloudsVolume.sharedProfile.TryGet(out _cachedClouds);
        _cloudsVolume.sharedProfile.TryGet(out _cachedEnvironment);

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
        switch (obj)
        {
            case WeatherType.Calm:
                _cachedClouds.cloudPreset = VolumetricClouds.CloudPresets.Sparse;
                break;
            case WeatherType.Storm:
                _cachedClouds.cloudPreset = VolumetricClouds.CloudPresets.Overcast;
                break;
            case WeatherType.Windy:
                _cachedClouds.cloudPreset = VolumetricClouds.CloudPresets.Cloudy;
                break;
            case WeatherType.Rainy:
                _cachedClouds.cloudPreset = VolumetricClouds.CloudPresets.Stormy;
                break;
            case WeatherType.Sunny:
                _cachedClouds.enable.value = false;
                break;
        }
        if (_cachedClouds.enable.value == false && obj != WeatherType.Sunny)
        {
            _cachedClouds.enable.value = true;
        }
    }

    private void Start()
    {
        _weatherManager = WeatherManager.Instance;
    }

    private void Update()
    {
        _currentWindParameter.customValue = _weatherManager.windSpeed * _cloudSpeedMultiplier;
        _currentWindSpeed.Override(_currentWindParameter);
        _cachedClouds.globalWindSpeed.SetValue(_currentWindSpeed);
    }

    private void LerpCloudPreset(VolumetricClouds.CloudPresets startPreset, VolumetricClouds.CloudPresets endPreset, float duration)
    {
        _lerpTime = 0f;
        _startPreset = startPreset;
        _endPreset = endPreset;

        StartCoroutine(LerpCloudPresetCoroutine(duration));
    }

    private float _lerpTime = 0f;
    private VolumetricClouds.CloudPresets _startPreset;
    private VolumetricClouds.CloudPresets _endPreset;

    private IEnumerator LerpCloudPresetCoroutine(float duration)
    {
        while (_lerpTime < duration)
        {
            float t = _lerpTime / duration;
            t = t * t * (3f - 2f * t); // Smoothstep function

            VolumetricClouds.CloudPresets currentPreset = _startPreset;
            if (t > 0.5f)
            {
                currentPreset = _endPreset;
            }
            else
            {
                currentPreset = (VolumetricClouds.CloudPresets)Mathf.Lerp((int)_startPreset, (int)_endPreset, t * 2f);
            }

            _cachedClouds.cloudPreset = currentPreset;

            _lerpTime += Time.deltaTime;
            yield return null;
        }
    }

}
