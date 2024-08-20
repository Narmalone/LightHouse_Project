using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    [SerializeField] private float _delaySwitchOn;
    [SerializeField] private float _timeToFadeOn;
    [SerializeField] private AnimationCurve _curveFade;

    private Coroutine _coroutineFade;

    private void Awake()
    {
        _eventEvening.handle += OnLightOn;
        _eventMorning.handle += OnLightOff;
    }

    private void Start()
    {
        if (!_turnOffOnStart) return;
        SwitchLight(0);
    }

    private void OnDestroy()
    {
        _eventEvening.handle -= OnLightOn;
        _eventMorning.handle -= OnLightOff;
    }

    private void OnLightOff()
    {
        SwitchLight(0, _hasFase);
    }

    private void OnLightOn()
    {
        SwitchLight(_intensity, _hasFase);
    }

    private void SwitchLight(float intensity, bool fade = false)
    {
        if (fade)
        {
            if (_coroutineFade != null) StopCoroutine(_coroutineFade);
            _coroutineFade = StartCoroutine(FadeLight(intensity));
            return;
        }
        _light.intensity = intensity;
    }

    IEnumerator FadeLight(float intensity)
    {
        if(intensity > 0 && _delaySwitchOn != 0) yield return new WaitForSeconds(_delaySwitchOn);

        float time = 0;
        float initialIntensity = _light.intensity;
        while (time < _timeToFadeOn)
        {
            time += Time.deltaTime;
            _light.intensity = Mathf.Lerp(initialIntensity, intensity, _curveFade.Evaluate(time/_timeToFadeOn));
            yield return null;
        }
        _light.intensity = intensity;
    }
}
