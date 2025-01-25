using DG.Tweening;
using UnityEngine;

public class LightHandlerLightHouse : MonoBehaviour
{
    [Header("Event")]
    [SerializeField] private CustomEvent _eventLightOn;
    [SerializeField] private CustomEvent _eventLightOff;
    [SerializeField] private CustomEvent _eventRotationOn;
    [SerializeField] private CustomEvent _eventRotationOff;

    [Header("Component")]
    [SerializeField] private Transform _parentLightTransform;
    [SerializeField] private Light[] _spotLight;

    [Header("Stats")]
    [SerializeField] private AnimationCurve _accelerationRotation;
    [SerializeField] private AnimationCurve _decelerationRotation;
    [SerializeField] private float _durationLoopLight;
    [SerializeField] private float _fadeLightDuration;

    private float _maxIntensity;
    private Tween _tweenRotation;

    private void Awake()
    {
        _eventLightOn.handle += OnActiveLight;
        _eventLightOff.handle += OnDesactiveLight;
        _eventRotationOn.handle += OnActiveRotation;
        _eventRotationOff.handle += OnDesactiveRotation;
    }

    private void Start()
    {
        _maxIntensity = _spotLight[0].intensity; 

        foreach (Light light in _spotLight)
        {
            light.DOIntensity(0, 0);
        }
    }

    private void OnDestroy()
    {
        _eventLightOn.handle -= OnActiveLight;
        _eventLightOff.handle -= OnDesactiveLight;
        _eventRotationOn.handle -= OnActiveRotation;
        _eventRotationOff.handle -= OnDesactiveRotation;
    }

    // Rotation
    private void OnActiveRotation()
    {
        ActiveDesactiveRotation(true);
    }

    private void OnDesactiveRotation()
    {
        ActiveDesactiveRotation(false);
    }

    // Light
    private void OnActiveLight()
    {
        ActiveDesactiveLight(true);
    }

    private void OnDesactiveLight()
    {
        ActiveDesactiveLight(false);
    }

    // Handler
    private void ActiveDesactiveLight(bool active)
    {
        var targetIntensity = active ? _maxIntensity : 0;
        foreach (Light light in _spotLight)
        {
            light.DOIntensity(targetIntensity, _fadeLightDuration);
        }
    }

    private void ActiveDesactiveRotation(bool active)
    {
        if (active)
        {
            Debug.Log("Active with deceleration");
            _parentLightTransform.DORotate(Vector3.up * 360, _durationLoopLight, RotateMode.LocalAxisAdd).SetEase(_accelerationRotation).OnComplete(()=>
            {
                Debug.Log("Active infinite");
                _tweenRotation = _parentLightTransform.DORotate(Vector3.up * 360, _durationLoopLight, RotateMode.LocalAxisAdd).SetLoops(-1, LoopType.Incremental);
            });
        }
        else
        {
            if(_tweenRotation != null)
            {
                Debug.Log("Stop with deceleration");
                _parentLightTransform.DORotate(Vector3.up * 360, _durationLoopLight, RotateMode.LocalAxisAdd).SetEase(_decelerationRotation).OnComplete(()=>
                {
                    Debug.Log("Kill");
                    _tweenRotation.Kill();
                    _tweenRotation = null;
                });
            }
        }
    }
}