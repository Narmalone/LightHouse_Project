using UnityEngine;

public class Category : MonoBehaviour, IDisplayable
{
    public void Show()
    {
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}
