using LightHouse.Inventory;
using System.Collections.Generic;

public static class PoolManager
{
    public static Dictionary<IInventoryItem, List<IInventoryItem>> InventoryItemPools = new();
    private static IInventoryItem Get(IInventoryItem inventoryItem)
    {
        if (!InventoryItemPools.ContainsKey(inventoryItem)) return null;
        if(InventoryItemPools[inventoryItem].Count == 0) return null;
        Remove(InventoryItemPools[inventoryItem][0]);
        return InventoryItemPools[inventoryItem][0];
    }

    public static void Add(IInventoryItem inventoryItem)
    {
        if (!InventoryItemPools.ContainsKey(inventoryItem))
        {
            InventoryItemPools.Add(inventoryItem, new List<IInventoryItem>());
        }

        DisableInventoryItem(inventoryItem);
        InventoryItemPools[inventoryItem].Add(inventoryItem);
    }

    private static void DisableInventoryItem(IInventoryItem item)
    {
        item.GetGameObject().SetActive(false);
        item.GetCollider().enabled = false;
        //rest
    }

    private static void EnableInventoryItem(IInventoryItem item)
    {

    }

    public static void Remove(IInventoryItem inventoryItem)
    {
        if (InventoryItemPools.ContainsKey(inventoryItem))
        {
            InventoryItemPools[inventoryItem].Remove(inventoryItem);
        }
    }
}
