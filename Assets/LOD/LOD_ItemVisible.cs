using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class LOD_ItemVisible : MonoBehaviour
{
    [SerializeField] private Renderer _renderer;
    [SerializeField] private Behaviour[] AdditionnalComponents;
    private Camera _mainCamera;
    private bool _isVisible = true;

    private void Awake()
    {
        _mainCamera = Camera.main;
    }

    void OnBecameInvisible()
    {
        if (_renderer.enabled)
        {
            _renderer.enabled = false;
            _isVisible = false;
        }
        if (AdditionnalComponents.Length == 0) return;
        foreach (var component in AdditionnalComponents)
        {
            component.enabled = false;
        }
    }
    void Update()
    {
        if (!_isVisible)
        {
            if (IsVisibleFromCamera())
            {
                _renderer.enabled = true;
                _isVisible = true;
            }
        }
    }
    bool IsVisibleFromCamera()
    {
        if (_mainCamera == null)
            return false;

        Plane[] planes = GeometryUtility.CalculateFrustumPlanes(_mainCamera);
        return GeometryUtility.TestPlanesAABB(planes, _renderer.bounds);
    }
    void OnBecameVisible()
    {
        if (AdditionnalComponents.Length == 0) return;
        foreach (var component in AdditionnalComponents)
        {
            component.enabled = true;
        }
    }

    private void OnValidate()
    {
        _renderer = GetComponent<Renderer>();
    }
}
