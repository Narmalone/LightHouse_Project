using LightHouse.Inventory;
using System.Collections.Generic;
using UnityEngine;

public static class PoolManager
{
    public static Dictionary<ushort, List<IInventoryItem>> InventoryItemPools = new();
    private static Dictionary<ushort, ushort> _specificIdCounters = new(); //Needed to make a proper count and set of specific id's taking in count dropping etc...

    public static bool GetWithoutRemovingFromPool(ushort globalID, ushort itemSpecifcID, out IInventoryItem item)
    {
        item = null;
        if (!InventoryItemPools.ContainsKey(globalID)) return false;
        if (InventoryItemPools[globalID].Count == 0) return false;
        item = InventoryItemPools[globalID].Find(x => x.ItemSpecificID == itemSpecifcID);
        if (item == null) return false;
        return true;
    }

    public static IInventoryItem Get(ushort globalID, ushort itemSpecificID, bool enablePhysicsOnGet)
    {
        if (!InventoryItemPools.ContainsKey(globalID)) return null;
        if (InventoryItemPools[globalID].Count == 0) return null;

        IInventoryItem findedItem = null;
        //findedItem = InventoryItemPools[globalID][0];
        findedItem = InventoryItemPools[globalID].Find(x => x.ItemSpecificID == itemSpecificID);

        if (findedItem != null)
        {
            EnableInventoryItem(findedItem, enablePhysicsOnGet);
            InventoryItemPools[globalID].Remove(findedItem);
        }

        if (InventoryItemPools[globalID].Count <= 0) InventoryItemPools.Remove(globalID);
        return findedItem;
    }

    public static void Add(IInventoryItem item)
    {
        if (!InventoryItemPools.ContainsKey(item.GlobalItemID))
        {
            InventoryItemPools.Add(item.GlobalItemID, new List<IInventoryItem>());
        }

        InventoryItemPools[item.GlobalItemID].Add(item);
        if (!_specificIdCounters.ContainsKey(item.GlobalItemID))
            _specificIdCounters[item.GlobalItemID] = 0;

        item.ItemSpecificID = _specificIdCounters[item.GlobalItemID];
        _specificIdCounters[item.GlobalItemID]++;
        DisableInventoryItem(item);
       
    }

    public static void DisableInventoryItem(IInventoryItem item)
    {
        item.GetGameObject().SetActive(false);
        item.GetCollider().enabled = false;
        item.GetRigidBody().isKinematic = true;
    }

    public static void EnableInventoryItem(IInventoryItem item, bool enablePhysics)
    {
        if (item == null) return;
        item.GetGameObject().SetActive(true);
        if (!enablePhysics) return;
        item.GetCollider().enabled = true;
        item.GetRigidBody().isKinematic = false;
    }

    public static void Clear()
    {
        InventoryItemPools.Clear();
        _specificIdCounters.Clear();
    }
}
