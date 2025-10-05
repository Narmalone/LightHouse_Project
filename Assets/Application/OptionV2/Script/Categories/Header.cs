using System.Collections.Generic;
using UnityEngine;

public class Header : MonoBehaviour
{
    [SerializeField] PopUp _popUp; // ref au Pop Up
    [SerializeField] List<IConfigurable> _settings = new List<IConfigurable>();
    [SerializeField] float _holdDuration = 2f; // durée requise en secondes

    float _holdTimer = 0f;
    bool _hasResetTriggered = false;
    Category[] _categories; // Référence aux catégories à afficher/masquer

    private void Awake()
    {
        GetAllSetting();
    }
    void Start()
    {
        AddCategory();
        ShowCategory(0); // Affiche Gameplay par défaut
    }

    void Update()
    {
        OnHoldInput();
    }

    void OnHoldInput()
    {
        // touche H du clavier
        if (Input.GetKey(KeyCode.H))
        {
            // fps => sec
            _holdTimer += Time.deltaTime;

            // Si le temps de maintient et supérieur/égale à la durée déterminée et qu'il que l'enclanchement n'a pas été réinitialisé
            if (_holdTimer >= _holdDuration && !_hasResetTriggered)
            {
                ResetAll(); // réinitialise tout
                _hasResetTriggered = true; // pour éviter de le refaire tant que la touche est maintenue
                Debug.Log("ResetAll déclenché après maintien");
            }
        }
        else
        {
            _holdTimer = 0f;
            _hasResetTriggered = false;
        }
    }

    void AddCategory()
    {
        // Si le tableau est vide, on le remplit automatiquement avec les enfants
        if (_categories == null || _categories.Length == 0)
        {
            // inclut les objets inactifs
            _categories = GetComponentsInChildren<Category>(true);
        }
    }

    void GetAllSetting()
    {
        _settings.Clear(); // vide la liste (juste au cas où)

        // récupère les components IConfigurable des enfants indirects
        IConfigurable[] configurables = GetComponentsInChildren<IConfigurable>(true);

        // parcourt le tableau
        foreach (IConfigurable configurable in configurables)
        {
            _settings.Add(configurable); // ajoute les enfants à la liste
        }
    }

    // Méthodes appelées quand on clique sur un bouton correspondant à une sous-catégorie
    public void OnClic(int index)
    {
        // Parcourt les categories
        foreach (var category in _categories)
        {
            // si il trouve un setting pas appliqué...
            if (category.GetUnappliedSetting())
            {
                _popUp.Show(); // pop apparait
            }
            else
            {
                ShowCategory(index); // chande de categories
            }
        }
    }

    // Affiche la sous-catégorie spécifiée et masque toutes les autres
    void ShowCategory(int indexToShow)
    {
        for (int i = 0; i < _categories.Length; i++)
        {
            // Vérifie que l'élément n'est pas null
            if (_categories[i] != null && i == indexToShow)
            {      
                // Affiche la sous-catégorie sélectionnée
                _categories[i].Show();
            }
            else
            {
                // Masque les autres
                _categories[i].Hide();
            }
        }
    }

    private void ResetAll()
    {
        // parcourt la liste _settings
        foreach (IConfigurable configurable in _settings)
        {
            // Si le paramètre à été changé...
            if (configurable.HasChanged())
            {
                configurable.Reset(); // réinitialise chaque le paramètre
            }
        }
    }
}
