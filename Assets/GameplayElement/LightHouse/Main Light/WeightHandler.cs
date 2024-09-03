using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

public class WeightHandler : MonoBehaviour
{
    [SerializeField] private CustomEvent_Float _eventOnReloadingWeight;
    [SerializeField] private CustomEvent _eventRotationOn;
    [SerializeField] private LineRenderer _lineRenderer;
    [SerializeField] private Transform _weightTransform;
    [SerializeField] private Transform _targetWeightPosition;

    private Vector3 _initialWeightPosition;
    private Vector3 _maxWeightPosition;

    private Coroutine _coroutineElevator;

    private float _timeSpeed;

    private void Awake()
    {
        _eventRotationOn.handle += OnRotation;
        _eventOnReloadingWeight.handle += OnUpdatePosition;
    }

    private void Start()
    {
        _timeSpeed = GameManager.Instance.gameSettings.DayCycleDuration.Duration;

        _initialWeightPosition = _weightTransform.position;
        _maxWeightPosition = _targetWeightPosition.position;
        _lineRenderer.SetPosition(0, _initialWeightPosition);
        UpdateLineRenderer();
    }

    private void OnDestroy()
    {
        _eventRotationOn.handle -= OnRotation;
        _eventOnReloadingWeight.handle -= OnUpdatePosition;
    }

    private void OnUpdatePosition(float time)
    {
        // Faire Descendre le poids et update les position des points du linerenderer
        _weightTransform.position = Vector3.Lerp(_initialWeightPosition, _maxWeightPosition, time);
        UpdateLineRenderer();
    }

    private void OnRotation()
    {
        // Faire remonter le poids et update les position des points du linerenderer
        StartElevator();
        UpdateLineRenderer();
    }

    private void StartElevator()
    {
        if (_coroutineElevator != null) StopCoroutine(_coroutineElevator);
        _coroutineElevator = StartCoroutine(Elevator_Coroutine());
    }

    private void UpdatePositions(float time)
    {
        _weightTransform.position = Vector3.Lerp(_maxWeightPosition, _initialWeightPosition, time);
        UpdateLineRenderer();
    }

    private void UpdateLineRenderer()
    {
        _lineRenderer.SetPosition(1, _weightTransform.position);
    }

    IEnumerator Elevator_Coroutine()
    {
        float time = 0;
        while (time < 12)
        {
            time += Time.deltaTime / 3600f * _timeSpeed;
            Debug.Log(time / 12);
            UpdatePositions(time/ 12);
            yield return null;
        }
    }
}
