using System;
using UnityEngine;
using LightHouse.Interactions;
using LightHouse.Inputs;
using LightHouse.Utilities;

namespace LightHouse.KinematicCharacterController
{
    public class PlayerInteractions : MonoBehaviour
    {
        #region SERIALIZED FIELDS
        [Header("Interaction Settings")]
        [SerializeField] private bool _enableInteraction = true;

        [Header("UI")]
        [SerializeField] private CanvasInteraction _canvasInteraction;

        [Header("Raycast")]
        [SerializeField] private Camera _playerCamera;
        [SerializeField] private float _interactionDistance = 3.0f;
        [SerializeField] private LayerMask _interactableLayer = 1 << 0;
        [SerializeField] private QueryTriggerInteraction _queryTriggerInteraction = QueryTriggerInteraction.Ignore;

        #endregion

        #region PRIVATE FIELDS
        private GameObject _lastObjectSeen = null;  //Store last object seen
        private IInteractable _raycastedIInteractable = null;
        private IItemName _raycastedIItemName = null;

        #endregion

        #region EVENTS AND PROPERTIES
        public event Action<IInteractable> OnInteractableItemDetected;
        #endregion

        #region MONO'S CALLBACK

        private void Start()
        {
            _canvasInteraction.Hide();
        }

        private void Update()
        {
            if (!_enableInteraction) return;
            Ray cameraRay = _playerCamera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0f));
            if (RaycastUtility.TryRaycast(cameraRay, _interactionDistance, _interactableLayer, _queryTriggerInteraction, out RaycastHit hit))
            {
                HandleNewObject(hit.collider.gameObject);
                HandleInteraction();
            }
            else
            {
                ResetSeenObject();
            }

            Debug.DrawRay(_playerCamera.transform.position, cameraRay.direction * _interactionDistance, Color.magenta);
        }

        #endregion

        #region RAYCAST HANDLER

        private void HandleNewObject(GameObject hitObject)
        {
            if (hitObject != _lastObjectSeen)
            {
                _lastObjectSeen = hitObject;
                _raycastedIInteractable = hitObject.GetComponent<IInteractable>();
                _raycastedIItemName = hitObject.GetComponent<IItemName>();

                if (_raycastedIInteractable != null)
                {
                    OnInteractableItemDetected?.Invoke(_raycastedIInteractable);
                    UpdateInteractionName();
                }
                else
                {
                    _canvasInteraction.HideItemInteractionName();
                }

                if (_raycastedIItemName != null)
                {
                    UpdateName();
                    _raycastedIItemName.IsItemRaycasted = true;
                }
                else
                {
                    _canvasInteraction.HideItemName();
                }
            }
        }

        private void ResetSeenObject()
        {
            if (_lastObjectSeen != null)
                _lastObjectSeen = null;

            if (_raycastedIItemName != null)
            {
                _raycastedIItemName.OnNameUpdated -= OnCurrentSeingObjectNameUpdate;
                _raycastedIItemName.IsItemRaycasted = false;
                _raycastedIItemName = null;
            }

            if (_canvasInteraction.ItemName_TMP.isActiveAndEnabled)
                _canvasInteraction.HideItemName();

            if (_raycastedIInteractable != null)
            {
                _raycastedIInteractable.OnInteractionNameChanged -= OnCurrentSeingObjectDescriptionUpdate;
                _raycastedIInteractable = null;
            }

            if(_canvasInteraction.ItemInteractionName_TMP.isActiveAndEnabled)
                _canvasInteraction.HideItemInteractionName();
        }
        #endregion

        #region HANDLE INPUTS
        private void HandleInteraction()
        {
            if (InputManager.Interact.WasPerformedThisFrame() && _raycastedIInteractable != null)
            {
                if(_raycastedIInteractable.CanBeInteracted)
                    _raycastedIInteractable.Interact();
            }
        }
        #endregion

        #region Update Interactions and Name Texts
        public void UpdateInteractionName()
        {
            if (_raycastedIInteractable == null) return;
            string itemInteractionName = _raycastedIInteractable?.GetInteractionName();
            _raycastedIInteractable.OnInteractionNameChanged += OnCurrentSeingObjectDescriptionUpdate;

            if (_canvasInteraction.IsHided)
                _canvasInteraction.Show();

            if (string.IsNullOrEmpty(itemInteractionName))
            {
                _canvasInteraction.HideItemInteractionName();
            }
            else
            {
                _canvasInteraction.ItemInteractionName_TMP.text = itemInteractionName;
                _canvasInteraction.ShowItemInteractionName();
            }

            if (!_raycastedIInteractable.CanBeRaycasted)
            {
                _canvasInteraction.HideItemInteractionName();
            }
        }

        public void UpdateName()
        {
            if (_raycastedIItemName == null) return;
            string itemName = _raycastedIItemName.GetName();
            _raycastedIItemName.OnNameUpdated += OnCurrentSeingObjectNameUpdate;

            if (_canvasInteraction.IsHided)
                _canvasInteraction.Show();

            if (string.IsNullOrEmpty(itemName))
            {
                _canvasInteraction.HideItemName();
            }
            else
            {
                _canvasInteraction.ItemName_TMP.text = itemName;
                _canvasInteraction.ShowItemName();
            }

            if (!_raycastedIItemName.CanBeRaycasted)
            {
                _canvasInteraction.HideItemName();
            }
        }

        #endregion

        #region Object Callbacks

        private void OnCurrentSeingObjectNameUpdate()
        {
            if (_raycastedIItemName == null) return;
            string itemName = _raycastedIItemName?.GetName();
            if (string.IsNullOrEmpty(itemName))
            {
                _canvasInteraction.HideItemName();
            }
            else
            {
                _canvasInteraction.ItemName_TMP.text = itemName;
                _canvasInteraction.ShowItemName();
            }

            if (!_raycastedIItemName.CanBeRaycasted)
            {
                _canvasInteraction.HideItemName();
            }
        }

        private void OnCurrentSeingObjectDescriptionUpdate()
        {
            if (_raycastedIInteractable == null) return;
            string itemDescription = _raycastedIInteractable?.GetInteractionName();
            if (string.IsNullOrEmpty(itemDescription))
            {
                _canvasInteraction.HideItemInteractionName();
            }
            else
            {
                _canvasInteraction.ItemInteractionName_TMP.text = itemDescription;
                _canvasInteraction.ShowItemInteractionName();
            }
            if (!_raycastedIInteractable.CanBeRaycasted)
            {
                _canvasInteraction.HideItemInteractionName();
            }
        }

        #endregion
    }

}
