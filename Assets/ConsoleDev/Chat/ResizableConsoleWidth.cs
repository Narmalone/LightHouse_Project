using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ResizableConsoleWidth : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    public RectTransform consoleRect;
    public float minX = 100f;
    public float maxX = 1920f;
    public float minY = 100f;
    public float maxY = 1080f;

    private Vector2 initialMousePosition;
    private Vector2 initialConsoleSize;

    [SerializeField]
    private bool forcePosOnStart = true;  
    
    [SerializeField]
    private Vector2 startPos = new Vector2(0f, 0f);
    public Vector2 StartPos => startPos;

    private Vector2 initialSize;
    public Vector2 InitialSize => initialSize;

    public enum AnchorPosition
    {
        TopLeft,
        TopRight,
        BottomLeft,
        BottomRight
    }

    public AnchorPosition anchorPosition = AnchorPosition.BottomRight;

    void Start()
    {
        SetAnchorPosition();
        if (forcePosOnStart)
        {
            consoleRect.anchoredPosition = startPos;
        }
        initialSize = new Vector2(consoleRect.rect.width, consoleRect.rect.height);
    }

    private void SetAnchorPosition()
    {
        switch (anchorPosition)
        {
            case AnchorPosition.TopLeft:
                consoleRect.anchorMin = new Vector2(0, 1);
                consoleRect.anchorMax = new Vector2(0, 1);
                consoleRect.pivot = new Vector2(0, 1);
                break;
            case AnchorPosition.TopRight:
                consoleRect.anchorMin = new Vector2(1, 1);
                consoleRect.anchorMax = new Vector2(1, 1);
                consoleRect.pivot = new Vector2(1, 1);
                break;
            case AnchorPosition.BottomLeft:
                consoleRect.anchorMin = new Vector2(0, 0);
                consoleRect.anchorMax = new Vector2(0, 0);
                consoleRect.pivot = new Vector2(0, 0);
                break;
            case AnchorPosition.BottomRight:
                consoleRect.anchorMin = new Vector2(1, 0);
                consoleRect.anchorMax = new Vector2(1, 0);
                consoleRect.pivot = new Vector2(1, 0);
                break;
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        initialMousePosition = eventData.position;
        initialConsoleSize = consoleRect.sizeDelta;
    }

    public void OnDrag(PointerEventData eventData)
    {
        Vector2 mouseDelta = eventData.position - initialMousePosition;
        Vector2 newSize = initialConsoleSize;

        switch (anchorPosition)
        {
            case AnchorPosition.TopLeft:
                newSize += new Vector2(mouseDelta.x, -mouseDelta.y);
                break;
            case AnchorPosition.TopRight:
                newSize += new Vector2(-mouseDelta.x, -mouseDelta.y);
                break;
            case AnchorPosition.BottomLeft:
                newSize += new Vector2(mouseDelta.x, mouseDelta.y);
                break;
            case AnchorPosition.BottomRight:
                newSize += new Vector2(-mouseDelta.x, mouseDelta.y);
                break;
        }

        newSize.x = Mathf.Clamp(newSize.x, minX, maxX);
        newSize.y = Mathf.Clamp(newSize.y, minY, maxY);
        consoleRect.sizeDelta = newSize;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        // Optional: you can add some logic here to handle the end of the drag operation
    }
}
