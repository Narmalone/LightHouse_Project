using UnityEngine;
using UnityEngine.EventSystems;

public class RadialSliderInput : MonoBehaviour, IPointerDownHandler, IDragHandler
{
    [SerializeField] private RectTransform centerReference;
    [SerializeField] private RadialSlider radialSlider;

    [SerializeField] private float minAngle = 0f;
    [SerializeField] private float maxAngle = 360f;

    public void OnPointerDown(PointerEventData eventData) => UpdateValue(eventData);
    public void OnDrag(PointerEventData eventData) => UpdateValue(eventData);

    [SerializeField] private float innerRadius = 40f; // Rayon minimum accepté (zone vide centrale)
    [SerializeField] private float outerRadius = 100f; // Rayon maximum accepté (bord du cercle)

    private void UpdateValue(PointerEventData eventData)
    {
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(centerReference, eventData.position, eventData.pressEventCamera, out Vector2 localPoint))
            return;

        float distance = localPoint.magnitude;
        if (distance < innerRadius || distance > outerRadius)
            return; // Ignore clics hors du donut

        float angle = Mathf.Atan2(localPoint.y, localPoint.x) * Mathf.Rad2Deg;
        angle = (angle + 360f) % 360f;

        float angleRange = (maxAngle - minAngle + 360f) % 360f;
        float adjustedAngle = (angle - minAngle + 360f) % 360f;
        float value = Mathf.Clamp01(adjustedAngle / angleRange);

        radialSlider.SetValue(value);
        Debug.Log(radialSlider.value);
    }

}
