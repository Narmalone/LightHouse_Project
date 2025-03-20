using LightHouse.Items;
using System;

namespace LightHouse.Interactions
{
    public interface IInteractable : IItemName
    {
        //Events
        public event Action OnObjectInteracted;
        public event Action OnInteractionNameChanged;

        //if you want to do some conditions
        public bool CanBeInteracted { get; set; }

        string GetInteractionName();
        void Interact();
    }
}
