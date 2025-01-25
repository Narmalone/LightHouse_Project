using UnityEngine;

[CreateAssetMenu(fileName = "", menuName = "ItemDatas/NewItemData")]
public class ItemDatas : ScriptableObject
{
    public GameObject prefab; // The 3D mesh of the item
    public Sprite icon; // The icon of the item
}