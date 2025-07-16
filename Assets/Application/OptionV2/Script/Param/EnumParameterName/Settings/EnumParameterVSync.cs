using TMPro;
using UnityEngine;

public class EnumParameterVSync : EnumWrapper
{
    [SerializeField]
    private TextMeshProUGUI _displayText;

    //private ELanguages Current => _activableQuality;
    private EActivableQuality _activableQuality;

    void Start()
    {
        SetDisplayText();
    }

    public void OnClicPositiveButton() => Increment();
    public void OnClicNegativeButton() => Decrement();

    public override string[] GetNames() => System.Enum.GetNames(typeof(EActivableQuality));
    public override int GetCount() => System.Enum.GetValues(typeof(EActivableQuality)).Length;
    public override string GetName(int index) => GetNames()[index];
    public override void SetIndex(int index) => _activableQuality = (EActivableQuality)index;
    public override int GetIndex() => (int)_activableQuality;

    private void Increment()
    {
        int index = Mathf.Clamp(GetIndex() + 1, 0, GetCount() - 1);
        SetIndex(index);
        SetDisplayText();
    }
    private void Decrement()
    {
        int index = Mathf.Clamp(GetIndex() - 1, 0, GetCount() - 1);
        SetIndex(index);
        SetDisplayText();
    }

    private void SetDisplayText()
    {
        _displayText.text = GetName(GetIndex());
    }
}
