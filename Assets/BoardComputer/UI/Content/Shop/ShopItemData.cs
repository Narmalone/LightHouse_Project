using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "ShopItem")]
public class ShopItemData : ScriptableObject
{
    public string Name;
    public string Description;
    public int Cost;
    public ItemBase Prefab;
}
