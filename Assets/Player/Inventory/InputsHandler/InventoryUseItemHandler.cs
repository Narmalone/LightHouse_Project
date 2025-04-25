using LightHouse.Inputs;
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

        public void Started()
        {
            if (InventoryHandlerData.IsGrabbingObjectOrIndexInvalid())
                return;

            if (SlotManager.CurrentSelectedSlot.SlotDatas.GetFirstItemInSlot(out IInventoryItem item))
            {
                if (item is IInventoryItemUsable usable)
                    SetTarget(usable);
            }
        }

        public void Canceled()
        {
            _inventoryUIController.FillInteractHoldedImage(0.0f);

            if (InventoryHandlerData.IsGrabbingObjectOrIndexInvalid())
                return;

            if (!SlotManager.CurrentSelectedSlot.SlotDatas.HasItem)
            {
                SetTarget(null);
                return;
            }

            if (SlotManager.CurrentSelectedSlot.SlotDatas.GetFirstItemInSlot(out IInventoryItem item))
            {
                if (item is IInventoryItemUsable)
                    SetTarget(null);
            }
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
                if (holdvaluePercent < 1f)
                    _inventoryUIController.FillInteractHoldedImage(holdvaluePercent);
                else
                    _inventoryUIController.FillInteractHoldedImage(0.0f);
            }
            else if (InputManager.InteractInInventory.WasReleasedThisFrame())
            {
                if (SlotManager.IsIndexInvalid(SlotManager.CurrentSlotIndex))
                    return;
                _inventoryUIController.FillInteractHoldedImage(0.0f);
            }

            //in seperated if bcs it cannot be performed while pressed
            if (InputManager.InteractInInventory.WasPerformedThisFrame())
            {
                if (SlotManager.IsIndexInvalid(SlotManager.CurrentSlotIndex))
                    return;
                UseItemFromInventory(_usableItem);
                _inventoryUIController.FillInteractHoldedImage(0.0f);
            }
        }
    }
}
