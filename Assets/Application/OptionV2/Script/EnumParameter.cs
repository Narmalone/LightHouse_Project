using System;
using TMPro;
using UnityEngine;

public class EnumParameter : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _displayText;

    private int _index;
    private Quality _currentQuality;

    void Start()
    {
        // Initialise Index avec l'enum
        _index = (int)_currentQuality;
        SetDisplayText();
    }

    // appelée lorsque PositiveButton est cliquer
    public void OnClicPositiveButton()
    {
        Increment(0, 4);
    }

    // appelée lorsque NegativeButton est cliquer
    public void OnClicNegativeButton()
    {
        Decrement(0, 4);
    }

    private void Increment(int minValue, int maxValue)
    {
        // increment index
        _index++;

        // établi des limites à l'index
        _index = Mathf.Clamp(_index, minValue, maxValue);

        // lie CurrentQuality à Index
        _currentQuality = (Quality)_index;

        SetDisplayText();
    }

    private void Decrement(int minValue, int maxValue)
    {
        // décrement index
        _index--;

        // établi des limites à l'index
        _index = Mathf.Clamp(_index, minValue, maxValue);

        // lie CurrentQuality à Index
        _currentQuality = (Quality)_index;

        SetDisplayText();
    }

    private void SetDisplayText()
    {
        _displayText.text = _currentQuality switch
        {
            Quality.Low => "Low",
            Quality.Medium => "Medium",
            Quality.High => "High",
            Quality.VeryHigh => "Very High",
            Quality.Epic => "Epic",
            _ => "Unknown"
        };
    }

    enum Quality
    {
        Low, Medium, High, VeryHigh, Epic
    }
}

