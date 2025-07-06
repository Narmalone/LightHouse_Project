using UnityEngine;

public class DisplayCategory : MonoBehaviour
{
    // Référence aux catégories à afficher/masquer
    private Category[] _categories;

    void Start()
    {
        AddCategory();

        // Affiche Gameplay par défaut
        ShowCategory(0);
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
    public void OnClicGameplay() => ShowCategory(0);
    public void OnClicControls() => ShowCategory(1);
    public void OnClicVideo() => ShowCategory(2);
    public void OnClicSound() => ShowCategory(3);

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
