using System;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class PressureButton : MonoBehaviour
{
    [SerializeField] private Button _button;
    [SerializeField] private float _minAirPressure = 980.0f;
    [SerializeField] private float _maxAirPressure = 980.0f;
    public event Action<PressureButton> OnClick;

    public Button Button => _button;
    public float MinAirPressure => _minAirPressure;
    public float MaxAirPressure => _maxAirPressure;

    private void Awake()
    {
        _button.onClick.AddListener(OnButtonCliqued);
    }

    private void OnDestroy()
    {
        _button.onClick.RemoveListener(OnButtonCliqued);
    }

    private void OnValidate()
    {
        if (_button != null) _button = GetComponent<Button>();
    }

    private void OnButtonCliqued()
    {
        OnClick?.Invoke(this);
    }
}
