using System;
using LightHouse.Inventory;

namespace LightHouse.Items.Interactable
{
    public class IDUseItemTracker : IDItemTracker
    {
        public Action OnConditionChecked;
        public Action OnItemUsedOnMe;
        public bool UnsubscribeToItemOnUse = true;
        protected IInventoryItemUsable _inventoryItemUsable;
        protected override void CheckConditions()
        {
            _hasKeyInInventory = SlotManager.IsItemExistInInventory((ushort)_itemNeeded);
            _hasKeyOnHands = SlotManager.IsItemOnHands((ushort)_itemNeeded, out IInventoryItem itm);

            if (_inventoryItemUsable != null)
            {
                ChangeCanBeUsedFromInventory(_inventoryItemUsable, false);
                UnsubscribeToItem(_inventoryItemUsable);
                _inventoryItemUsable = null;
            }

            if (!_hasKeyOnHands || itm == null)
            {
                OnConditionChecked?.Invoke();
                return;
            }

            if (itm is IInventoryItemUsable usable)
            {
                _inventoryItemUsable = usable;
                SubscribeToItem(_inventoryItemUsable);
                ChangeCanBeUsedFromInventory(_inventoryItemUsable, true);
            }
            OnConditionChecked?.Invoke();
        }

        protected virtual void Usable_OnItemUsed()
        {
            OnItemUsedOnMe?.Invoke();
            if (UnsubscribeToItemOnUse && _inventoryItemUsable != null)
            {
                ChangeCanBeUsedFromInventory(_inventoryItemUsable, false);
                UnsubscribeToItem(_inventoryItemUsable);
                _inventoryItemUsable = null;
            }
        }

        public override void OnRaycastEnd()
        {
            base.OnRaycastEnd();
            if (_inventoryItemUsable != null)
            {
                UnsubscribeToItem(_inventoryItemUsable);
                ChangeCanBeUsedFromInventory(_inventoryItemUsable, false);
                _inventoryItemUsable = null;
            }
        }

        public void ChangeCanBeUsedFromInventory(IInventoryItemUsable usable, bool value)
        {
            usable.CanBeUsedFromInventory = value;
            usable.InvokeOnCanBeUsedFromInventoryChanged();
        }

        protected virtual void SubscribeToItem(IInventoryItemUsable usable) => usable.OnItemUsed += Usable_OnItemUsed;
        protected virtual void UnsubscribeToItem(IInventoryItemUsable usable) => usable.OnItemUsed -= Usable_OnItemUsed;
    }
}

