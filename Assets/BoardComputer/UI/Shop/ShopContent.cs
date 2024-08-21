using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ShopContent : ContentWindow
{
    [SerializeField] private TextMeshProUGUI playerCurrencyTxt;
    [SerializeField] private TextMeshProUGUI deliveryTimeTxt;
    [SerializeField] private ShopCardController shopCardPrefab;
    [SerializeField] private Transform shopCardsTransform;
    [SerializeField] private ShopItemConfig shopItemConfig;

    private List<ShopCardController> controllers = new List<ShopCardController>();

    private PlayerManager playerManagerInstance;

    private void Awake()
    {
        SortItems();
        InitShopCards();
    }

    private void Start()
    {
        SetDeliveryTime(shopItemConfig.AverageDeliveryTime);
        playerManagerInstance = PlayerManager.Instance;
        GetPlayerCurrency();
    }

    private void InitShopCards()
    {
        for (int i = 0; i < shopItemConfig.Items.Count; i++)
        {
            ShopCardController newItem = Instantiate(shopCardPrefab, shopCardsTransform);
            newItem.SetCardsInfos(shopItemConfig.Items[i].Name, shopItemConfig.Items[i].Cost);
            controllers.Add(newItem);
        }
    }

    private void SortItems()
    {
        switch (shopItemConfig.TriFunction)
        {
            case TriDisplay.CostAsc:
                shopItemConfig.Items.Sort((a, b) => a.Cost.CompareTo(b.Cost));
                break;
            case TriDisplay.CostDesc:
                shopItemConfig.Items.Sort((a, b) => b.Cost.CompareTo(a.Cost));
                break;
            case TriDisplay.NameAZ:
                shopItemConfig.Items.Sort((a, b) => a.Name.CompareTo(b.Name));
                break;
            case TriDisplay.NameZA:
                shopItemConfig.Items.Sort((a, b) => b.Name.CompareTo(a.Name));
                break;
        }
    }

    public void SetDeliveryTime(TimeDatas whenDeliv)
    {
        deliveryTimeTxt.text = $"Delivery Time: {whenDeliv.Hour} Hours.";
    }

    public void GetPlayerCurrency()
    {
        playerCurrencyTxt.text = "Currency: " + playerManagerInstance._data._currencyShop;
    }
}