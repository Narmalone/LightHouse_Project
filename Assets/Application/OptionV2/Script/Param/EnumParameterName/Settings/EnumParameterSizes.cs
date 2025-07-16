using TMPro;
using UnityEngine;


public class EnumParameterLanguages : EnumWrapper
{
    [SerializeField]
    private TextMeshProUGUI _displayText;

    [SerializeField]
    private string[] _array;

    //private ELanguages Current => _size;

    private ESizes _size;

    void Start()
    {
        SetDisplayText();
    }

    public void OnClicPositiveButton() => Increment();
    public void OnClicNegativeButton() => Decrement();

    public override string[] GetNames() => System.Enum.GetNames(typeof(ESizes));
    public override int GetCount() => System.Enum.GetValues(typeof(ESizes)).Length;
    public override string GetName(int index) => GetNames()[index];
    public override void SetIndex(int index) => _size = (ESizes)index;
    public override int GetIndex() => (int)_size;

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

