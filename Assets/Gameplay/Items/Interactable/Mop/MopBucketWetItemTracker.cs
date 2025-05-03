using LightHouse.Inventory;
using LightHouse.Items.Inventory;

namespace LightHouse.Items.Interactable
{
    public class MopBucketWetItemTracker : IDUseItemTracker
    {
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
                return;
            if (itm is IInventoryItemUsable usable)
            {
                _inventoryItemUsable = usable;
                if(_inventoryItemUsable is Mop mop)
                {
                    if (mop.IsWet)
                    {
                        return;
                    }
                }

                SubscribeToItem(_inventoryItemUsable);
                ChangeCanBeUsedFromInventory(_inventoryItemUsable, true);
            }
        }

        protected override void Usable_OnItemUsed()
        {
            if(_inventoryItemUsable is Mop mop)
            {
                mop.MakeMeWet();
            }
        }
    }

}
