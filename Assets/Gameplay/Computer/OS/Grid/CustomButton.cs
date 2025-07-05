using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;

[RequireComponent(typeof(Image))]
public class CustomUIButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler,
                              IPointerClickHandler, ISelectHandler, IDeselectHandler
{
    public Action OnDoubleClick;

    public Color normalColor = Color.white;
    public Color hoverColor = new Color(0.8f, 0.8f, 1f);
    public Color selectedColor = new Color(0.7f, 0.7f, 1.2f);
    public Color pressedColor = Color.gray;

    private Image _image;
    private bool _isSelected = false;

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
        _image.color = pressedColor;
        EventSystem.current.SetSelectedGameObject(this.gameObject);

        if (eventData.clickCount >= 2)
        {
            OnDoubleClick?.Invoke();
        }
    }

    public void OnSelect(BaseEventData eventData)
    {
        _isSelected = true;
        _image.color = selectedColor;
    }

    public void OnDeselect(BaseEventData eventData)
    {
        _isSelected = false;
        _image.color = normalColor;
    }
}
