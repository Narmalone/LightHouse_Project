using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class CompasController : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    private bool _isSelected = false;
    [SerializeField] private MPUIKIT.MPImageBasic _arrowImage;
    [SerializeField] private TextMeshProUGUI _windDirectionText;
    [SerializeField] private WindDirection _windDirection;

    [SerializeField] private CustomEvent_WindDirection _onWindDirectionCompassCliqued;

    private void Awake()
    {
        _onWindDirectionCompassCliqued.handle += _onWindDirectionCompassCliqued_handle;
    }

    private void OnDestroy()
    {
        _onWindDirectionCompassCliqued.handle -= _onWindDirectionCompassCliqued_handle;
    }

    private void _onWindDirectionCompassCliqued_handle(WindDirection obj)
    {
        if(obj != this._windDirection && _isSelected)
        {
            _isSelected = false;
            _arrowImage.color = Color.black;
            _arrowImage.OutlineWidth = 0f;
            _arrowImage.FallOffDistance = 0f;
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (_isSelected) return;

        _onWindDirectionCompassCliqued?.Raise(_windDirection);
        _isSelected = true;
        _arrowImage.color = Color.red;
        _arrowImage.OutlineWidth = 0.001f;
        _arrowImage.FallOffDistance = 0.0001f;
        _arrowImage.OutlineColor = Color.black;
        _arrowImage.OnRebuildRequested();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        CursorManager.Instance.SetCursor(CursorType.ComputerClick);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        CursorManager.Instance.SetCursor(CursorType.ComputerDefault);
    }
}
