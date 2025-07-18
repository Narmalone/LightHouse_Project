using LightHouse.Weather;
using System;
using TMPro;
using UnityEngine;

public class CompassArrowElement : MonoBehaviour
{
    public event Action<CompassArrowElement> CompassArrow;
    [SerializeField] private WindOrientationType _windOrientation;
    [SerializeField] private CustomUIButton _button;
    [SerializeField] private TextMeshProUGUI _cardinalText;

    public WindOrientationType WindOrientation => _windOrientation;
    public CustomUIButton Button => _button;

    private void Awake()
    {
        _button.OnClick += OnCliqued;
    }

    private void OnDestroy()
    {
        _button.OnClick -= OnCliqued;
    }

    private void OnCliqued(CustomUIButton button)
    {
        CompassArrow?.Invoke(this);
    }

    public void OnSelect()
    {
        _button.Select();
        _cardinalText.color = _button.selectedColor;
        Debug.Log("select");
    }

    public void OnDeselect()
    {
        _button.Deselect();
        _cardinalText.color = Color.white;
        Debug.Log("deselect");
    }
}
