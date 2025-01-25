using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class EnterExitUiEvent : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public event Action<PointerEventData> pointerEnter;
    public event Action<PointerEventData> pointerExit;

    public virtual void OnPointerEnter(PointerEventData eventData)
    {
        pointerEnter?.Invoke(eventData);
    }

    public virtual void OnPointerExit(PointerEventData eventData)
    {
        pointerExit?.Invoke(eventData);
    }
}
