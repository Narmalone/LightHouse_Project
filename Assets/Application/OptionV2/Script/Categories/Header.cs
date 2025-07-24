using System;
using System.Collections.Generic;
using UnityEngine;

public class Header : MonoBehaviour
{
    [SerializeField] PopUp _popUp;

    Category[] _categories; // Référence aux catégories à afficher/masquer

    void Start()
    {
        AddCategory();
        ShowCategory(0); // Affiche Gameplay par défaut
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

    // Méthodes appelées quand on clique sur un bouton correspondant à une sous-catégorie
    public void OnClic(int index)
    {
        foreach (var category in _categories)
        {
            if (category.HasAnyAppliedSetting())
            {
                ShowCategory(index);
            }
            else
            {
                _popUp.Show();
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
}
