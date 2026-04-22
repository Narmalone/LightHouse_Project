using System;
using UnityEngine;

public class PlayerControllerMenu : MonoBehaviour
{
    public static event Action OnRightClickPressed;
    [SerializeField] private float maxDistance = 25f;

    private Camera _cam;

    private GameObject _lastObject;
    private IRaycastable _currentItem;
    private IRaycastable _lockedItem;
    private void Awake()
    {
        _cam = Camera.main;
    }

    private void Update()
    {
        HandleClick();
        if (_lockedItem != null)
        {
            // On reste focus sur l'objet lock
            return;
        }

        Ray ray = _cam.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit, maxDistance))
        {
            HandleHit(hit);
        }
        else
        {
            ClearCurrent();
        }

    }

    private void HandleClick()
    {
        if (Input.GetMouseButtonDown(0) && _currentItem != null)
        {
            _currentItem.OnClicked();
            _lockedItem = _currentItem;
        }

        if (Input.GetMouseButtonUp(0))
        {
            if(_currentItem != null)
                _currentItem.OnClickReleased();
            _lockedItem = null;
        }

        if (Input.GetMouseButtonDown(1) && _lockedItem == null)
        {
            OnRightClickPressed?.Invoke();
        }
    }

    private void HandleHit(RaycastHit hit)
    {
        GameObject hitObject = hit.collider.gameObject;

        // 👉 Même objet → ne rien faire
        if (_lastObject == hitObject)
            return;

        // 👉 Leave ancien
        if (_currentItem != null)
        {
            _currentItem.OnRaycastLeave();
            _currentItem = null;
        }

        _lastObject = hitObject;

        if (hit.collider.TryGetComponent(out IRaycastable newItem))
        {
            _currentItem = newItem;
            _currentItem.OnRaycastEnter();
        }
    }

    private void ClearCurrent()
    {
        if (_currentItem == null)
            return;

        _currentItem.OnRaycastLeave();
        _currentItem = null;
        _lastObject = null;
    }
}