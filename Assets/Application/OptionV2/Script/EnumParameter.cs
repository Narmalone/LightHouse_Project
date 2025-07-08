using System;
using TMPro;
using UnityEngine;

public class EnumParameter : MonoBehaviour
{
    [SerializeField] private EnumWrapper enumWrapper;
    [SerializeField] private TextMeshProUGUI displayText;

    private void Start()
    {
        UpdateDisplay();
    }

    public void OnClicPositiveButton()
    {
        Increment();
    }

    public void OnClicNegativeButton()
    {
        Decrement();
    }

    void Increment()
    {
        int index = Mathf.Clamp(enumWrapper.GetIndex() + 1, 0, enumWrapper.GetCount() - 1);
        enumWrapper.SetIndex(index);
        UpdateDisplay();
    }
    
    void Decrement()
    {
        int index = Mathf.Clamp(enumWrapper.GetIndex() - 1, 0, enumWrapper.GetCount() - 1);
        enumWrapper.SetIndex(index);
        UpdateDisplay();
    }

    private void UpdateDisplay()
    {
        displayText.text = enumWrapper.GetName(enumWrapper.GetIndex());
    }
}

