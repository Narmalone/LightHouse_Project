using LightHouse.Handlers;
using LightHouse.Inventory;
using LightHouse.Items.Interactable;
using System;
using UnityEngine;

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
