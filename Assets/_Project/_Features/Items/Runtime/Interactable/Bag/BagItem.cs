using LightHouse.Core.Player;
using LightHouse.Core.Player.Inventory;
using LightHouse.Core.Player.Inventory.UI;
using UnityEngine;

namespace LightHouse.Features.Items.Interactable.Bag
{
    public class BagItem : InteractableItemBase
    {
        [SerializeField] private byte _additionalSlots = 2;

        public override string GetInteractionName()
        {
            return "Press E to grab";
        }

        public override void Interact()
        {
            OnEquipped();
            InvokeObjectInteracted();
            this.gameObject.SetActive(false);
            
        }

        private void OnDisable()
        {
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
    }
}

