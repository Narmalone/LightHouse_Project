using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class MeteoTabBtn : TabBtn
{
    public GameObject HoveredObject;
    public Color HoverColor;
    public DayNightManager.DayState DayState;

    public override void OnPointerEnter(PointerEventData eventData)
    {
        base.OnPointerEnter(eventData);
        background.color = HoverColor;
    }

    public override void OnPointerExit(PointerEventData eventData)
    {
        base.OnPointerExit(eventData);
        background.color = InitialBackgroundColor;
    }

    public override void Select()
    {
        HoveredObject.SetActive(true);
    }

    public override void DeSelect()
    {
        HoveredObject.SetActive(false);
    }

    public override void OnPointerClick(PointerEventData eventData)
    {
        base.OnPointerClick(eventData);

    }
}
