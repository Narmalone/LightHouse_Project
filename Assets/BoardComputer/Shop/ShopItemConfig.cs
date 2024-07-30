using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "ShopConfig")]
public class ShopItemConfig : ScriptableObject
{
    public List<ShopItemData> Items;
    public TriDisplay TriFunction;
    public TimeDatas MinDeliveryTime;
    public TimeDatas AverageDeliveryTime;
    public TimeDatas MaxDeliveryTime;
}

public enum TriDisplay
{
    CostAsc,
    CostDesc,
    NameAZ,
    NameZA
}