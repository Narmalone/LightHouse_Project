using UnityEditor.Search;
using UnityEngine;

public class DisplayMode : EnumWrapper, IConfigurable
{
    [SerializeField] private EDisplayMode _displayMode;
    [SerializeField] private EDisplayMode _defaultDisplayMode;

    private new void Start()
    {
        base.Start();
        _defaultDisplayMode = EDisplayMode.Windowed;
    }

    public override string[] GetNames() => System.Enum.GetNames(typeof(EDisplayMode));
    public override int GetCount() => System.Enum.GetValues(typeof(EDisplayMode)).Length;
    public override string GetName(int index) => GetNames()[index];
    public override void SetIndex(int index) => _displayMode = (EDisplayMode)index;
    public override int GetIndex() => (int)_displayMode;

    public bool HasChanged()
    {
        return _displayMode != _defaultDisplayMode;
    }

    public void Apply()
    {
        if (HasChanged())
        {
            Debug.Log("DisplayMode apply");
        }
    }

    public void Reset()
    {
        if (HasChanged())
        {
            Debug.Log("DisplayMode reset");
            _displayMode = _defaultDisplayMode;
            SetDisplayText();
        }
    }
}
