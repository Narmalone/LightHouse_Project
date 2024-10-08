using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopCardPromotions : ShopCardController
{
    public Button BuyButton;
    public TextMeshProUGUI OldCostTxt;
    public TextMeshProUGUI NumberOfItemRemaining;

    public override void UpdateCardInfoFromItemData()
    {
        NumberOfItemRemaining.text = "x" + ItemDataInstance.StockItems.ToString();
        ItemName.text = ItemDataInstance.Name;
        ItemDescription.text = ItemDataInstance.Description;
        OldCostTxt.text = ItemDataInstance.BaseCost.ToString();
    }
}
