using TMPro;
using UnityEngine;

public class VSync : EnumWrapper, IConfigurable
{
    [SerializeField] private EActivableQuality _activableQuality;
    [SerializeField] private EActivableQuality _defaultActivableQuality;
    [SerializeField] private EActivableQuality _appliedActivableQuality;

    private new void Start()
    {
        base.Start();
        _defaultActivableQuality = EActivableQuality.Disable;
        _appliedActivableQuality = _activableQuality;
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
        if (HasChanged())
        {
            Debug.Log("Vsync apply");
            _appliedActivableQuality = _activableQuality;
        }
    }

    public void Reset()
    {
        if (HasChanged())
        {
            Debug.Log("Vsync reset");
            _activableQuality = _defaultActivableQuality;
            _appliedActivableQuality = _defaultActivableQuality;
            SetDisplayText();
        }
    }

    public bool HasBeenApplied()
    {
        return _appliedActivableQuality == _activableQuality;
    }
}
