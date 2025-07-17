using System;
using UnityEngine;

public class GraphicsPreset : EnumWrapper, IConfigurable
{
    [SerializeField] private EQuality _quality;
    [SerializeField] private EQuality _defaultQualityIndex;


    private new void Start()
    {
        base.Start();
        _defaultQualityIndex = EQuality.Low;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            Apply();
        }

        if (Input.GetKeyDown(KeyCode.G))
        {
            Revert();
        }
    }

    public override string[] GetNames() => System.Enum.GetNames(typeof(EQuality));
    public override int GetCount() => System.Enum.GetValues(typeof(EQuality)).Length;
    public override string GetName(int index) => GetNames()[index];
    public override void SetIndex(int index) => _quality = (EQuality)index;
    public override int GetIndex() => (int)_quality;

    public bool HasChanged()
    {
        return _defaultQualityIndex != _quality;
    }

    public void Apply()
    {
        if (HasChanged())
        {
            Debug.Log("Graphics Preset apply");
        }
    }

    public void Revert()
    {
        if (HasChanged())
        {
            _quality = _defaultQualityIndex;
            SetDisplayText();
            Debug.Log("Graphics Preset reset");
        }
    }
}
