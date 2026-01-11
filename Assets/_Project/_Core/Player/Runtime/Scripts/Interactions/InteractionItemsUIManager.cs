using LightHouse.Features.Items.Detection;
using LightHouse.Features.Interactions;
using LightHouse.Features.Interactions.UI;

using System;
using UnityEngine;

namespace LightHouse.Core.Player.Interactions.UI
{
    /// <summary>
    /// Handles interactions (not name display) with objects in the world.
    /// </summary>
    public class InteractionItemsUIManager : MonoBehaviour
    {
        public static Action ForceUpdateTarget;
        #region FIELDS
        [Header("Settings")]
        [SerializeField] private CanvasInteraction _interactionCanvas;

        //controllers s
        private RaycastInteractionHandler _interactionHandler;
        [SerializeField] private ItemsDetectionSystem _unifiedRaycastSystem;
        private RaycastDetector<IInteractable> _raycastInteractable;
        #endregion

        #region MONO CALLBACKS
        private void Awake()
        {
            _interactionHandler = new RaycastInteractionHandler(_interactionCanvas);
        }

        private void Start()
        {
            _raycastInteractable = _unifiedRaycastSystem.InteractableDetector;
            _raycastInteractable.OnDetected += interactable => _interactionHandler.SetTarget(interactable);
            _raycastInteractable.OnItemDestroyed += () => _interactionHandler.SetTarget(null);
            _raycastInteractable.OnItemLost += () => _interactionHandler.SetTarget(null);
        }

        private void Update()
        {
            _interactionHandler.Update(); // For UI animations, etc.
        }

        private void OnDestroy()
        {
            _raycastInteractable.OnDetected -= interactable => _interactionHandler.SetTarget(interactable);
            _raycastInteractable.OnItemLost -= () => _interactionHandler.SetTarget(null);
            _interactionHandler.OnDestroyed();
        }
        #endregion

        #region UI
        public void Enable()
        {
            _interactionCanvas.gameObject.SetActive(true);
        }

        public void Disable()
        {
            _interactionCanvas.gameObject.SetActive(false);
        }

        #endregion
    }
}
