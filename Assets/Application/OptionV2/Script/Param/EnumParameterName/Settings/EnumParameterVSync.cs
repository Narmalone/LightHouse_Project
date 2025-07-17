using TMPro;
using UnityEngine;

public class EnumParameterVSync : EnumWrapper, IConfigurable
{
    private EActivableQuality _activableQuality;

    public override string[] GetNames() => System.Enum.GetNames(typeof(EActivableQuality));
    public override int GetCount() => System.Enum.GetValues(typeof(EActivableQuality)).Length;
    public override string GetName(int index) => GetNames()[index];
    public override void SetIndex(int index) => _activableQuality = (EActivableQuality)index;
    public override int GetIndex() => (int)_activableQuality;

    public bool HasChanged()
    {
        throw new System.NotImplementedException();
    }

    public void Apply()
    {
        throw new System.NotImplementedException();
    }

    public void Revert()
    {
        throw new System.NotImplementedException();
    }
}
