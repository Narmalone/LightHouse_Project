using System;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(RectTransform))]
public class ClickableCircleSlider : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerClickHandler, IPointerExitHandler
{
    public event Action<float> OnValueChanged;
    [Range(0f, 1f)] public float ringThickness = 0.15f;
    public RectTransform ringTransform;
    public Material radialMaterial;
    private float _lastValue;
    private bool _handlingClick;

    public float CurrentValue => _lastValue;
    public Vector3 OuterLocalPoint;
    public Vector3 WorldOuterPoint;

    public void OnPointerClick(PointerEventData eventData)
    {
        HandleInteraction(eventData);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        HandleInteraction(eventData);
        _handlingClick = true;
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        if (_handlingClick)
        {
            _handlingClick = false;
            OnValueChanged?.Invoke(CurrentValue);
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        HandleInteraction(eventData);
    }

    private void HandleInteraction(PointerEventData eventData)
    {
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
            ringTransform, eventData.position, eventData.pressEventCamera, out Vector2 localPoint))
            return;

        Vector2 center = ringTransform.rect.center;
        Vector2 offset = localPoint - center;

        float rx = ringTransform.rect.width / 2f;
        float ry = ringTransform.rect.height / 2f;

        float normX = offset.x / rx;
        float normY = offset.y / ry;

        float radius = Mathf.Sqrt(normX * normX + normY * normY);

        float outerRadius = 1f;
        float innerRadius = 1f - ringThickness;

        if (radius < innerRadius || radius > outerRadius)
            return;

        float angle = Mathf.Atan2(normY, normX);
        if (angle < 0) angle += Mathf.PI * 2;

        float startAngle = radialMaterial.GetFloat("_StartAngle");
        float endAngle = radialMaterial.GetFloat("_EndAngle");
        float direction = Mathf.Sign(radialMaterial.GetFloat("_Direction"));

        float arcSpan = (direction > 0)
            ? NormalizeAngle(endAngle - startAngle)
            : NormalizeAngle(startAngle - endAngle);

        float clickAngle = (direction > 0)
            ? NormalizeAngle(angle - startAngle)
            : NormalizeAngle(startAngle - angle);

        if (clickAngle > arcSpan)
            return;

        float newFill = Mathf.Clamp01(clickAngle / arcSpan);
        radialMaterial.SetFloat("_FillValue", newFill);

        // Calcule du point juste à l’extérieur du cercle (en World)
        Vector2 dir = offset.normalized;
        Vector2 outerLocalPoint = center + dir * (ringTransform.rect.width * 0.5f); // à l'extérieur
        Vector3 worldOuterPoint = ringTransform.TransformPoint(outerLocalPoint);
        OuterLocalPoint = outerLocalPoint;
        WorldOuterPoint = worldOuterPoint;

        _lastValue = newFill;
        OnValueChanged?.Invoke(newFill);
        Debug.DrawRay(outerLocalPoint, dir, Color.yellow, 3f);
    }

    float NormalizeAngle(float angle)
    {
        float twoPI = Mathf.PI * 2;
        return (angle % twoPI + twoPI) % twoPI;
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (ringTransform == null) return;

        Gizmos.color = new Color(0f, 1f, 0f, 0.6f);
        DrawEllipseGizmo(ringTransform, 1f);

        Gizmos.color = new Color(1f, 0f, 0f, 0.6f);
        DrawEllipseGizmo(ringTransform, 1f - ringThickness);
    }

    private void DrawEllipseGizmo(RectTransform rectTransform, float normalizedRadius)
    {
        const int segments = 100;
        Vector3[] points = new Vector3[segments + 1];

        float rx = rectTransform.rect.width / 2f * normalizedRadius;
        float ry = rectTransform.rect.height / 2f * normalizedRadius;
        Vector3 center = rectTransform.position - Vector3.forward;

        for (int i = 0; i <= segments; i++)
        {
            float angle = 2f * Mathf.PI * i / segments;
            float x = Mathf.Cos(angle) * rx;
            float y = Mathf.Sin(angle) * ry;
            Vector3 local = new Vector3(x, y, 0f);
            points[i] = center + rectTransform.rotation * local;
        }

        for (int i = 0; i < segments; i++)
        {
            Gizmos.DrawLine(points[i], points[i + 1]);
        }
    }
#endif
}
