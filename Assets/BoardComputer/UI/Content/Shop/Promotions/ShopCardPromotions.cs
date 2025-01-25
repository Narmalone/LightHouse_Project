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
    public int CostWithPromotion;

    public override void UpdateCardInfoFromItemData()
    {
        NumberOfItemRemaining.text = "x" + ItemDataInstance.StockItems.ToString();
        ItemName.text = ItemDataInstance.Name;
        ItemDescription.text = ItemDataInstance.Description;
        OldCostTxt.text = ItemDataInstance.BaseCost.ToString();
    }

    public void OnItemAddedToCart()
    {
        ItemDataInstance.StockItems -= 1;
        if (ItemDataInstance.StockItems <= 0)
        {
            BuyButton.interactable = false;
        }
        NumberOfItemRemaining.text = "x" + ItemDataInstance.StockItems;
    }

    public void AddAndUpdateStock(int numberToAdded)
    {
        ItemDataInstance.StockItems += numberToAdded;
        NumberOfItemRemaining.text = "x" + ItemDataInstance.StockItems;
        if (ItemDataInstance.StockItems > 0 && !BuyButton.interactable)
        {
            BuyButton.interactable = true;
        }
    }
}
