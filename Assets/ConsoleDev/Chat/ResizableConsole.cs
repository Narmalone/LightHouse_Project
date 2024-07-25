using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ResizableConsole : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    public RectTransform consoleRect;
    public float minScale = 0.5f;
    public float maxScale = 2f;

    private Vector2 initialMousePosition;
    private float initialConsoleScale;

    public void OnPointerDown(PointerEventData eventData)
    {
        initialMousePosition = eventData.position;
        initialConsoleScale = consoleRect.localScale.x; // or y, since they should be equal
    }

    public void OnDrag(PointerEventData eventData)
    {
        Vector2 mouseDelta = eventData.position - initialMousePosition;
        float scaleDelta = mouseDelta.x * 0.01f; // or mouseDelta.y, since they should be equal
        float newScale = Mathf.Clamp(initialConsoleScale + scaleDelta, minScale, maxScale);
        consoleRect.localScale = new Vector2(newScale, newScale);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        // Optional: you can add some logic here to handle the end of the drag operation
    }
}