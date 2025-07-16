using UnityEngine;

public class ShowSubCategory : MonoBehaviour
{
    // Référence aux catégories à afficher/masquer
    private SubCategory[] _subCategories;

    void Start()
    {
        AddCategory();

        // Affiche Gameplay par défaut
        ShowCategory(0);
    }

    void AddCategory()
    {
        // Si le tableau est vide, on le remplit automatiquement avec les enfants
        if (_subCategories == null || _subCategories.Length == 0)
        {
            // inclut les objets inactifs
            _subCategories = GetComponentsInChildren<SubCategory>(true);
        }
    }

    // Méthodes appelées quand on clique sur un bouton correspondant à une sous-catégorie
    public void OnClicFirstSubCategory() => ShowCategory(0);
    public void OnClicSecondSubCategory() => ShowCategory(1);
    public void OnClicThirdSubCategory() => ShowCategory(2);

    // Affiche la sous-catégorie spécifiée et masque toutes les autres
    void ShowCategory(int indexToShow)
    {
        for (int i = 0; i < _subCategories.Length; i++)
        {
            // Vérifie que l'élément n'est pas null
            if (_subCategories[i] != null && i == indexToShow)
            {      
                // Affiche la sous-catégorie sélectionnée
                _subCategories[i].Show();
            }
            else
            {
                // Masque les autres
                _subCategories[i].Hide();
            }
        }
    }
}
