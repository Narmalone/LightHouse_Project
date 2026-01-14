using LightHouse.Features.Interactions;
using LightHouse.Features.Items.Interactable;
using UnityEngine;

namespace LightHouse.Features.Computer
{
    public class InteractableComputer : InteractableItemBase, IInteractable
    {
        public bool Enabled = false;
        public Collider Collider => _detectionCollider;

        public override void Interact()
        {
            InvokeObjectInteracted();
        }
    }

}
