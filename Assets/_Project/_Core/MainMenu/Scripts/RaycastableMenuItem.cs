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
    private bool _isHovered = false;

    public bool IsSelected => _isSelected;
    public bool IsHovered => _isHovered;

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


    public void OnRaycastEnter()
    {
        _isHovered = true;
    }

    public void OnRaycastExit()
    {
        _isHovered = false;
    }

    public void ShowInformations()
    {
        _isSelected = true;
        OnShowInformationsEvent?.Invoke();
    }

    public void HideInformations()
    {
        OnHideInformationsEvent?.Invoke();
        _isSelected = false;
    }
}