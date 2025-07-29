using UnityEngine;

public class Category : MonoBehaviour, IDisplayable
{
    // Référence aux catégories à afficher/masquer
    [SerializeField] private SubCategory[] _subCategories;
    [SerializeField] private PopUp _popUp;

    CanvasGroup _canvasGroup;

    SubCategory[] SubCategories
    {
        get
        {
            if (_subCategories == null || _subCategories.Length == 0)
            {
                AddCategory();
            }
            return _subCategories;
        }
    }
    private void Update()
    {
        //print(GetUnappliedSetting());
    }
    private void Awake()
    {
       AddCategory();
    }

    void Start()
    {
       ShowCategory(0); // Affiche Gameplay par défaut
       _canvasGroup = GetComponent<CanvasGroup>();

    }

    public bool GetUnappliedSetting()
    {
        // Parcourt les subCategories
        foreach (var subCategories in SubCategories)
        {
            // s'il y a des param non-appliqués...
            if (subCategories.HasAnyUnappliedSetting())
            {
                return true; // renvoie vrai
            }
        }
        return false; // renvoie faux
    }

    void AddCategory()
    {
        // Si le tableau est vide, on le remplit automatiquement avec les enfants
        if (_subCategories == null || _subCategories.Length == 0)
        {
            // inclut les objets SubCategory actif / inactifs à l'array
            _subCategories = GetComponentsInChildren<SubCategory>(true);
        }
    }

    // Méthodes appelées quand on clique sur un bouton correspondant à une sous-catégorie
    public void OnClic(int index)
    {
        // s'il y a des param non-appliqués...
        if (GetUnappliedSetting())
        {
            _popUp.Show(); // fait apparaitre le pop up
        }
        else
        {
            ShowCategory(index); // change de catégorie
        }
    }

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

    public void Show()
    {
        //SetCanvaGroup(1f, true, true);
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        //SetCanvaGroup(0f, false, false);
        gameObject.SetActive(false);
    }

    // change les valeurs du canva group
    void SetCanvaGroup(float alpha, bool interactable, bool blocksRaycasts)
    {
        _canvasGroup.alpha = alpha;
        _canvasGroup.interactable = interactable;
        _canvasGroup.blocksRaycasts = blocksRaycasts;
    }
}
