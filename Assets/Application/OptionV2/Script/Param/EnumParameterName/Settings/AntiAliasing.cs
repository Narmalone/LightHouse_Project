using TMPro;
using UnityEngine;

public class AntiAliasing : EnumWrapper, IConfigurable
{
    [SerializeField] private EAntiAliasing _antiAliasing;
    [SerializeField] private EAntiAliasing _defaultAntiAliasing;
    [SerializeField] private EAntiAliasing _appliedAntiAliasing;

    private new void Start()
    {
        base.Start();
        _defaultAntiAliasing = EAntiAliasing.Disable;
        _appliedAntiAliasing = _antiAliasing;
    }

    public override string[] GetNames() => System.Enum.GetNames(typeof(EAntiAliasing));
    public override int GetCount() => System.Enum.GetValues(typeof(EAntiAliasing)).Length;
    public override string GetName(int index) => GetNames()[index];
    public override void SetIndex(int index) => _antiAliasing = (EAntiAliasing)index;
    public override int GetIndex() => (int)_antiAliasing;


    public bool HasChanged()
    {
        return _antiAliasing != _defaultAntiAliasing;
    }

    public void Apply()
    {
        if (HasChanged())
        {
            _appliedAntiAliasing = _antiAliasing;
            Debug.Log("Anti-Aliasing : " + QualitySettings.antiAliasing);
        }
    }

    public void Reset()
    {
        if (HasChanged())
        {
            _antiAliasing = _defaultAntiAliasing;
            _appliedAntiAliasing = _defaultAntiAliasing;
            SetDisplayText();
        }
    }

    public bool HasBeenApplied()
    {
        return _appliedAntiAliasing == _antiAliasing;
    }

    public override void SetParameter()
    {
        switch (_antiAliasing)
        {
            case EAntiAliasing.Disable:
                QualitySettings.antiAliasing = 0;
                break;
            case EAntiAliasing.MSAAx2:
                QualitySettings.antiAliasing = 2;
                break;
            case EAntiAliasing.MSAAx4:
                QualitySettings.antiAliasing = 4;
                break;
            case EAntiAliasing.MSAAx8:
                QualitySettings.antiAliasing = 8;
                break;
        }
    }
}
