using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShopCartItem : ShopCardController
{
    public int StoredPrice;
    public Button RemoveFromCartButton;
    public ShopItemState BoughtFromItemState;
    public HorizontalLayoutGroup RightHorizontalLayout;
    public RectTransform RemoveButtonParent;

    public override void UpdateCardInfoFromItemData()
    {
        ItemName.text = ItemDataInstance.Name;
        ItemDescription.text = ItemDataInstance.Description;
        ItemCost.text = StoredPrice.ToString();
    }
}
