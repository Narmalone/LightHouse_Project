using TMPro;
using UnityEngine;


public class ComputerTheme : EnumWrapper, IConfigurable
{
    [SerializeField] private EColors _color;
    [SerializeField] private EColors _defaultColor;
    [SerializeField] private EColors _appliedColor;

    private new void Start()
    {
        base.Start();
        _defaultColor = EColors.Red;
        _color = _defaultColor;
        _appliedColor = _color;
    }

    public override string[] GetNames() => System.Enum.GetNames(typeof(EColors));
    public override int GetCount() => System.Enum.GetValues(typeof(EColors)).Length;
    public override string GetName(int index) => GetNames()[index];
    public override void SetIndex(int index) => _color = (EColors)index;
    public override int GetIndex() => (int)_color;

    public bool HasChanged()
    {
        return _color != _defaultColor;
    }

    public void Apply()
    {
        if (HasChanged() && !HasBeenApplied())
        {
            Debug.Log("ComputerTheme apply");
            _appliedColor = _color;
        }
    }

    public void Reset()
    {
        if (HasChanged() && HasBeenApplied())
        {
            Debug.Log("ComputerTheme reset");
            _color = _defaultColor;
            _appliedColor = _defaultColor;
            SetDisplayText();
        }
    }

    public bool HasBeenApplied()
    {
        return _appliedColor == _color;
    }
}

