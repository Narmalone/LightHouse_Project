using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New InventoryItem", menuName = "Items/NewInventoryItem")]
public class InventoryItemData : ScriptableObject
{
    public GameObject Prefab;
    public string Name;
}
