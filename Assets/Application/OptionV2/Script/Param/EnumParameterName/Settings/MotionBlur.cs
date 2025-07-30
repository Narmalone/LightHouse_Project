using UnityEngine;

public class MotionBlur : EnumWrapper, IConfigurable
{
    [SerializeField] private EQuality _quality;
    [SerializeField] private EQuality _defaultQuality;
    [SerializeField] private EQuality _appliedQuality;


    private new void Start()
    {
        base.Start();
        _defaultQuality = EQuality.Low;
        _appliedQuality = _quality;
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
        if (HasChanged() && !HasBeenApplied())
        {
            //Debug.Log("Motion Blur apply");
            _appliedQuality = _quality;
        }
    }

    public void Reset()
    {
        if (HasChanged())
        {
            //Debug.Log("Motion Blur reset");
            _quality = _defaultQuality;
            _appliedQuality = _defaultQuality;
            SetDisplayText();
        }
    }

    public bool HasBeenApplied()
    {
        return _appliedQuality == _quality;
    }
}
