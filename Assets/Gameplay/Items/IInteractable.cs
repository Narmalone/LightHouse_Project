using LightHouse.Items;
using System;

namespace LightHouse.Interactions
{
    public interface IInteractable : IItemName
    {
        //Events
        public event Action OnObjectInteracted;
        public event Action OnInteractionNameChanged; //MUST BE CALLED ONLY WHEN WE ARE STILL RAYCASTING THE ITEM

        //if you want to do some conditions
        public bool CanBeInteracted { get; set; }

        string GetInteractionName();
        void Interact();
        
    }
}
