using UnityEngine;
using UnityEngine.UI;

public class ToggleParameter : MonoBehaviour
{
    [SerializeField] private Button TrueButton;
    [SerializeField] private Button FalseButton;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        FalseButton.image.color = new Color(1f, 1f, 1f, 0.5f);
        TrueButton.image.color = new Color(1f, 1f, 1f, 1f);
    }

    public void OnClicTrue()
    {
        print("Vrai");
        // change la couleur de l'autre boutton

        FalseButton.image.color = new Color(1f, 1f, 1f,0.5f);

        // change sa propre couleur
        TrueButton.image.color = new Color(1f, 1f, 1f, 1f);

        //Appeller une fonction lorsque l'interupteur est vrai
    }
    
    public void OnClicFalse()
    {
        print("Faux");

        // change la couleur de l'autre boutton
        TrueButton.image.color = new Color(1f, 1f, 1f, 0.5f);

        // change sa propre couleur
        FalseButton.image.color = new Color(1f, 1f, 1f, 1f);

        //Appeller une fonction lorsque l'interrupteur est faux
    }
}
