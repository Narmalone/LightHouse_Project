using System;
using TMPro;
using UnityEngine;

public class UpdateState : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI DisplayText;
    [SerializeField] private int Index;
    [SerializeField] private Quality CurrentQuality;

    void Start()
    {
        // Initialise Index avec l'enum
        Index = (int)CurrentQuality;
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
        Index++;

        // établi des limites à l'index
        Index = Mathf.Clamp(Index, minValue, maxValue);

        // lie CurrentQuality à Index
        CurrentQuality = (Quality)Index;

        SetDisplayText();
    }

    private void Decrement(int minValue, int maxValue)
    {
        // décrement index
        Index--;

        // établi des limites à l'index
        Index = Mathf.Clamp(Index, minValue, maxValue);

        // lie CurrentQuality à Index
        CurrentQuality = (Quality)Index;

        SetDisplayText();
    }

    private void SetDisplayText()
    {
        DisplayText.text = CurrentQuality switch
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

