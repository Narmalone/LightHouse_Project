using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AirPressureControllerWindow : MonoBehaviour
{
    [SerializeField] private PressureButton[] _airPressureButtons;
    [SerializeField] private TextMeshProUGUI _selectedHpaText;
    private PressureButton _selectedButton;

    private void Awake()
    {
        foreach (var button in _airPressureButtons)
        {
            button.OnClick += AirPressureButtonCliqued;
        }
    }

    private void OnDestroy()
    {
        foreach (var button in _airPressureButtons)
        {
            button.OnClick -= AirPressureButtonCliqued;
        }
    }

    private void AirPressureButtonCliqued(PressureButton clickedButton)
    {
        _selectedButton = clickedButton;

        // Mise à jour du texte
        _selectedHpaText.text = $"{_selectedButton.MinAirPressure}-{_selectedButton.MaxAirPressure} hPa";

        for (int i = 0; i < _airPressureButtons.Length; i++)
        {
            var img = _airPressureButtons[i].Button.targetGraphic;
            if (img == null) continue;

            img.color = (i >= Array.IndexOf(_airPressureButtons, clickedButton))
                ? Color.white
                : new Color(0.1f,0.1f,0.1f, 1);
        }

    }



    private void OnValidate()
    {
        _airPressureButtons = GetComponentsInChildren<PressureButton>();
    }
}
