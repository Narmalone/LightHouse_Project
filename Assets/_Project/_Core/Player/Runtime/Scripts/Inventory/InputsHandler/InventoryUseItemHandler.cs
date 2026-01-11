using LightHouse.Core.Inputs;
using LightHouse.Core.Player.Inventory.UI;
using LightHouse.Features.Items.Inventory;
using UnityEngine;

namespace LightHouse.Core.Player.Inventory.InputsHandler
{
    public class InventoryUseItemHandler
    {
        private IInventoryItemUsable _usableItem;
        private InventoryUIController _inventoryUIController;
        
        private float _currentHoldValue;
        private bool _hasBeenPerformed;
        private bool _isHolding = false;
        public bool IsHolding => _isHolding;
        public bool HasBeenPerformed => _hasBeenPerformed;

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
                {
                    SetTarget(usable);
                    _hasBeenPerformed = false;
                    ResetHoldValue();
                }
            }
        }
        
        public void UpdateHoldValue()
        {
            _currentHoldValue = _currentHoldValue + Time.deltaTime;
        }

        public void ResetHoldValue()
        {
            _currentHoldValue = 0.0f;
        }

        public float GetPercentHoldValue(float currentHoldValue)
        {
            return currentHoldValue / _usableItem.UseHoldTime;
        }
        
        public void Canceled()
        {
            _inventoryUIController.FillInteractHoldedImage(0.0f);
            _isHolding = false;
            _hasBeenPerformed = false;

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
                {
                    SetTarget(null);
                }
            }
        }

        public void HandeInteractInInventoryInput()
        {
            if (_usableItem == null) return;
            if (_hasBeenPerformed) return;
            if (InputManager.InteractInInventory.IsPressed())
            {
                _isHolding = true;
                UpdateHoldValue();
                if (SlotManager.IsIndexInvalid(SlotManager.CurrentSlotIndex))
                    return;
                if (!_usableItem.CanBeUsedFromInventory) return;
                float holdvaluePercent = GetPercentHoldValue(_currentHoldValue); //InputManager.InteractInInventory.GetTimeoutCompletionPercentage();
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
                _isHolding = false;
            }

            //in seperated if bcs it cannot be performed while pressed
            if (_currentHoldValue >= _usableItem.UseHoldTime) /*InputManager.InteractInInventory.WasPerformedThisFrame()*/
            {
                if (SlotManager.IsIndexInvalid(SlotManager.CurrentSlotIndex))
                    return;
                UseItemFromInventory(_usableItem);
                _inventoryUIController.FillInteractHoldedImage(0.0f);
                _hasBeenPerformed = true;
                _isHolding = false;
            }
        }
    }
}
