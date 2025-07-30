using UnityEngine;
using UnityEngine.UI;

public class ToggleParameter : MonoBehaviour
{
    [SerializeField] protected bool _appliedEnable;
    [SerializeField] protected bool _enable;
    [SerializeField] protected bool _defaultEnable = true;

    [SerializeField] Button _trueButton;
    [SerializeField] Button _falseButton;

    Color _activeColor = new Color(1f, 1f, 1f, 1f);
    Color _inactiveColor = new Color(0.5f, 0.5f, 0.5f, 1f);

    protected void Start()
    {
        _trueButton.image.color = _activeColor;
        _falseButton.image.color = _inactiveColor;

        _enable = _defaultEnable;
        _appliedEnable = _enable;
    }

    public void OnClic(bool enable)
    {
        if (enable) // param buttons
        {
            _enable = true;
            Toggle();
        }
        else
        {
            _enable = false;
            Toggle();
        }
    }

    protected void Toggle()
    {
        if (_enable)
        {
            _falseButton.image.color = _inactiveColor; // change la couleur de l'autre boutton

            _trueButton.image.color = _activeColor; // change sa propre couleur

            True(); // Appelle une fonction lorsque l'interupteur est actif
        }
        else
        {
            _trueButton.image.color = _inactiveColor; // change la couleur de l'autre boutton

            _falseButton.image.color = _activeColor; // change sa propre couleur

            False(); //Appelle une fonction lorsque l'interrupteur est inactif
        }
    }

    protected void True()
    {
        //Debug.Log("true");
    }

    protected void False()
    {
        //Debug.Log("faux");
    }
}
