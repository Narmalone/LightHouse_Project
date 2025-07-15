using TMPro;
using UnityEngine;


public class EnumParameterLanguages : EnumWrapper, IConfigurable
{
    [SerializeField]
    private TextMeshProUGUI _displayText;

    [SerializeField]
    private string[] _array;

    private Languages Current => current;
    private enum Languages { English, French, Spanish, Chinese, Japonese, Russian}

    private Languages current;

    void Start()
    {
        SetDisplayText();
    }

    public void OnClicPositiveButton() => Increment();
    public void OnClicNegativeButton() => Decrement();

    public override string[] GetNames() => System.Enum.GetNames(typeof(Languages));
    public override int GetCount() => System.Enum.GetValues(typeof(Languages)).Length;
    public override string GetName(int index) => GetNames()[index];
    public override void SetIndex(int index) => current = (Languages)index;
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

