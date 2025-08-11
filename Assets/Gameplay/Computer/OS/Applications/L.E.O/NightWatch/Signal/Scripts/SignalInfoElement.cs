using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SignalInfoElement : MonoBehaviour
{
    [SerializeField] private Image _icon;
    [SerializeField] private TextMeshProUGUI _label;
    [SerializeField] private TextMeshProUGUI _timerText;

    private ISignal _model;
    public ISignal Model => _model;
    public string Key => _model.Key;

    public event Action<ISignal> OnTimerEnded;
    public Image Icon => _icon;

    public void Initialize(ISignal model, Sprite icon)
    {
        _model = model;
        _icon.sprite = icon;
        _label.text = model.DisplayText;
    }

    void Update()
    {
        if (_model.RemainingTime <= 0f)
        {
            OnTimerEnded?.Invoke(_model);
            return;
        }

        int m = Mathf.FloorToInt(_model.RemainingTime / 60f);
        int s = Mathf.FloorToInt(_model.RemainingTime % 60f);
        _timerText.text = $"{m:00}:{s:00}";
    }
}
