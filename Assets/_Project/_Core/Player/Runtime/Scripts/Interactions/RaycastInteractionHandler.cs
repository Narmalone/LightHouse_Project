using UnityEngine;
using LightHouse.Inputs;
using LightHouse.Interactions;

namespace LightHouse.Interactions
{
    public class RaycastInteractionHandler
    {
        private CanvasInteraction _interactionCanvas;

        private IInteractable _currentInteractable;
        public bool HasTarget => _currentInteractable != null;

        public RaycastInteractionHandler(CanvasInteraction interactionCanva)
        {
            _interactionCanvas = interactionCanva;
        }

        public void SetTarget(IInteractable interactable)
        {
            if (_currentInteractable != null)
            {
                _currentInteractable.OnInteractionNameChanged -= UpdateInteractionText;
            }

            _currentInteractable = interactable;

            if (_currentInteractable == null)
            {
                _interactionCanvas.HideItemInteractionName();
                return;
            }

            _currentInteractable.OnInteractionNameChanged += UpdateInteractionText;
            UpdateInteractionText();
        }

        public void Update()
        {
            if (_currentInteractable != null && _currentInteractable.CanBeInteracted && _currentInteractable.CanBeRaycasted &&
                InputManager.Interact.WasPerformedThisFrame())
            {
                _currentInteractable.Interact();
            }
        }

        private void UpdateInteractionText()
        {
            if (_currentInteractable == null) return;

            string name = _currentInteractable.GetInteractionName();

            if (!_currentInteractable.CanBeRaycasted || string.IsNullOrEmpty(name))
            {
                _interactionCanvas.HideItemInteractionName();
            }
            else
            {
                _interactionCanvas.ItemInteractionName_TMP.text = name;
                _interactionCanvas.ShowItemInteractionName();
            }
        }

        public void OnDestroyed()
        {
            if (_currentInteractable != null)
            {
                _currentInteractable.OnInteractionNameChanged -= UpdateInteractionText;
            }
        }
    }
}

