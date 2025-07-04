using UnityEngine;

public class SubCategory : MonoBehaviour, IDisplayable
{

    public void ShowSubCategory()
    {
        gameObject.SetActive(true);
    }

    public void HideSubCategory()
    {
        gameObject.SetActive(false);
    }
}
