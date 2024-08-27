using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

public class NightLamp : MonoBehaviour
{
    [Header("Events")]
    [SerializeField] private CustomEvent _eventEvening;
    [SerializeField] private CustomEvent _eventMorning;

    [Header("Components")]
    [SerializeField] private Light _light;

    [Header("States")]
    [SerializeField] private bool _hasFase;
    [SerializeField] private bool _turnOffOnStart;

    [Header("Stats")]
    [SerializeField] private float _intensity;
    [SerializeField] private float _timeToFadeOn;
    [SerializeField] private AnimationCurve _curveFade;

    private float _delaySwitchOn;
    private float _delaySwitchOff;
    private Coroutine _coroutineFade;

    private void Awake()
    {
        _eventEvening.handle += OnLightOn;
        _eventMorning.handle += OnLightOff;
    }

    private void Start()
    {
        var ratio = GameManager.Instance.gameSettings.DayCycleDuration.Duration;
        _delaySwitchOn = 1.5f / 3600f * ratio;
        _delaySwitchOff = .8f / 3600f * ratio;

        if (!_turnOffOnStart) return;
        SwitchLight(0, 0);
    }

    private void OnDestroy()
    {
        _eventEvening.handle -= OnLightOn;
        _eventMorning.handle -= OnLightOff;
    }

    private void OnLightOff()
    {
        SwitchLight(0, _delaySwitchOff, _hasFase);
    }

    private void OnLightOn()
    {
        SwitchLight(_intensity, _delaySwitchOn, _hasFase);
    }

    private void SwitchLight(float intensity, float delay, bool fade = false)
    {
        if (fade)
        {
            if (_coroutineFade != null) StopCoroutine(_coroutineFade);
            _coroutineFade = StartCoroutine(FadeLight(intensity, delay));
            return;
        }

        _light.intensity = intensity;
    }

    IEnumerator FadeLight(float intensity, float delay)
    {
        if(delay != 0) yield return new WaitForSeconds(delay);

        float time = 0;
        float initialIntensity = _light.intensity;
        while (time < _timeToFadeOn)
        {
            time += Time.deltaTime;
            _light.intensity = Mathf.Lerp(initialIntensity, intensity, _curveFade.Evaluate(time/_timeToFadeOn));
            yield return null;
        }
        _light.intensity = intensity;
        _coroutineFade = null;
    }
}
