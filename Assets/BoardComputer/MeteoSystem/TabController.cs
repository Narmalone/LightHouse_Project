using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class TabController : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    public bool IsMouseOverElement = false;
    public bool IsSelected = false;

    public virtual void OnPointerClick(PointerEventData eventData)
    {
        if (IsSelected) return;   
    }

    public virtual void OnPointerEnter(PointerEventData eventData)
    {
        if(!IsMouseOverElement) IsMouseOverElement = true;
    }

    public virtual void OnPointerExit(PointerEventData eventData)
    {
        if (IsMouseOverElement) IsMouseOverElement = false;
    }
}
