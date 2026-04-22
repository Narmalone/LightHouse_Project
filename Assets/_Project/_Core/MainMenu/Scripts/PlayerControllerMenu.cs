using System;
using UnityEngine;

public class PlayerControllerMenu : MonoBehaviour
{
    public static event Action OnRightClickPressed;

    [SerializeField] private float maxDistance = 25f;

    private Camera _cam;

    private GameObject _lastObject;

    private IRaycastEnter _currentEnter;
    private IRaycastExit _currentExit;
    private IClickable _currentClickable;
    private IClickableUp _currentClickableUp;

    private IClickable _lockedClickable;
    private IClickableUp _lockedClickableUp;

    private void Awake()
    {
        _cam = Camera.main;
    }

    private void Update()
    {
        HandleClick();

        // 🔒 Si lock → pas de raycast
        if (_lockedClickable != null)
            return;

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

    // ─────────────────────────────
    // CLICK
    // ─────────────────────────────

    private void HandleClick()
    {
        if (Input.GetMouseButtonDown(0) && _currentClickable != null)
        {
            _currentClickable.OnClicked();

            _lockedClickable = _currentClickable;
            _lockedClickableUp = _currentClickableUp;
        }

        if (Input.GetMouseButtonUp(0))
        {
            _lockedClickableUp?.OnClickReleased();

            _lockedClickable = null;
            _lockedClickableUp = null;
        }

        if (Input.GetMouseButtonDown(1) && _lockedClickable == null)
        {
            OnRightClickPressed?.Invoke();
        }
    }

    // ─────────────────────────────
    // RAYCAST
    // ─────────────────────────────

    private void HandleHit(RaycastHit hit)
    {
        GameObject hitObject = hit.collider.gameObject;

        if (_lastObject == hitObject)
            return;

        ClearCurrent();

        _lastObject = hitObject;

        // 🔥 On récupère chaque capacité indépendamment
        hit.collider.TryGetComponent(out _currentEnter);
        hit.collider.TryGetComponent(out _currentExit);
        hit.collider.TryGetComponent(out _currentClickable);
        hit.collider.TryGetComponent(out _currentClickableUp);

        _currentEnter?.OnRaycastEnter();
    }

    private void ClearCurrent()
    {
        if (_currentExit != null)
        {
            _currentExit.OnRaycastExit();
        }

        _currentEnter = null;
        _currentExit = null;
        _currentClickable = null;
        _currentClickableUp = null;
        _lastObject = null;
    }
}