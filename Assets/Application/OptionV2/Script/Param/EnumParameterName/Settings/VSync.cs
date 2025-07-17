using TMPro;
using UnityEngine;

public class VSync : EnumWrapper, IConfigurable
{
    private EActivableQuality _activableQuality;
    private EActivableQuality _defaultActivableQuality;

    private new void Start()
    {
        base.Start();
        _defaultActivableQuality = EActivableQuality.Disable;
    }

    public override string[] GetNames() => System.Enum.GetNames(typeof(EActivableQuality));
    public override int GetCount() => System.Enum.GetValues(typeof(EActivableQuality)).Length;
    public override string GetName(int index) => GetNames()[index];
    public override void SetIndex(int index) => _activableQuality = (EActivableQuality)index;
    public override int GetIndex() => (int)_activableQuality;

    public bool HasChanged()
    {
        return _activableQuality != _defaultActivableQuality;
    }

    public void Apply()
    {
        Debug.Log("Vsync apply");
    }

    public void Revert()
    {
        Debug.Log("Vsync reset");
        _activableQuality = _defaultActivableQuality;
        SetDisplayText();
    }
}
