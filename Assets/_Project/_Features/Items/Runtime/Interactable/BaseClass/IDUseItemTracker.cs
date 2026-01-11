using LightHouse.Core.Player.Inventory;
using LightHouse.Features.Items.Inventory;
using System;

namespace LightHouse.Features.Items.Interactable
{
    public class IDUseItemTracker : IDItemTracker
    {
        public Action OnConditionChecked;
        public Action OnItemUsedOnMe;
        public bool AutoSubscribeOnSeen = true;
        public bool AutoUnsubscribeOnItemUsed = true;
        protected IInventoryItemUsable _inventoryItemUsable;
        protected override bool CheckConditions()
        {
            _hasKeyInInventory = SlotManager.IsItemExistInInventory((ushort)_itemNeeded);
            _hasKeyOnHands = SlotManager.IsItemOnHands((ushort)_itemNeeded, out IInventoryItem itm);

            if (_inventoryItemUsable != null)
            {
                UnsubscribeFromCheckCondition();
                _inventoryItemUsable = null;
            }

            if (!_hasKeyOnHands || itm == null)
            {
                OnConditionChecked?.Invoke();
                return false;
            }

            if (itm is IInventoryItemUsable usable)
            {
                _inventoryItemUsable = usable;
                if (AutoSubscribeOnSeen)
                    SubscribeFromCheckCondition();
            }
            OnConditionChecked?.Invoke();
            return true;
        }

        protected virtual void SubscribeFromCheckCondition()
        {
            SubscribeToItem(_inventoryItemUsable);
            ChangeCanBeUsedFromInventory(_inventoryItemUsable, true);
        }

        protected virtual void UnsubscribeFromCheckCondition()
        {
            ChangeCanBeUsedFromInventory(_inventoryItemUsable, false);
            UnsubscribeToItem(_inventoryItemUsable);
        }

        protected virtual void Usable_OnItemUsed()
        {
            OnItemUsedOnMe?.Invoke();
            if (AutoUnsubscribeOnItemUsed && _inventoryItemUsable != null)
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

