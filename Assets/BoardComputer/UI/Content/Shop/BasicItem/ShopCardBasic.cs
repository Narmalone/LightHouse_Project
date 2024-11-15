using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopCardBasic : ShopCardController
{
    public Button BuyButton;
    public TextMeshProUGUI NumberOfItemRemaining;

    public override void UpdateCardInfoFromItemData()
    {
        base.UpdateCardInfoFromItemData();
        NumberOfItemRemaining.text = "x" + ItemDataInstance.StockItems.ToString();
    }

    public void OnItemAddedToCart()
    {
        if (!BuyButton.interactable) return;
        ItemDataInstance.StockItems -= 1;
        if (ItemDataInstance.StockItems <= 0)
        {
            BuyButton.interactable = false;
        }
        NumberOfItemRemaining.text = "x" + ItemDataInstance.StockItems;
    }

    public void Refill()
    {

    }

    public void AddAndUpdateStock(int numberToAdded)
    {
        ItemDataInstance.StockItems += numberToAdded;
        NumberOfItemRemaining.text = "x" + ItemDataInstance.StockItems;
        if (ItemDataInstance.StockItems >= 0 && !BuyButton.interactable)
        {
            BuyButton.interactable = true;
        }
    }
}
