using TMPro;
using UnityEngine;


public class ComputerTheme : EnumWrapper, IConfigurable
{
    private EColors _color;
    private EColors _defaultColor;

    private new void Start()
    {
        base.Start();
        _defaultColor = EColors.Blue;
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
        if (HasChanged())
        {
            Debug.Log("Sizes apply");
        }
    }

    public void Reset()
    {
        if (HasChanged())
        {
            Debug.Log("Sizes reset");
            _color = _defaultColor;
            SetDisplayText();
        }
    }
}

