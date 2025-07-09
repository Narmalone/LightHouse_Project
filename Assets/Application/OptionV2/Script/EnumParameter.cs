using TMPro;
using UnityEngine;

public class EnumParameter : MonoBehaviour
{
    [SerializeField]
    private EnumWrapper _enumWrapper;
    [SerializeField]
    private TextMeshProUGUI _displayText;
    [SerializeField]
    private int Index;
    [SerializeField]
    private string[] _array;

    void Start()
    {
        _array = _enumWrapper.OptionName;
        Index = 0;
        SetDisplayText();
    }

    public void OnClicPositiveButton() => Increment(_array.Length - 1);
    public void OnClicNegativeButton() => Decrement(_array.Length - 1);

    private void Increment( int maxValue)
    {
        Index++;
        Index = Mathf.Clamp(Index,0, maxValue);
        SetDisplayText();
    }

    private void Decrement(int maxValue)
    {
        Index--;
        Index = Mathf.Clamp(Index,0, maxValue);
        SetDisplayText();
    }

    private void SetDisplayText()
    {
        if (Index >= 0 && Index < _array.Length)
        {
            _displayText.text = _array[Index];
        }
    }
}

