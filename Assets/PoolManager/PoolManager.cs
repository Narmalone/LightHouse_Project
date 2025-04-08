using LightHouse.Inventory;
using System.Collections.Generic;
using UnityEngine;

public static class PoolManager
{
    public static Dictionary<ushort, List<IInventoryItem>> InventoryItemPools = new();

    public static IInventoryItem Get(ushort globalID)
    {
        if (!InventoryItemPools.ContainsKey(globalID)) return null;
        if (InventoryItemPools[globalID].Count == 0) return null;

        IInventoryItem findedItem = null;
        findedItem = InventoryItemPools[globalID][0];

        if (findedItem != null)
        {
            EnableInventoryItem(findedItem);
            InventoryItemPools[globalID].Remove(findedItem);
        }
        return findedItem;
    }

    public static void Add(IInventoryItem item)
    {
        if (!InventoryItemPools.ContainsKey(item.GlobalItemID))
        {
            InventoryItemPools.Add(item.GlobalItemID, new List<IInventoryItem>());
        }
        DisableInventoryItem(item);
        InventoryItemPools[item.GlobalItemID].Add(item);
        item.ItemSpecificID = (ushort)InventoryItemPools[item.GlobalItemID].IndexOf(item);
    }

    private static void DisableInventoryItem(IInventoryItem item)
    {
        item.GetGameObject().SetActive(false);
        item.GetCollider().enabled = false;
    }

    private static void EnableInventoryItem(IInventoryItem item)
    {
        item.GetGameObject().SetActive(true);
        item.GetCollider().enabled = true;
    }

    public static void RemoveFromPool(ushort itemGlobalID, ushort itemSpecificID)
    {
        IInventoryItem removedItem = null;
        if (InventoryItemPools.ContainsKey(itemGlobalID))
        {
            removedItem = InventoryItemPools[itemGlobalID].Find(x => x.ItemSpecificID == itemSpecificID);
            if (InventoryItemPools[itemGlobalID].Count == 0) InventoryItemPools.Remove(itemGlobalID);
        }

        if(removedItem != null)
            EnableInventoryItem(removedItem);
    }
}
