using TMPro;
using UnityEngine;

public abstract class EnumWrapper : MonoBehaviour
{

    [SerializeField] protected TextMeshProUGUI _displayText;

    protected void Start()
    {
        SetDisplayText();
    }
    
    public abstract string[] GetNames();
    public abstract int GetCount();
    public abstract string GetName(int index);
    public abstract void SetIndex(int index);
    public abstract int GetIndex();

    public void OnClicPositiveButton() => Increment();
    public void OnClicNegativeButton() => Decrement();

    protected void Increment()
    {
        int index = Mathf.Clamp(GetIndex() + 1, 0, GetCount() - 1);
        SetIndex(index);
        SetDisplayText();
    }
    protected void Decrement()
    {
        int index = Mathf.Clamp(GetIndex() - 1, 0, GetCount() - 1);
        SetIndex(index);
        SetDisplayText();
    }
    protected void SetDisplayText()
    {
        _displayText.text = GetName(GetIndex());
    }
}
