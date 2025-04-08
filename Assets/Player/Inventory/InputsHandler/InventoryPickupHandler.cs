using LightHouse.Inventory;
using UnityEngine;

public class InventoryPickupHandler
{
    private ItemSlot[] _slots;
    private byte _inventoryCapacity;

    public InventoryPickupHandler(ItemSlot[] slots, byte inventoryCapacity)
    {
        _slots = slots;
        _inventoryCapacity = inventoryCapacity;
    }

    public void PickupItem(int slotIndex, IInventoryItem item)
    {
        if (item == null) return;

        PoolManager.Add(item);

        ItemSlot targetSlot = null;

        if (IsSlotInvalidOrOccupied(slotIndex))
        {
            targetSlot = FindEmptySlot();
        }
        else
        {
            targetSlot = _slots[slotIndex];
        }

        item.IsItemInInventory = true;

        if (item is IInventoryStackable stackable)
        {
            if (TryFindStackableSlot(item.GlobalItemID, out ItemSlot existingSlot))
            {
                if (existingSlot.SlotDatas.TotalItemsInSlots < existingSlot.SlotDatas.MaxStack)
                    targetSlot = existingSlot;
            }

            if (targetSlot.SlotDatas.TotalItemsInSlots == 0)
                targetSlot.SlotDatas.MaxStack = stackable.MaxStack;
        }

        targetSlot.AddItemDatasToSlot(item);
    }

    private bool IsSlotInvalidOrOccupied(int index)
    {
        return index < 0 || index >= _inventoryCapacity || _slots[index].SlotDatas.HasItem;
    }

    private ItemSlot FindEmptySlot()
    {
        foreach (var slot in _slots)
        {
            if (!slot.SlotDatas.HasItem)
                return slot;
        }
        return null;
    }

    private bool TryFindStackableSlot(ushort itemID, out ItemSlot slot)
    {
        slot = null;
        foreach (var s in _slots)
        {
            if (s.SlotDatas.ItemGlobalID != itemID || !s.SlotDatas.CanStack()) continue;
            slot = s;
            return true;
        }
        return false;
    }
}
