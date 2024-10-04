using MPUIKIT;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopCardController : MonoBehaviour
{
    public MPImageBasic ItemBackground;
    public MPImageBasic ItemIcon;
    public TextMeshProUGUI ItemName;
    public TextMeshProUGUI ItemDescription;
    public TextMeshProUGUI ItemCost;
    public TextMeshProUGUI dollarText;

    public ShopItemData ShopItemData;

    public void SetCardsInfos(string name, int cost)
    {
        ItemName.text = name;
        ItemCost.text = cost.ToString();
    }
}
