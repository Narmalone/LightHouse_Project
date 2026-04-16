using System;
using UnityEngine;

public class RaycastableMenuItem : MonoBehaviour
{
    [SerializeField] private Renderer[] _renderers;
    public event Action OnShowInformationsEvent;
    public event Action OnHideInformationsEvent;
    private MaterialPropertyBlock _mpb;
    private float _enabledValue = 0f;
    private bool _isSelected = false;

    private void Awake()
    {
        _mpb = new MaterialPropertyBlock();
    }

    private void Update()
    {
        // Smooth transition (optionnel mais fortement recommandé)
        float target = _isHovered ? 1f : 0f;
        _enabledValue = Mathf.Lerp(_enabledValue, target, Time.deltaTime * 8f);

        foreach (var renderer in _renderers)
        {
            renderer.GetPropertyBlock(_mpb);
            _mpb.SetFloat("_Enabled", _enabledValue);
            renderer.SetPropertyBlock(_mpb);
        }
    }

    private bool _isHovered = false;

    public void OnRaycastEnter()
    {
        Debug.Log($"Raycasted on {gameObject.name}");
        _isHovered = true;
    }

    public void OnRaycastExit()
    {
        Debug.Log($"Raycast exited on {gameObject.name}");
        _isHovered = false;
    }

    public void ShowInformations()
    {
        _isSelected = true;
        OnShowInformationsEvent?.Invoke();
        Debug.Log("Showing informations for " + gameObject.name);
    }

    public void HideInformations()
    {
        OnHideInformationsEvent?.Invoke();
        _isSelected = false;
        Debug.Log("Hiding informations for " + gameObject.name);
    }
}