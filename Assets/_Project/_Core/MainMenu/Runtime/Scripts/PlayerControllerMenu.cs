using System;
using UnityEngine;

namespace LightHouse.Core.Interaction
{
    /// <summary>
    /// Gère les interactions joueur dans le menu via raycast (hover, click, hold, release).
    /// </summary>
    public class PlayerControllerMenu : MonoBehaviour
    {
        #region ===== Events =====

        public static event Action OnRightClickPressed;

        #endregion

        #region ===== Settings =====

        [SerializeField] private float _maxDistance = 25f;

        #endregion

        #region ===== State =====

        private Camera _camera;

        private GameObject _currentObject;

        // Raycast current
        private IRaycastEnter _currentEnter;
        private IRaycastExit _currentExit;
        private IClickable _currentClickable;
        private IClickableUp _currentClickableUp;

        // Locked (drag / hold)
        private IClickableHold _lockedHold;
        private IClickable _lockedClickable;
        private IClickableUp _lockedClickableUp;

        #endregion

        #region ===== Unity Lifecycle =====

        private void Awake()
        {
            Initialize();
        }

        private void Update()
        {
            HandleClick();

            if (IsLocked())
            {
                HandleHold();
                return;
            }

            HandleRaycast();
        }

        #endregion

        #region ===== Initialization =====

        private void Initialize()
        {
            _camera = Camera.main;
        }

        #endregion

        #region ===== Click Handling =====

        private void HandleClick()
        {
            if (Input.GetMouseButtonDown(0))
                HandleLeftClickDown();

            if (Input.GetMouseButtonUp(0))
                HandleLeftClickUp();

            if (Input.GetMouseButtonDown(1))
                HandleRightClick();
        }

        private void HandleLeftClickDown()
        {
            if (_currentClickable == null) return;

            _currentClickable.OnClicked();

            LockInteraction();
        }

        private void HandleLeftClickUp()
        {
            _lockedClickableUp?.OnClickReleased();
            UnlockInteraction();
        }

        private void HandleRightClick()
        {
            if (IsLocked()) return;

            OnRightClickPressed?.Invoke();
        }

        #endregion

        #region ===== Raycast =====

        private void HandleRaycast()
        {
            Ray ray = _camera.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out RaycastHit hit, _maxDistance))
            {
                ProcessHit(hit);
            }
            else
            {
                ClearCurrent();
            }
        }

        private void ProcessHit(RaycastHit hit)
        {
            GameObject hitObject = hit.collider.gameObject;

            if (_currentObject == hitObject)
                return;

            ClearCurrent();
            SetCurrent(hit);
        }

        private void SetCurrent(RaycastHit hit)
        {
            _currentObject = hit.collider.gameObject;

            hit.collider.TryGetComponent(out _currentEnter);
            hit.collider.TryGetComponent(out _currentExit);
            hit.collider.TryGetComponent(out _currentClickable);
            hit.collider.TryGetComponent(out _currentClickableUp);

            _currentEnter?.OnRaycastEnter();
        }

        private void ClearCurrent()
        {
            _currentExit?.OnRaycastExit();

            _currentEnter = null;
            _currentExit = null;
            _currentClickable = null;
            _currentClickableUp = null;
            _currentObject = null;
        }

        #endregion

        #region ===== Lock System =====

        private bool IsLocked()
        {
            return _lockedClickable != null;
        }

        private void LockInteraction()
        {
            _lockedClickable = _currentClickable;
            _lockedClickableUp = _currentClickableUp;

            if (_currentObject != null)
                _currentObject.TryGetComponent(out _lockedHold);
        }

        private void UnlockInteraction()
        {
            _lockedClickable = null;
            _lockedClickableUp = null;
            _lockedHold = null;
        }

        private void HandleHold()
        {
            _lockedHold?.OnClickHold();
        }

        #endregion
    }
}