using System;
using TMPro;
using UnityEngine;

public class EnumParameter : MonoBehaviour
{
    [SerializeField] private EnumWrapper _enumWrapper;
    [SerializeField] private TextMeshProUGUI _displayText;

    private void Start()
    {
        UpdateDisplay();
    }

    public void OnClicPositiveButton() => Increment();
    public void OnClicNegativeButton() => Decrement();

    void Increment()
    {
        int index = Mathf.Clamp(_enumWrapper.GetIndex() + 1, 0, _enumWrapper.GetCount() - 1);
        _enumWrapper.SetIndex(index);
        UpdateDisplay();
    }
    
    void Decrement()
    {
        int index = Mathf.Clamp(_enumWrapper.GetIndex() - 1, 0, _enumWrapper.GetCount() - 1);
        _enumWrapper.SetIndex(index);
        UpdateDisplay();
    }

    void UpdateDisplay()
    {
        _displayText.text = _enumWrapper.GetName(_enumWrapper.GetIndex());
    }
}

