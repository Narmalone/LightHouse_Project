using TMPro;
using UnityEngine;


public class MouseCursorSize : EnumWrapper, IConfigurable
{
    private ESizes _size;
    private ESizes _defaultSize;
    private ESizes _appliedSize;

    private new void Start()
    {
        base.Start();
        _defaultSize = ESizes.Small;
        _appliedSize = _size;
    }

    public override string[] GetNames() => System.Enum.GetNames(typeof(ESizes));
    public override int GetCount() => System.Enum.GetValues(typeof(ESizes)).Length;
    public override string GetName(int index) => GetNames()[index];
    public override void SetIndex(int index) => _size = (ESizes)index;
    public override int GetIndex() => (int)_size;

    public bool HasChanged()
    {
        return _size != _defaultSize;
    }

    public void Apply()
    {
        if (HasChanged())
        {
            Debug.Log("Sizes apply");
            _appliedSize = _size;
        }
    }

    public void Reset()
    {
        if (HasChanged())
        {
            Debug.Log("Sizes reset");
            _size = _defaultSize;
            _appliedSize = _defaultSize;
            SetDisplayText();
        }
    }

    bool IConfigurable.HasBeenApplied()
    {
        return _appliedSize == _size; ;
    }
}

