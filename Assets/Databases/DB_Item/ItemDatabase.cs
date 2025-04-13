using AYellowpaper.SerializedCollections;
using LightHouse.Inventory;
using UnityEngine;

[CreateAssetMenu(fileName = "ItemDB_", menuName = "New ItemDB")]
public class ItemDatabase : ScriptableObject
{
    public SerializedDictionary<ushort, GameObject> items = new SerializedDictionary<ushort, GameObject>();

    public IInventoryItem Get(ushort ID)
    {
        return items[ID].GetComponentInChildren<IInventoryItem>();
    }
}
