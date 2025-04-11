using LightHouse.Inputs;
using LightHouse.Interactions;
using UnityEngine;

public class RaycastInteractionHandler
{
    [SerializeField] private CanvasInteraction _interactionCanvas;

    private IInteractable currentInteractable;
    public bool HasTarget => currentInteractable != null;

    public RaycastInteractionHandler(CanvasInteraction interactionCanva)
    {
        _interactionCanvas = interactionCanva;
    }

    public void SetTarget(IInteractable interactable)
    {
        if (currentInteractable != null)
        {
            currentInteractable.OnInteractionNameChanged -= UpdateInteractionText;
            if (currentInteractable is IItemCallback interacCallback) interacCallback.OnRaycastEnd();
        }

        currentInteractable = interactable;

        if (currentInteractable == null)
        {
            _interactionCanvas.HideItemInteractionName();
            return;
        }

        if (interactable is IItemCallback interactableCallback) interactableCallback.OnRaycastStart();

        currentInteractable.OnInteractionNameChanged += UpdateInteractionText;
        UpdateInteractionText();
    }

    public void Update()
    {
        if (currentInteractable != null && currentInteractable.CanBeInteracted && currentInteractable.CanBeRaycasted &&
            InputManager.Interact.WasPerformedThisFrame())
        {
            currentInteractable.Interact();
        }
    }

    private void UpdateInteractionText()
    {
        if (currentInteractable == null) return;

        string name = currentInteractable.GetInteractionName();

        if (!currentInteractable.CanBeRaycasted || string.IsNullOrEmpty(name))
        {
            _interactionCanvas.HideItemInteractionName();
        }
        else
        {
            _interactionCanvas.ItemInteractionName_TMP.text = name;
            _interactionCanvas.ShowItemInteractionName();
        }
    }
}
