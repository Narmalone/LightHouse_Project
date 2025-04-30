using UnityEngine;
using LightHouse.Items.Detection;
using LightHouse.Interactions;
using System;

namespace LightHouse.KinematicCharacterController
{
    /// <summary>
    /// Handles interactions (not name display) with objects in the world.
    /// </summary>
    public class PlayerInteractableManager : MonoBehaviour
    {
        public static Action ForceUpdateTarget;
        #region FIELDS
        [Header("Settings")]
        [SerializeField] private CanvasInteraction _interactionCanvas;

        //controllers s
        private RaycastInteractionHandler _interactionHandler;
        [SerializeField] private UnifiedRaycastSystem _unifiedRaycastSystem;
        private RaycastDetector<IInteractable> _raycastInteractable;
        #endregion

        #region MONO CALLBACKS
        private void Awake()
        {
            _interactionHandler = new RaycastInteractionHandler(_interactionCanvas);

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
        }
        #endregion
    }
}
