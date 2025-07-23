using TMPro;
using UnityEngine;


public class SubtitlesLanguages : EnumWrapper, IConfigurable
{
    private ELanguages _languages;
    private ELanguages _defaultLanguages;
    private ELanguages _appliedtLanguages;

    private new void Start()
    {
        base.Start();
        _defaultLanguages = ELanguages.English;
        _appliedtLanguages = _languages;
    }

    public override string[] GetNames() => System.Enum.GetNames(typeof(ELanguages));
    public override int GetCount() => System.Enum.GetValues(typeof(ELanguages)).Length;
    public override string GetName(int index) => GetNames()[index];
    public override void SetIndex(int index) => _languages = (ELanguages)index;
    public override int GetIndex() => (int)_languages;

    public bool HasChanged()
    {
        return _languages != _defaultLanguages;
    }

    public void Apply()
    {
        if (HasChanged())
        {
            Debug.Log("Subtitles Language apply");
            _appliedtLanguages = _languages;
        }
    }

    public void Reset()
    {
        if (HasChanged())
        {
            Debug.Log("Subtitles Language reset");
            _languages = _defaultLanguages;
            _appliedtLanguages = _defaultLanguages;
            SetDisplayText();
        }
    }

    bool IConfigurable.HasBeenApplied()
    {
        return _appliedtLanguages == _languages;
    }
}

