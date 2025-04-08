using AYellowpaper.SerializedCollections;
using LightHouse.Inventory;
using UnityEngine;

[CreateAssetMenu(fileName = "ItemDB_", menuName = "New ItemDB")]
public class ItemDatabase : ScriptableObject
{
    public SerializedDictionary<uint, GameObject> items = new SerializedDictionary<uint, GameObject>();

    public IInventoryItem Get(uint ID)
    {
        items[ID].TryGetComponent(out IInventoryItem item);
        return item;
    }
    
}
