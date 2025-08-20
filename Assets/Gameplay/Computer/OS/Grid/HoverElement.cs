using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class HoverElement : MonoBehaviour, IPointerEnterHandler, IPointerClickHandler, IPointerExitHandler
{
    [SerializeField] private bool _isSelected;
    [SerializeField] private Image _targetGraphic;
    [SerializeField] private Color _hoverColor;
    [SerializeField] private Color _stopHoveringColor;

    public void OnPointerClick(PointerEventData eventData)
    {
        _isSelected = !_isSelected;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        _targetGraphic.color = _hoverColor;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _targetGraphic.color = _stopHoveringColor;
    }
}
