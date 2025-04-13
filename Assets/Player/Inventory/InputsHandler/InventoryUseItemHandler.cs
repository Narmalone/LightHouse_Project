using LightHouse.Inputs;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LightHouse.Inventory
{
    public class InventoryUseItemHandler
    {
        private IInventoryItemUsable _usableItem;
        private InventoryUIController _inventoryUIController;

        public InventoryUseItemHandler(InventoryUIController inventoryUIController)
        {
            _inventoryUIController = inventoryUIController;
        }

        public void SetTarget(IInventoryItemUsable inventoryUsable)
        {
            _usableItem = inventoryUsable;
        }

        public bool UseItemFromInventory(IInventoryItemUsable usable)
        {
            if (usable == null || !usable.CanBeUsedFromInventory) return false;
            usable.UseFromInventory();
            return true;
        }

        public void HandeInteractInInventoryInput()
        {
            if (_usableItem == null) return;
            if (InputManager.InteractInInventory.IsPressed())
            {
                if (SlotManager.IsIndexInvalid(SlotManager.CurrentSlotIndex))
                    return;
                if (!_usableItem.CanBeUsedFromInventory) return;
                float holdvaluePercent = InputManager.InteractInInventory.GetTimeoutCompletionPercentage();
                _inventoryUIController.FillHoldedImage(holdvaluePercent);
            }
            else if (InputManager.InteractInInventory.WasReleasedThisFrame())
            {
                if (SlotManager.IsIndexInvalid(SlotManager.CurrentSlotIndex))
                    return;
                _inventoryUIController.FillHoldedImage(0.0f);
            }

            //in seperated if bcs it cannot be performed while pressed
            if (InputManager.InteractInInventory.WasPerformedThisFrame())
            {
                if (SlotManager.IsIndexInvalid(SlotManager.CurrentSlotIndex))
                    return;
                UseItemFromInventory(_usableItem);
                _inventoryUIController.FillHoldedImage(0.0f);
            }
        }
    }
}
