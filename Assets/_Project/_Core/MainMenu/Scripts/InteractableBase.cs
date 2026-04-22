using System;
using UnityEngine;

public class InteractableBase : MonoBehaviour, IClickable, IRaycastEnter, IRaycastExit
{
    public event Action OnHoverEnter;
    public event Action OnHoverExit;
    public event Action OnClickDown;
    public event Action OnClickUp;

    public void OnRaycastEnter() => OnHoverEnter?.Invoke();
    public void OnRaycastExit() => OnHoverExit?.Invoke();
    public void OnClicked() => OnClickDown?.Invoke();
    public void OnClickReleased() => OnClickUp?.Invoke();
}