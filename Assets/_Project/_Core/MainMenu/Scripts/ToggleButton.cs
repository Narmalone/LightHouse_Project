using System;
using UnityEngine;

public class ToggleButton : MonoBehaviour
{
    public event Action<bool> IsOnChanged;
    [SerializeField] private Renderer targetRenderer;
    [SerializeField] private Color _enabledColor = Color.green;
    [SerializeField] private Color _disabledColor = Color.red;
    [SerializeField] private float _enabledIntensity = 3f;
    [SerializeField] private float _disabledIntensity = 3f;
    private bool _isOn;
    private InteractableBase _interactable;

    private MaterialPropertyBlock _mpb;
    private const string EmissiveColorID = "_EmissiveColor";

    private void OnDestroy()
    {
        if (_interactable != null)
            _interactable.OnClickDown -= Interactable_OnClickDown;
    }

    private void Awake()
    {
        _mpb = new MaterialPropertyBlock();
        if (_isOn)
            Apply(_enabledColor, _enabledIntensity);
        else Apply(_disabledColor, _disabledIntensity);
    }

    public void Bind(InteractableBase interactable)
    {
        _interactable = interactable;
        interactable.OnClickDown += Interactable_OnClickDown;
    }

    private void Interactable_OnClickDown()
    {
        _isOn = !_isOn;
        IsOnChanged?.Invoke(_isOn);
        if (_isOn)
            Apply(_enabledColor, _enabledIntensity);
        else
            Apply(_disabledColor, _disabledIntensity);
    }

    public void Apply(Color color, float intensity)
    {
        targetRenderer.GetPropertyBlock(_mpb);
        _mpb.SetColor(EmissiveColorID, color * intensity);
        targetRenderer.SetPropertyBlock(_mpb);
    }
}