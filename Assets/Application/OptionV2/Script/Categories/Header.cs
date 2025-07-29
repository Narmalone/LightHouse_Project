using System.Collections.Generic;
using UnityEngine;

public class Header : MonoBehaviour
{
    [SerializeField] PopUp _popUp; // ref au Pop Up
    [SerializeField] List<IConfigurable> _settings = new List<IConfigurable>();

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

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.H))
        {
            ResetAll();
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
        // Parcours des enfants directs
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform child = transform.GetChild(i);

            IConfigurable configurable = child.GetComponent<IConfigurable>();

            if (configurable != null)
            {
                _settings.Add(configurable);
                print("configurable");
            }
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
        foreach (IConfigurable configurable in _settings)
        {
            configurable.Reset();
            print("ResetAll");
        }
    }
}
