using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ShortcutButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [Header("UI")]
    public Image Graphic;

    [Header("Options")]
    public bool EnableHovering = true;
    public byte NumberOfClickNeededToSelect = 2;

    [Header("Colors")]
    public Color HoverColor;
    public Color SelectedColor;
    public Color NormalColor;
    public Color DisabledColor;

    public bool IsSelected { get; private set; }
    public bool IsHover { get; private set; }
    public bool IsDisabled { get; private set; }

    public event Action<ShortcutButton> OnClick;

    public void OnPointerClick(PointerEventData eventData)
    {
        if (IsDisabled) return;
        if (eventData.clickCount >= NumberOfClickNeededToSelect)
        {
            OnClick?.Invoke(this);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!EnableHovering || IsDisabled) return;
        IsHover = true;
        ApplyVisual();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!EnableHovering || IsDisabled) return;
        IsHover = false;
        ApplyVisual();
    }

    public void Select()
    {
        IsSelected = true;
        ApplyVisual();
    }

    public void Unselect()
    {
        IsSelected = false;
        ApplyVisual();
    }

    public void Disable()
    {
        IsDisabled = true;
        ApplyVisual();
    }

    public void Enable()
    {
        IsDisabled = false;
        ApplyVisual();
    }

    private void OnEnable()
    {
        ApplyVisual(); // remet à l'état normal quand activé
    }

    public void ApplyVisual()
    {
        if (Graphic == null) return;

        if (IsDisabled)
            Graphic.color = DisabledColor;
        else if (IsSelected)
            Graphic.color = SelectedColor;
        else if (IsHover)
            Graphic.color = HoverColor;
        else
            Graphic.color = NormalColor;

        
    }
}
