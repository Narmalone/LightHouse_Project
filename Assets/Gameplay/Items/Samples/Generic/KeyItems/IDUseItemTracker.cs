using LightHouse.Inventory;
using UnityEngine;

public class IDUseItemTracker : IDItemTracker
{
    protected IInventoryItemUsable _inventoryItemUsable;
    protected override void CheckConditions()
    {
        _hasKeyInInventory = InventoryHandlerData.TryFindItem((ushort)_itemNeeded, out IInventoryItem item);
        _hasKeyOnHands = InventoryHandlerData.IsItemOnHands((ushort)_itemNeeded, out IInventoryItem itm);

        if (_inventoryItemUsable != null && !_hasKeyOnHands)
        {
            ChangeCanBeUsedFromInventory(_inventoryItemUsable, false);
            UnsubscribeToItem(_inventoryItemUsable);
            _inventoryItemUsable = null;
        }

        InvokeNameUpdated();
        InvokeInteractionDescriptionUpdated();

        if (itm == null) return;
        if (itm is IInventoryItemUsable usable)
        {
            _inventoryItemUsable = usable;
            SubscribeToItem(_inventoryItemUsable);
            if (_hasKeyOnHands)
                ChangeCanBeUsedFromInventory(_inventoryItemUsable, true);
        }
    }

    protected virtual void Usable_OnItemUsed()
    {
        if (_inventoryItemUsable != null) 
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

    public void SubscribeToItem(IInventoryItemUsable usable) => usable.OnItemUsed += Usable_OnItemUsed;
    public void UnsubscribeToItem(IInventoryItemUsable usable) => usable.OnItemUsed -= Usable_OnItemUsed;
}
