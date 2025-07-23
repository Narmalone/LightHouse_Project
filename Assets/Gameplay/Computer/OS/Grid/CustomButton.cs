using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;

[RequireComponent(typeof(Image))]
public class CustomUIButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler,
                              IPointerClickHandler
{
    public Action<CustomUIButton> OnClick;
    public Action OnDoubleClick { get; set; }

    public Color normalColor = Color.white;
    public Color hoverColor = new Color(0.8f, 0.8f, 1f);
    public Color selectedColor = new Color(0.7f, 0.7f, 1.2f);

    private Image _image;
    public bool _isSelected = false;

    private void Awake()
    {
        _image = GetComponent<Image>();
        _image.color = normalColor;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!_isSelected)
            _image.color = hoverColor;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!_isSelected)
            _image.color = normalColor;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        _isSelected = true;
        OnClick?.Invoke(this);
        if (eventData.clickCount >= 2)
        {
            OnDoubleClick?.Invoke();
        }
    }

    public void Deselect()
    {
        _isSelected = false;
        _image.color = normalColor;
    }
    public void Select()
    {
        _isSelected = true;
        _image.color = selectedColor;
    }   
}
