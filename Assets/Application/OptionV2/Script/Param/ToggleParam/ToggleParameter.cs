using UnityEngine;
using UnityEngine.UI;

public class ToggleParameter : MonoBehaviour
{
    [SerializeField] protected bool _appliedEnable;
    [SerializeField] protected bool _enable;
    [SerializeField] protected bool _defaultEnable = true;

    [SerializeField] Button _TrueButton;
    [SerializeField] Button _FalseButton;

    Color _activeColor = new Color(1f, 1f, 1f, 1f);
    Color _inactiveColor = new Color(0.5f, 0.5f, 0.5f, 1f);

    protected void Start()
    {
        _TrueButton.image.color = _activeColor;
        _FalseButton.image.color = _inactiveColor;
        _enable = _defaultEnable;
        _appliedEnable = _enable;
    }

    public void OnClic(bool enable)
    {
        if (enable)
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
            
            // change la couleur de l'autre boutton
            _FalseButton.image.color = _inactiveColor;

            // change sa propre couleur
            _TrueButton.image.color = _activeColor;

            //Appelle une fonction lorsque l'interupteur est actif
            True();
        }
        else
        {

            // change la couleur de l'autre boutton
            _TrueButton.image.color = _inactiveColor;

            // change sa propre couleur
            _FalseButton.image.color = _activeColor;

            //Appelle une fonction lorsque l'interrupteur est inactif
            False();
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
