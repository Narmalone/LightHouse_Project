using UnityEngine;
using System;
using TMPro;

public class RadioFrequencyController : MonoBehaviour
{
    [SerializeField] private float targetFrequency = 92.5f;
    [SerializeField] private float tolerance = 0.3f;
    [SerializeField] private TextMeshProUGUI _frequencyDisplay;

    public float CurrentFrequency { get; private set; }

    public event Action<float, float> OnFrequencyChanged;
    public event Action<bool> OnSignalStateChanged;

    private bool _isInRange;

    public void HideFrequencyText()
    {
        _frequencyDisplay.gameObject.SetActive(false);
    }

    public void ShowFrequencyText()
    {
        _frequencyDisplay.gameObject.SetActive(true);
    }

    public void SetFrequency(float value)
    {
        CurrentFrequency = value;
        _frequencyDisplay.text = $"{CurrentFrequency:0.0} FM";
        OnFrequencyChanged?.Invoke(value, targetFrequency);

        bool inRange = Mathf.Abs(CurrentFrequency - targetFrequency) <= tolerance;
        if (inRange != _isInRange)
        {
            _isInRange = inRange;
            OnSignalStateChanged?.Invoke(_isInRange);
        }
    }
}