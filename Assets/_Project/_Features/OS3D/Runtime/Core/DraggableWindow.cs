using UnityEngine;
using UnityEngine.EventSystems;

namespace LightHouse.Features.Computer.OS
{
    [RequireComponent(typeof(RectTransform))]
    public class DraggableWindow : MonoBehaviour, IPointerDownHandler, IDragHandler
    {
        public RectTransform dragArea; // Ex: la barre de titre
        private RectTransform window;
        private Vector2 offset;

        private void Awake()
        {
            window = GetComponent<RectTransform>();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (dragArea == null || RectTransformUtility.RectangleContainsScreenPoint(dragArea, eventData.position, eventData.enterEventCamera))
            {
                RectTransformUtility.ScreenPointToLocalPointInRectangle(window, eventData.position, eventData.pressEventCamera, out offset);
            }
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (dragArea == null || RectTransformUtility.RectangleContainsScreenPoint(dragArea, eventData.position, eventData.enterEventCamera))
            {
                Vector2 localPoint;
                if (RectTransformUtility.ScreenPointToLocalPointInRectangle(window.parent as RectTransform, eventData.position, eventData.pressEventCamera, out localPoint))
                {
                    window.localPosition = localPoint - offset;
                }
            }
        }
    }

}
