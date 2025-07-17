using TMPro;
using UnityEngine;


public class EnumParameterLanguages : EnumWrapper, IConfigurable
{
    private ESizes _size;

    public override string[] GetNames() => System.Enum.GetNames(typeof(ESizes));
    public override int GetCount() => System.Enum.GetValues(typeof(ESizes)).Length;
    public override string GetName(int index) => GetNames()[index];
    public override void SetIndex(int index) => _size = (ESizes)index;
    public override int GetIndex() => (int)_size;

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

