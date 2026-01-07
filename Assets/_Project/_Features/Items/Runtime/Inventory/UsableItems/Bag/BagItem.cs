using LightHouse.Handlers;
using LightHouse.Inventory;
using System;
using UnityEngine;

public class BagItem : MonoBehaviour
{
    [SerializeField] private byte _additionalSlots = 2;

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.T))
        {
            OnEquipped();
        }
    }

    public void OnEquipped()
    {
        if(PlayerHandlerData.MainPlayer != null && InventoryHandlerData.IsInitialized)
        {
            Debug.Log("on equiped");
            InventoryUIController inventoryUI = PlayerHandlerData.MainPlayer.Inventory.InventoryUI;
            inventoryUI.AddItemToSlots(_additionalSlots, PlayerHandlerData.MainPlayer.Inventory.ItemDatabase);
        }
    }
}
