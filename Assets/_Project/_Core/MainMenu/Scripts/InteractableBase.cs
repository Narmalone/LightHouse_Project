using System;
using UnityEngine;

public class InteractableBase : MonoBehaviour, IRaycastable
{
    public event Action OnHoverEnter;
    public event Action OnHoverExit;
    public event Action OnClickDown;
    public event Action OnClickUp;

    public void OnRaycastEnter() => OnHoverEnter?.Invoke();
    public void OnRaycastLeave() => OnHoverExit?.Invoke();
    public void OnClicked() => OnClickDown?.Invoke();
    public void OnClickReleased() => OnClickUp?.Invoke();
}