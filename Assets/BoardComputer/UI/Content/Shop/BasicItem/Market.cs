using System;
using System.Collections.Generic;
using UnityEngine;

public class Market : MonoBehaviour
{
    public ScrollWindowController Scroller;
    public List<ShopCardBasic> Controllers = new List<ShopCardBasic>();
    public ShopCardBasic Prefab;
    public RectTransform InstParent;
    public event Action<ShopCardBasic> OnItemBuyCliqued;

    public List<ShopCardBasic> FindItemByName(string name)
    {
        return Controllers.FindAll(x => x.ItemDataInstance.Name == name);
    }

    public List<ShopCardBasic> FindItemByID(uint ID)
    {
        return Controllers.FindAll(x => x.ItemDataInstance.ItemID == ID);
    }

    public List<ShopCardBasic> GenerateMarketItems(ShopItemConfig shopConfig)
    {
        if (Controllers.Count > 0)
            Controllers.ClearAndResetObjects();

        for (int i = 0; i < shopConfig.Items.Count; i++)
        {
            ShopCardBasic controller = Instantiate(Prefab, InstParent);
            Controllers.Add(controller);

            controller.SetItemData(shopConfig.Items[i]);
            controller.SetItemState(ShopItemState.MainMarket);

            controller.BuyButton.onClick.AddListener(() =>
            {
                OnItemBuyCliqued?.Invoke(controller);
                controller.OnItemAddedToCart();
            });
        }
        Scroller.UpdateContentTransform();
        return Controllers;
    }
}
