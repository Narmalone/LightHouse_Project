using UnityEngine;

[CreateAssetMenu(fileName = "", menuName = "InventoryItemData/NewItemData")]
public class InventoryItemData : ScriptableObject
{
    public Mesh mesh; // The 3D mesh of the item
    public Material material; // The material of the item
    public Sprite icon; // The icon of the item
    public string itemName; // The name of the item
    public string itemDescription; // The description of the item
}