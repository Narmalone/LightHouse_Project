using AYellowpaper.SerializedCollections;
using LightHouse.Inventory;
using UnityEngine;

[CreateAssetMenu(fileName = "ItemDB_", menuName = "LightHouse/Databases/InventoryItems")]
public class ItemDatabase : ScriptableObject
{
    public SerializedDictionary<ushort, GameObject> items = new SerializedDictionary<ushort, GameObject>();

    public IInventoryItem Get(ushort ID)
    {
        return items[ID].GetComponentInChildren<IInventoryItem>();
    }

    public GameObject GetPrefab(ushort ID)
    {
        return items[ID];
    }
}
