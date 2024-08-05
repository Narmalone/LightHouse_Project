using UnityEngine;

[CreateAssetMenu(fileName = "", menuName = "ItemDatas/NewItemData")]
public class ItemDatas : ScriptableObject
{
    public GameObject mesh; // The 3D mesh of the item
    public GameObject prefab; // The 3D mesh of the item
    public Sprite icon; // The icon of the item
    public string itemName; // The name of the item
    public string itemDescription; // The description of the item
}