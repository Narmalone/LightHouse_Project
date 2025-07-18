using LightHouse.Interactions;
using System.Collections.Generic;
using UnityEngine;

public class SubCategory : MonoBehaviour, IDisplayable
{
    private CanvasGroup _canvasGroup;

    [SerializeField] private List<IConfigurable> _settings = new List<IConfigurable>();

    private void Awake()
    {
        // Récupère le CanvasGroup
        _canvasGroup = GetComponent<CanvasGroup>();
    }

    private void Start()
    {
        GetAllSetting();
    }

    private void Update()
    {
        // en fait c'est "A" mais pour une raison que j'ignore c'est en QWERTY
        if (Input.GetKeyDown(KeyCode.Q))
        {
            ApplyChangedSettings();
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            ResetChangedSettings();
        }
    }

    void GetAllSetting()
    {
        // Vérifie si ce GameObject lui-même a IConfigurable
        IConfigurable selfConfigurable = GetComponent<IConfigurable>();

        // Parcours des enfants directs
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform child = transform.GetChild(i);

            IConfigurable configurable = child.GetComponent<IConfigurable>();

            if (configurable != null)
            {
                _settings.Add(configurable);
            }
        }
    }

    // applique tout les param de la sous-catégorie
    void ApplyChangedSettings()
    {
        foreach ( IConfigurable configurable in _settings)
        {
            configurable.Apply();
        }
    }

    // réinitialise tout les param de la sous-catégorie
    void ResetChangedSettings()
    {
        foreach ( IConfigurable configurable in _settings)
        {
            configurable.Reset();
        }
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
