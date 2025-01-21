using FMODUnity;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;
using Random = UnityEngine.Random;

public class LightningsController : MonoBehaviour
{
    [SerializeField] private Volume _lightningVolume;
    [SerializeField] private float _lightningsTimeSpeedMultiplier = 1.0f;
    [SerializeField] private StudioEventEmitter _lightningAudioSource;
    [SerializeField] private AnimationCurve _lightningWeight;
    [SerializeField] private AnimationCurve _lightningCurveFadeIn;
    [SerializeField] private AnimationCurve _lightningCurveFadeOut;

    [SerializeField] private CustomEvent_WeatherType _onWeatherChanged;
    private bool _isEnabled = false;

    private CloudLayer _cloudComponent;

    public float timerTest = 3f;
    public float currentTimer = 0f;

    private void Awake()
    {
        _onWeatherChanged.handle += _onWeatherChanged_handle;

        _lightningVolume.sharedProfile.TryGet(out _cloudComponent);
    }

    private void OnDestroy()
    {
        _onWeatherChanged.handle -= _onWeatherChanged_handle;
    }

    private void Update()
    {
        if(!_isEnabled) return;

        currentTimer += _lightningsTimeSpeedMultiplier * Time.deltaTime * GameManager.GlobalSpeedTime;
        if(currentTimer >= timerTest)
        {
            timerTest = Random.Range(
                _lightningCurveFadeIn.keys[_lightningCurveFadeIn.keys.Length - 1].time +
                _lightningCurveFadeOut.keys[_lightningCurveFadeOut.keys.Length - 1].time, 7f);
            currentTimer = 0f;
            StartCoroutine(LightningRoutine(_lightningCurveFadeIn, _lightningCurveFadeOut));
        }
    }

    public void FadeAudio(float targetVolume, float targetDuration, Action onEnd = null)
    {
        StartCoroutine(FadeAudioCoroutine(targetVolume, targetDuration, onEnd));
    }

    private IEnumerator FadeAudioCoroutine(float targetVolume, float targetDuration, Action onEnd = null)
    {
        float initialVolume = 0f;
        _lightningAudioSource.EventInstance.getVolume(out initialVolume);
        float timer = 0f;

        while (timer < targetDuration)
        {
            timer += _lightningsTimeSpeedMultiplier * Time.deltaTime * GameManager.GlobalSpeedTime;
            _lightningAudioSource.EventInstance.setVolume(Mathf.Lerp(initialVolume, targetVolume, timer / targetDuration));
            _lightningAudioSource.EventInstance.getVolume(out float vol);
            yield return null;
        }

        _lightningAudioSource.EventInstance.setVolume(targetVolume);
        onEnd?.Invoke();
    }

    private void _onWeatherChanged_handle(WeatherType obj)
    {
        if (obj == WeatherType.Storm)
        {
            if (_isEnabled) return;
            StartCoroutine(WeightVolume(1f, _lightningWeight));
            StartLightning();
        }
        else
        {
            if (_isEnabled)
            {
                _lightningVolume.priority = 0f;
                StartCoroutine(WeightVolume(0f, _lightningWeight));

                _lightningAudioSource?.Stop();
                _isEnabled = false;
/*                FadeAudio(0f, 3f, () =>
                {
                    _lightningAudioSource?.Stop();
                });*/
                //set the volume to null
            }
        }
    }

    public void StartLightning()
    {
        _lightningVolume.priority = 100f;
        _lightningAudioSource?.Play();
        //FadeAudio(0.1f, 3f);
        StartCoroutine(LightningRoutine(_lightningCurveFadeIn, _lightningCurveFadeOut));
        _isEnabled = true;
    }

    //After reading the lightning fade in, read the fade out
    private IEnumerator LightningRoutine(AnimationCurve fadeIn, AnimationCurve fadeOut)
    {
        float startTime = Time.time;
        float fadeInDuration = fadeIn.keys[fadeIn.keys.Length - 1].time;
        float fadeOutDuration = fadeOut.keys[fadeOut.keys.Length - 1].time;

        // Fade in
        while (Time.time - startTime < fadeInDuration)
        {
            float t = (Time.time - startTime) / fadeInDuration;
            float exposure = fadeIn.Evaluate(t);
            _cloudComponent.layerA.exposure.Override(exposure);
            yield return null;
        }

        // Fade out
        startTime = Time.time;
        while (Time.time - startTime < fadeOutDuration)
        {
            float t = (Time.time - startTime) / fadeOutDuration;
            float exposure = fadeOut.Evaluate(t);
            _cloudComponent.layerA.exposure.Override(exposure);
            yield return null;
        }
    }

    IEnumerator WeightVolume(float targetWeight, AnimationCurve curve)
    {
        float startTime = Time.time;
        float duration = curve.keys[curve.keys.Length - 1].time;

        while (Time.time - startTime < duration)
        {
            float t = (Time.time - startTime) / duration;
            float weight = curve.Evaluate(t);
            _lightningVolume.weight = Mathf.Lerp(_lightningVolume.weight, targetWeight, weight);
            yield return null;
        }

        _lightningVolume.weight = targetWeight;
        _cloudComponent.active = true;
    }
}
