using System;
using UnityEngine;

public class OptionToggle : MonoBehaviour
{
    public bool isOn = false;
    [SerializeField] private ShortcutButton EnableButton;
    [SerializeField] private ShortcutButton DisableButton;

    private ShortcutButton _currentSelected;

    public event Action<bool> OnValueChanged;

    private void Awake()
    {
        EnableButton.OnClick += OnEnableCliqued;
        DisableButton.OnClick += OnDisableCliqued;
    }

    private void OnDisableCliqued(ShortcutButton btn)
    {
        isOn = false;
        OnValueChanged?.Invoke(false);
        SwitchSelected(btn);
        
    }

    private void OnEnableCliqued(ShortcutButton btn)
    {
        isOn = true;
        SwitchSelected(btn);
        OnValueChanged?.Invoke(true);
    }

    public void SwitchSelected(ShortcutButton target)
    {
        if (_currentSelected != null)
            _currentSelected.Unselect();
        if (target == null) return;
        _currentSelected = target;
        _currentSelected.Select();
    }

    public void SwitchSelected(bool value)
    {
        if (_currentSelected != null)
            _currentSelected.Unselect();
        
        _currentSelected = value ? EnableButton : DisableButton;
        isOn = value;
        _currentSelected.Select();
    }

    public void DisableAll()
    {
        EnableButton.Disable();
        DisableButton.Disable();
    }

    public void EnableAll()
    {
        EnableButton.Enable();
        DisableButton.Enable();
    }

    public void SetValueWithoutNotify(bool value)
    {
        isOn = value;
    }

    public void SetValue(bool value)
    {
        isOn = value;
    }

    private void OnDestroy()
    {
        EnableButton.OnClick -= OnEnableCliqued;
        DisableButton.OnClick -= OnDisableCliqued;
    }
}
