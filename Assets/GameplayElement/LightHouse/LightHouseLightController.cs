using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightHouseLightController : MonoBehaviour
{
    [SerializeField] private CustomEvent _onLightEnable;
    [SerializeField] private CustomEvent _onLightDisable;
    [SerializeField] private CustomEvent _onRotationStart;
    [SerializeField] private CustomEvent _onRotationStop;

    [SerializeField] private float _fullyTurnInSeconds = 20f;
    [SerializeField] private AnimationCurve _accelerationRotation;

    [SerializeField] private Light[] _mainLight;

    private TweenerCore<Quaternion, Vector3, QuaternionOptions> _currentRotateTransform;

    private void Awake()
    {
        _onLightEnable.handle += OnLightEnabled;
        _onLightDisable.handle += OnLightDisabled;

        _onRotationStart.handle += OnRotationStart;
        _onRotationStop.handle += OnRotationStop;
    }

    private void OnDestroy()
    {
        _onLightEnable.handle -= OnLightEnabled;
        _onLightDisable.handle -= OnLightDisabled;
        _onRotationStart.handle -= OnRotationStart;
        _onRotationStop.handle -= OnRotationStop;
    }

    private void OnRotationStop()
    {
        _currentRotateTransform?.Kill();
    }

    private void OnRotationStart()
    {
        _currentRotateTransform = transform.DORotate(Vector3.up * 360, _fullyTurnInSeconds, RotateMode.LocalAxisAdd).SetLoops(-1).SetEase(_accelerationRotation);
    }

    private void OnLightDisabled()
    {
        foreach (var l in _mainLight)
        {
            l.enabled = false;
        }
    }

    private void OnLightEnabled()
    {
        foreach (var l in _mainLight) 
        {
            l.enabled = true;
        }
    }
}
