using TMPro;
using UnityEngine;


public class SubtitlesLanguages : EnumWrapper, IConfigurable
{
    private ELanguages _languages;

    public override string[] GetNames() => System.Enum.GetNames(typeof(ELanguages));
    public override int GetCount() => System.Enum.GetValues(typeof(ELanguages)).Length;
    public override string GetName(int index) => GetNames()[index];
    public override void SetIndex(int index) => _languages = (ELanguages)index;
    public override int GetIndex() => (int)_languages;

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

