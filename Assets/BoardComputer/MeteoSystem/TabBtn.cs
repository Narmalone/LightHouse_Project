using MPUIKIT;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class TabBtn : ComputerClickableElement, IPointerClickHandler
{
    public TabGroup TabGroup;
    public MPImageBasic background;
    public bool IsMouseOverElement = false;

    public Color InitialBackgroundColor;

    protected virtual void Awake()
    {
        TabGroup.Subscribe(this);
        InitialBackgroundColor = background.color;
    }

    public virtual void OnPointerClick(PointerEventData eventData)
    {
        TabGroup.OnTabSelected(this);
    }

    public override void OnPointerEnter(PointerEventData eventData)
    {
        base.OnPointerEnter(eventData);
        if(!IsMouseOverElement) IsMouseOverElement = true;
        TabGroup.OnTabEnter(this);
    }

    public override void OnPointerExit(PointerEventData eventData)
    {
        base.OnPointerExit(eventData);
        if (IsMouseOverElement) IsMouseOverElement = false;
        TabGroup.OnTabExit(this);
    }

    public virtual void Select() { }
    public virtual void DeSelect() { }
}
