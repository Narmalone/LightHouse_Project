using TMPro;
using UnityEngine;

public class VSync : EnumWrapper, IConfigurable
{
    [SerializeField] private EVsync _vsync;
    [SerializeField] private EVsync _defaultvsync;
    [SerializeField] private EVsync _appliedvsync;

    private new void Start()
    {
        base.Start();
        _defaultvsync = EVsync.Disable;
        _appliedvsync = _vsync;
    }

    public override string[] GetNames() => System.Enum.GetNames(typeof(EVsync));
    public override int GetCount() => System.Enum.GetValues(typeof(EVsync)).Length;
    public override string GetName(int index) => GetNames()[index];
    public override void SetIndex(int index) => _vsync = (EVsync)index;
    public override int GetIndex() => (int)_vsync;

    public bool HasChanged()
    {
        return _vsync != _defaultvsync;
    }

    public void Apply()
    {
        if (HasChanged())
        {
            _appliedvsync = _vsync;
            Debug.Log("vSyncCount : " + QualitySettings.vSyncCount);
        }
    }

    public void Reset()
    {
        if (HasChanged())
        {
            _vsync = _defaultvsync;
            _appliedvsync = _defaultvsync;
            SetDisplayText();
            Debug.Log("vSyncCount : " + QualitySettings.vSyncCount);
        }
    }

    public bool HasBeenApplied()
    {
        return _appliedvsync == _vsync;
    }

    public override void SetParameter()
    {
        switch (_appliedvsync)
        {
            case EVsync.Disable:
                QualitySettings.vSyncCount = 0;
                break;
            case EVsync.X1:
                QualitySettings.vSyncCount = 1;
                break;
            case EVsync.X2:
                QualitySettings.vSyncCount = 2;       
                break;
            default:
                QualitySettings.vSyncCount = 0;
                break;
        }
    }
}
