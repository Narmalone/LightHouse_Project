using LightHouse.Items;
using System;

namespace LightHouse.Interactions
{
    public interface IInteractable : IItemName
    {
        //Events
        //MUST BE CALLED ONLY WHEN WE ARE STILL RAYCASTING THE ITEM
        public event Action OnObjectInteracted;
        public event Action OnInteractionNameChanged;

        /// <summary>
        /// If we can use the interaction Key to call Interact Method
        /// </summary>
        public bool CanBeInteracted { get; set; }

        /// <summary>
        /// Give the text to display when raycasting the item
        /// </summary>
        string GetInteractionName();

        /// <summary>
        /// When the player interacted with the item what should I do ?
        /// </summary>
        void Interact();
        
    }
}
