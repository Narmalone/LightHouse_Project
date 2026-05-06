using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
using TMPro;

public class CellView : MonoBehaviour, IPointerClickHandler
{
    public int X;
    public int Y;

    [SerializeField] private Image _background;
    [SerializeField] private Image _icon;
    [SerializeField] private TextMeshProUGUI adjacentText;

    public event Action<int, int> OnClicked;
    public event Action<int, int> OnRightClicked;

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
            OnClicked?.Invoke(X, Y);
        else if (eventData.button == PointerEventData.InputButton.Right)
            OnRightClicked?.Invoke(X, Y);
    }

    public void SetSprite(Sprite sprite)
    {
        _icon.sprite = sprite;
    }

    public void SetText(string text)
    {
        adjacentText.text = text;
    }

    public void SetTextColor(Color color)
    {
        adjacentText.color = color;
    }

    public void SetFlagged(bool flagged)
    {
        // TODO : afficher un drapeau (optionnel)
    }

    public void SetBackgroundColor(Color color)
    {
        _background.color = color;    
    }

    public void SetIcondColor(Color color)
    {
        _icon.color = color;
    }
}