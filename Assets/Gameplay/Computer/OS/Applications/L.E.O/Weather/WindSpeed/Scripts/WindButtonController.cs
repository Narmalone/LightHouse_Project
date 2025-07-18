using System;
using UnityEngine;

public class WindButtonController : MonoBehaviour
{
    [SerializeField] private CompassArrowElement[] buttons;
    private CompassArrowElement _lastSelectedButton;

    private void Awake()
    {
        foreach(var button in buttons)
        {
            button.CompassArrow += OnButtonCliqued;
        }
    }

    private void OnDestroy()
    {
        foreach (var button in buttons)
        {
            button.CompassArrow -= OnButtonCliqued;
        }
    }

    private void OnValidate()
    {
        buttons = GetComponentsInChildren<CompassArrowElement>();
    }

    private void OnButtonCliqued(CompassArrowElement cliquedButton)
    {
        if (_lastSelectedButton != null && cliquedButton != _lastSelectedButton)
        {
            _lastSelectedButton.OnDeselect();
        }
        else if(_lastSelectedButton != null && cliquedButton == _lastSelectedButton)
        {
            _lastSelectedButton.OnDeselect();
            _lastSelectedButton = null;
            return;
        }
        _lastSelectedButton = cliquedButton;
        _lastSelectedButton.OnSelect();
    }
}
