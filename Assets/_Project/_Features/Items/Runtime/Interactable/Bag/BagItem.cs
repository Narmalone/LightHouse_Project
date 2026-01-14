using LightHouse.Core.Player;
using LightHouse.Core.Player.Inventory;
using LightHouse.Core.Player.Inventory.UI;
using LightHouse.Features.Interactions;
using LightHouse.Features.Items.Detection;
using System;
using UnityEngine;

namespace LightHouse.Features.Items.Interactable.Bag
{
    public class BagItem : InteractableItemBase, IDestroyable, IItemCallback
    {
        [SerializeField] private byte _additionalSlots = 2;

        public event Action OnDestroyed;

        public override void Interact()
        {
            OnEquipped();
            InvokeObjectInteracted();
            //this.gameObject.SetActive(false);

            OnDestroyed?.Invoke();
            Destroy(this.gameObject);
        }

        public void OnEquipped()
        {
            if (PlayerHandlerData.MainPlayer != null && InventoryHandlerData.IsInitialized)
            {
                Debug.Log("on equiped");
                InventoryUIController inventoryUI = PlayerHandlerData.MainPlayer.Inventory.InventoryUI;
                inventoryUI.AddItemToSlots(_additionalSlots, PlayerHandlerData.MainPlayer.Inventory.ItemDatabase);
            }
        }

        public void OnRaycastStart()
        {
            if (!CanBeInteracted)
                InteractionText = "Cannot interact at the moment";
            else
                InteractionText = "PickUpBag";
        }
        public void OnRaycastEnd() { }
    }
}

