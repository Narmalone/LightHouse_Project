using TMPro;
using UnityEngine;


public class EnumParameterSizes : EnumWrapper, IConfigurable
{
    [SerializeField]
    private TextMeshProUGUI _displayText;

    [SerializeField]
    private string[] _array;

    private Sizes Current => current;
    private enum Sizes { Small, Medium, Large }

    private Sizes current;

    void Start()
    {
        SetDisplayText();
    }

    public void OnClicPositiveButton() => Increment();
    public void OnClicNegativeButton() => Decrement();

    public override string[] GetNames() => System.Enum.GetNames(typeof(Sizes));
    public override int GetCount() => System.Enum.GetValues(typeof(Sizes)).Length;
    public override string GetName(int index) => GetNames()[index];
    public override void SetIndex(int index) => current = (Sizes)index;
    public override int GetIndex() => (int)current;

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

