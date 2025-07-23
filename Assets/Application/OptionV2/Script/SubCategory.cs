using LightHouse.Interactions;
using UnityEngine;

public class SubCategory : MonoBehaviour, IDisplayable
{
    private CanvasGroup _canvasGroup;

    private void Awake()
    {
        // Récupère le CanvasGroup
        _canvasGroup = GetComponent<CanvasGroup>();
    }

    public void Show()
    {
        // Visible
        _canvasGroup.alpha = 1f;

        // Permet l'interaction
        _canvasGroup.interactable = true;

        // Permet les clics
        _canvasGroup.blocksRaycasts = true;  
    }

    public void Hide()
    {
        // invisible
        _canvasGroup.alpha = 0f;

        // Désactiver interaction
        _canvasGroup.interactable = false;

        // Ignorer les clics
        _canvasGroup.blocksRaycasts = false; 
    }
}
