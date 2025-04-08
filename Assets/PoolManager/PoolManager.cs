using LightHouse.Inventory;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class PoolManager
{
    public static Dictionary<ushort, List<IInventoryItem>> InventoryItemPools = new();

    public static IInventoryItem GetSpecificItem(ushort globalID, ushort specificID)
    {
        if (!InventoryItemPools.ContainsKey(globalID)) return null;
        if (InventoryItemPools[globalID].Count == 0) return null;
        IInventoryItem findedItem = InventoryItemPools[globalID].Find(x => x.SpecificID == specificID);

        if (findedItem != null)
        {
            Debug.Log(findedItem);
            //RemoveFromPool(findedItem);
        }
        return findedItem;
    }

    public static void Add(IInventoryItem item)
    {
        if (!InventoryItemPools.ContainsKey(item.ID))
        {
            InventoryItemPools.Add(item.ID, new List<IInventoryItem>());
        }
        DisableInventoryItem(item);
        InventoryItemPools[item.ID].Add(item);
        item.SpecificID = (ushort)InventoryItemPools[item.ID].IndexOf(item);
        Debug.Log("Item added to pool");
    }

    private static void DisableInventoryItem(IInventoryItem item)
    {
        item.GetGameObject().SetActive(false);
        item.GetCollider().enabled = false;
        //rest
    }

    private static void EnableInventoryItem(IInventoryItem item)
    {
        item.GetGameObject().SetActive(true);
        item.GetCollider().enabled = true;
    }

    public static void RemoveFromPool(ushort itemID, ushort itemGlobalID)
    {
        if (InventoryItemPools.ContainsKey(itemID))
        {
            //InventoryItemPools[itemID].Remove(inventoryItem);
            if (InventoryItemPools[itemID].Count == 0) InventoryItemPools.Remove(itemID);
        }
        //EnableInventoryItem(inventoryItem);
    }
}
