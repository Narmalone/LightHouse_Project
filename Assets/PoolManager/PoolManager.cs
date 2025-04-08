using LightHouse.Inventory;
using System.Collections.Generic;
using UnityEngine;

public enum SortingType
{
    None,
    Random,
    Target
}

public struct SortingResult
{
    public SortingType SortType;
    public ushort targetID;

    public SortingResult(SortingType sortType, ushort targetID)
    {
        this.SortType = sortType;
        this.targetID = targetID;
    }
}

public static class PoolManager
{
    public static Dictionary<ushort, List<IInventoryItem>> InventoryItemPools = new();

    public static IInventoryItem Get(ushort globalID, SortingResult sortingMode)
    {
        if (!InventoryItemPools.ContainsKey(globalID)) return null;
        if (InventoryItemPools[globalID].Count == 0) return null;

        IInventoryItem findedItem = null;
        switch (sortingMode.SortType)
        {
            case SortingType.None:
                findedItem = InventoryItemPools[globalID][0];
                break;
            case SortingType.Random:
                break;
            case SortingType.Target:
                findedItem = InventoryItemPools[globalID].Find(x => x.SpecificID == sortingMode.targetID);
                break;
        }

        if (findedItem != null)
        {
            EnableInventoryItem(findedItem);
            InventoryItemPools[globalID].Remove(findedItem);
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

    public static void RemoveFromPool(ushort itemGlobalID, ushort itemSpecificID)
    {
        IInventoryItem removedItem = null;
        if (InventoryItemPools.ContainsKey(itemGlobalID))
        {
            removedItem = InventoryItemPools[itemGlobalID].Find(x => x.SpecificID == itemSpecificID);
            if (InventoryItemPools[itemGlobalID].Count == 0) InventoryItemPools.Remove(itemGlobalID);
        }

        if(removedItem != null)
            EnableInventoryItem(removedItem);
    }
}
