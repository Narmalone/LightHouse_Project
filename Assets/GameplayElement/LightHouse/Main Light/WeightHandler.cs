using UnityEngine;

public class WeightHandler : MonoBehaviour
{
    [SerializeField] private CustomEvent _eventOnReloadingWeight;
    [SerializeField] private CustomEvent _eventRotationOn;
    [SerializeField] private LineRenderer _lineRenderer;
    [SerializeField] private Transform _weightTransform;

    private void Awake()
    {
        _eventRotationOn.handle += OnRotation;
        _eventOnReloadingWeight.handle += OnUpdatePosition;
    }

    private void OnDestroy()
    {
        _eventRotationOn.handle -= OnRotation;
        _eventOnReloadingWeight.handle -= OnUpdatePosition;
    }

    private void OnUpdatePosition()
    {
        // Faire Descendre le poids et update les position des points du linerenderer
    }

    private void OnRotation()
    {
        // Faire remonter le poids et update les position des points du linerenderer
    }
}
