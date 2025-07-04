using UnityEngine;
using System.Collections.Generic;

public class DisplaySubCategory : MonoBehaviour
{
    [SerializeField] private SubCategory[] SubCategories;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        OnClicGameplay();
    }

    public void OnClicGameplay()
    {
        print("Gameplay");
        SubCategories[0].ShowSubCategory();
        SubCategories[1].HideSubCategory();
        SubCategories[2].HideSubCategory();
        SubCategories[3].HideSubCategory();
    }

    public void OnClicControls()
    {
        print("Controls");
        SubCategories[1].ShowSubCategory();
        SubCategories[0].HideSubCategory();
        SubCategories[2].HideSubCategory();
        SubCategories[3].HideSubCategory();
    }

    public void OnClicVideo()
    {
        print("Video");
        SubCategories[2].ShowSubCategory();
        SubCategories[1].HideSubCategory();
        SubCategories[0].HideSubCategory();
        SubCategories[3].HideSubCategory();
    }

    public void OnClicSound()
    {
        print("son");
        SubCategories[3].ShowSubCategory();
        SubCategories[1].HideSubCategory();
        SubCategories[2].HideSubCategory();
        SubCategories[0].HideSubCategory();
    }
}
