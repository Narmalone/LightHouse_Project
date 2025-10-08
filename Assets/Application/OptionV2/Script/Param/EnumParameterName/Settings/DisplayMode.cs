using UnityEngine;

public class DisplayMode : EnumWrapper, IConfigurable
{
    [SerializeField] private EDisplayMode _displayMode;
    [SerializeField] private EDisplayMode _defaultDisplayMode;
    [SerializeField] private EDisplayMode _appliedDisplayMode;

    private new void Start()
    {
        base.Start();
        _defaultDisplayMode = EDisplayMode.Windowed;
        _appliedDisplayMode = _displayMode;
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
        if (HasChanged() && !HasBeenApplied())
        {
            _appliedDisplayMode = _displayMode;
            Debug.Log("DisplayMode : " + Screen.fullScreenMode);
        }
    }

    public void Reset()
    {
        if (HasChanged())
        {
            _displayMode = _defaultDisplayMode;
            _appliedDisplayMode = _defaultDisplayMode;
            SetDisplayText();
        }
    }

    public bool HasBeenApplied()
    {
        return _appliedDisplayMode == _displayMode;
    }
    public override void SetParameter()
    {
        switch (_displayMode)
        {
            case EDisplayMode.Windowed:
                Screen.fullScreenMode = FullScreenMode.Windowed;
                break;
            case EDisplayMode.FullScreenWindowed:
                Screen.fullScreenMode = FullScreenMode.ExclusiveFullScreen;
                break;
            case EDisplayMode.FullScreen:
                Screen.fullScreenMode = FullScreenMode.FullScreenWindow;
                break;
        }
    }
}
