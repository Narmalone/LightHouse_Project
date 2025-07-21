using UnityEngine;

public class ShadowsQualities : EnumWrapper, IConfigurable
{
    [SerializeField] private EQuality _quality;
    [SerializeField] private EQuality _defaultQuality;


    private new void Start()
    {
        base.Start();
        _defaultQuality = EQuality.Low;
    }

    public override string[] GetNames() => System.Enum.GetNames(typeof(EQuality));
    public override int GetCount() => System.Enum.GetValues(typeof(EQuality)).Length;
    public override string GetName(int index) => GetNames()[index];
    public override void SetIndex(int index) => _quality = (EQuality)index;
    public override int GetIndex() => (int)_quality;

    public bool HasChanged()
    {
        return _defaultQuality != _quality;
    }

    public void Apply()
    {
        if (HasChanged())
        {
            Debug.Log("Shadows Qualities apply");
        }
    }

    public void Reset()
    {
        if (HasChanged())
        {
            Debug.Log("Shadows Qualities reset");
            _quality = _defaultQuality;
            SetDisplayText();
        }
    }
}
