using UnityEngine;
using UnityEngine.UI;

public class ToggleParameter : MonoBehaviour
{
    [SerializeField] private Button _TrueButton;
    [SerializeField] private Button _FalseButton;

    private Color _activeColor = new Color(1f, 1f, 1f, 1f);
    private Color _inactiveColor = new Color(1f, 1f, 1f, 0.5f);

    protected void Start()
    {
        _FalseButton.image.color = _inactiveColor;
        _TrueButton.image.color = _activeColor;
    }

    public void OnClicTrue()
    {
        // change la couleur de l'autre boutton
        _FalseButton.image.color = _inactiveColor;

        // change sa propre couleur
        _TrueButton.image.color = _activeColor;

        //Appelle une fonction lorsque l'interupteur est actif
        True();
    }

    public void OnClicFalse()
    {
        // change la couleur de l'autre boutton
        _TrueButton.image.color = _inactiveColor;

        // change sa propre couleur
        _FalseButton.image.color = _activeColor;

        //Appelle une fonction lorsque l'interrupteur est inactif
        False();
    }

    protected void True()
    {
        Debug.Log("true");
    }

    protected void False()
    {
        Debug.Log("faux");
    }
}
