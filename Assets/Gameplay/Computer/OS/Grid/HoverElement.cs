using UnityEngine;
using UnityEngine.EventSystems;

public class HoverElement : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler, IPointerExitHandler
{
    public bool IsSelected;

    public void OnPointerClick(PointerEventData eventData)
    {
        IsSelected = !IsSelected;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        Debug.Log("hovering");
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Debug.Log("not hovering");
    }
}
