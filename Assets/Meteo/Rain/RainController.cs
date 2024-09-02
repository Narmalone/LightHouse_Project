using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;
using UnityEngine.VFX;

//Idées plus tard -> réalisme de la pluie, pluie qui touche le sol ou on voit les ploc + éventuellement
//elles glissent sur les matériaux cf https://www.youtube.com/watch?v=4C2PzDhkc0k
//pluie qui se déplace ? -> passer volume en local
public class RainController : MonoBehaviour
{
    #region CONST VARIABLES
    private const string P_INTENSITY = "Intensity";
    private const string P_MAXSPAWNRATE = "MaxSpawnRate";
    private const string P_MINSPAWNRATE = "MinSpawnRate";
    #endregion

    #region SERIALIZED
    [Header("CONTROLLERS REFERENCES")]
    [SerializeField] private VisualEffect _rainEffect;
    [SerializeField] private Volume _rainVolume;
    [SerializeField] private AudioSource _rainAudioSource;

    [Header("--- EVENTS ---")]
    [Header("LISTENERS")]
    [SerializeField] private CustomEvent_WeatherType _onWeatherChanged;

    [Header("RAIN SETTINGS")]
    [SerializeField] private RainSettings _rainySettings;
    [SerializeField] private RainSettings _stormySettings;

    [Header("DEBUG ONLY")]
    public bool IsRaining = false;

    #endregion

    #region CACHE & PROPERTIES

    //cached component
    private RainSettings _currentRainingSettings;
    public RainSettings CurrentRainingSettings => _currentRainingSettings;

    private Fog _cachedFog;

    #endregion

    #region MONO CALLBACKS

    private void Awake()
    {
        _rainVolume.sharedProfile.TryGet(out _cachedFog);
        _rainEffect.Stop();
        _onWeatherChanged.handle += _onWeatherChanged_handle;
    }

    private void OnDestroy()
    {
        _onWeatherChanged.handle -= _onWeatherChanged_handle;
    }

    #endregion

    #region DELEGATES

    private void _onWeatherChanged_handle(WeatherType obj)
    {
        if (obj == WeatherType.Storm)
        {
            _currentRainingSettings = _stormySettings;

            if (!IsRaining)
            {
                _rainEffect.Play();
                _rainAudioSource?.Play();
                StartCoroutine(WeightVolume(1f, _currentRainingSettings.VolumeWeightCurve));
            }
            IsRaining = true;

        }
        else if (obj == WeatherType.Rainy)
        {
            _currentRainingSettings = _rainySettings;

            if (!IsRaining)
            {
                _rainEffect.Play();
                _rainAudioSource?.Play();
                StartCoroutine(WeightVolume(1f, _currentRainingSettings.VolumeWeightCurve));
            }
            IsRaining = true;
        }
        else
        {
            if (IsRaining)
            {
                IsRaining = false;
                _rainEffect.Stop();
                _rainAudioSource?.Stop(); //faire en sorte de pouvoir changer en fonction de storm ou autre
                StartCoroutine(WeightVolume(0f, _currentRainingSettings.VolumeWeightCurve));
            }
            return;
        }

        _rainEffect.SetFloat(P_INTENSITY, _currentRainingSettings.Intensity);
        _rainEffect.SetInt(P_MAXSPAWNRATE, (int)_currentRainingSettings.MaxRainSpawnRate);
        _rainEffect.SetInt(P_MINSPAWNRATE, (int)_currentRainingSettings.MinRainSpawnRate);
        _cachedFog.meanFreePath.value = _currentRainingSettings.FogAttenuationDistance;
    }

    #endregion

    #region COROUTINES

    IEnumerator WeightVolume(float targetWeight, AnimationCurve curve)
    {
        float startTime = Time.time;
        float duration = curve.keys[curve.keys.Length - 1].time;

        while (Time.time - startTime < duration)
        {
            float t = (Time.time - startTime) / duration;
            float weight = curve.Evaluate(t);
            _rainVolume.weight = Mathf.Lerp(_rainVolume.weight, targetWeight, weight);
            yield return null;
        }

        _rainVolume.weight = targetWeight;
    }
    #endregion
}
