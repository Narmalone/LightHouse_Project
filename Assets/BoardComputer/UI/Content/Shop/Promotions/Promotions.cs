using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class Promotions : MonoBehaviour
{
    public ScrollWindowController Scroller;
    public ShopCardPromotions Prefab;
    public RectTransform InstParent;
    public List<ShopCardPromotions> Controllers = new List<ShopCardPromotions>();
    public event Action<ShopCardPromotions> OnItemBuyCliqued;

    public List<ShopCardPromotions> FindItemByName(string name)
    {
        return Controllers.FindAll(x => x.ItemDataInstance.Name == name);
    }

    public List<ShopCardPromotions> FindItemByID(uint ID)
    {
        return Controllers.FindAll(x => x.ItemDataInstance.ItemID == ID);
    }

    public List<ShopCardPromotions> GenerateRandomPromotions(ShopItemConfig shopConfig)
    {
        int rdm = Random.Range(1, 3);
        if(Controllers.Count > 0)
            Controllers.ClearAndResetObjects();

        List<ShopItemData> possibleItems = new List<ShopItemData>();
        foreach(ShopItemData item in shopConfig.Items)
        {
            possibleItems.Add(item);
        }

        for (int i = 0; i < rdm; i++)
        {
            ShopCardPromotions controller = Instantiate(Prefab, InstParent);
            Controllers.Add(controller);

            int rdmItemInList = Random.Range(0, possibleItems.Count);
            ShopItemData rdmItem = possibleItems[rdmItemInList];
            controller.SetItemData(rdmItem);
            controller.SetItemState(ShopItemState.Promotion);

            float minPromotion = 15f;
            float maxPromotion = 50f;

            float promotionRandomPercentage = Random.Range(minPromotion, maxPromotion);
            int resultPromotionItemValue = Mathf.RoundToInt(rdmItem.BaseCost * (1 - promotionRandomPercentage / 100f));
            controller.ItemCost.text = resultPromotionItemValue.ToString();
            controller.CostWithPromotion = resultPromotionItemValue;

            //calculer la durée d'une promotion (rotation hebdo ? Quotidienne ?)

            controller.BuyButton.onClick.AddListener(() =>
            {
                OnItemBuyCliqued?.Invoke(controller);
                controller.OnItemAddedToCart();
            });

            possibleItems.RemoveAt(rdmItemInList);
        }
        Scroller.UpdateContentTransform();
        return Controllers;
    }
}
