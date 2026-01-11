using LightHouse.Features.Interactions;
using LightHouse.Features.Items.Interactable;
using UnityEngine;

namespace LightHouse.Features.Computer
{
    public class InteractableComputer : InteractableItemBase, IInteractable
    {
        public bool Enabled = false;
        public Collider Collider => _detectionCollider;
        public override string GetInteractionName()
        {
            return "Press to enter in the computer";
        }

        public override void Interact()
        {
            InvokeObjectInteracted();
        }
    }

}
