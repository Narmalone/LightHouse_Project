using UnityEngine;

public interface IRaycastable
{
    void OnRaycastEnter();
    void OnRaycastLeave();
    void OnClicked();
    void OnClickReleased();
}
