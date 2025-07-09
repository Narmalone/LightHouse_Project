using UnityEngine;
using UnityEngine.UI;

public class ToggleParameter : MonoBehaviour
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

        //Appeller une fonction lorsque l'interupteur est vrai
        print("Vrai");
    }
    
    public void OnClicFalse()
    {
        // change la couleur de l'autre boutton
        _TrueButton.image.color = new Color(1f, 1f, 1f, 0.5f);

        // change sa propre couleur
        _FalseButton.image.color = new Color(1f, 1f, 1f, 1f);

        //Appeller une fonction lorsque l'interrupteur est faux
        print("Faux");
    }
}
