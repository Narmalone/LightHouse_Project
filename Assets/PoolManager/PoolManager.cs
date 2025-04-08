using LightHouse.Inventory;
using System.Collections.Generic;

public static class PoolManager
{
    public static Dictionary<uint, List<IInventoryItem>> InventoryItemPools = new();
    public static IInventoryItem Get(IInventoryItem inventoryItem)
    {
        if (!InventoryItemPools.ContainsKey(inventoryItem.ID)) return null;
        if(InventoryItemPools[inventoryItem.ID].Count == 0) return null;
        Remove(InventoryItemPools[inventoryItem.ID][0]);
        return InventoryItemPools[inventoryItem.ID][0];
    }

    public static void Add(IInventoryItem item)
    {
        if (!InventoryItemPools.ContainsKey(item.ID))
        {
            InventoryItemPools.Add(item.ID, new List<IInventoryItem>());
        }
        DisableInventoryItem(item);
        InventoryItemPools[item.ID].Add(item);
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

    public static void Remove(IInventoryItem inventoryItem)
    {
        if (InventoryItemPools.ContainsKey(inventoryItem.ID))
        {
            InventoryItemPools[inventoryItem.ID].Remove(inventoryItem);
            if (InventoryItemPools[inventoryItem.ID].Count == 0) InventoryItemPools.Remove(inventoryItem.ID);
        }
    }
}
