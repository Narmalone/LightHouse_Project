using UnityEngine;
using UnityEngine.UI;

public class ToggleParameter : MonoBehaviour, IConfigurable
{
    [SerializeField] private Button _TrueButton;
    [SerializeField] private Button _FalseButton;
    
    void Start()
    {
        _FalseButton.image.color = new Color(1f, 1f, 1f, 0.5f);
        _TrueButton.image.color = new Color(1f, 1f, 1f, 1f);
    }

    public void OnClicTrue()
    {
        // change la couleur de l'autre boutton
        _FalseButton.image.color = new Color(1f, 1f, 1f,0.5f);

        // change sa propre couleur
        _TrueButton.image.color = new Color(1f, 1f, 1f, 1f);

        //Appelle une fonction lorsque l'interupteur est actif
        True();

        HasChanged(true);
    }

    public void OnClicFalse()
    {
        // change la couleur de l'autre boutton
        _TrueButton.image.color = new Color(1f, 1f, 1f, 0.5f);

        // change sa propre couleur
        _FalseButton.image.color = new Color(1f, 1f, 1f, 1f);

        //Appelle une fonction lorsque l'interrupteur est inactif
        False();

        HasChanged(false);
    }

    void True()
    {

    }

    void False()
    {

    }

    public bool HasChanged(bool state)
    {
        throw new System.NotImplementedException();
    }

    public void Apply()
    {
        throw new System.NotImplementedException();
    }

    public void Revert()
    {
        throw new System.NotImplementedException();
    }

    public bool HasChanged()
    {
        throw new System.NotImplementedException();
    }
}
