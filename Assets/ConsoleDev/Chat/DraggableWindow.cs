using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class DraggableWindow : MonoBehaviour, IPointerDownHandler, IDragHandler
{
    public RectTransform windowRect; // the RectTransform of the window

    private Vector2 initialMousePosition;
    private Vector2 initialWindowPosition;

    public void OnPointerDown(PointerEventData eventData)
    {
        initialMousePosition = eventData.position;
        initialWindowPosition = windowRect.anchoredPosition;
    }

    public void OnDrag(PointerEventData eventData)
    {
        Vector2 mouseDelta = eventData.position - initialMousePosition;
        windowRect.anchoredPosition = initialWindowPosition + mouseDelta;
    }
}