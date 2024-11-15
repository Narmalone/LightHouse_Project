using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ShopContent : ContentWindow
{
    [SerializeField] private TextMeshProUGUI playerCurrencyTxt;
    [SerializeField] private TextMeshProUGUI deliveryTimeTxt;
    [SerializeField] private ShopItemConfig shopItemConfig;
    public Promotions PromotionsController;
    public Market MarketController;
    public Cart CartController;
    public Commands CommandsController;
    [SerializeField] private CustomEvent_Float _onPlayerCurrencyChanged;

    private PlayerManager playerManagerInstance;

    private void Awake()
    {
        _onPlayerCurrencyChanged.handle += _onPlayerCurrencyChanged_handle;
        PromotionsController.OnItemBuyCliqued += PromotionsController_OnItemBuyCliqued;
        MarketController.OnItemBuyCliqued += MarketController_OnItemBuyCliqued;
        CartController.CommandButton.onClick.AddListener(OnCommandCartCliqued);
    }

    private void Start()
    {
        playerManagerInstance = PlayerManager.Instance;
        PromotionsController.GenerateRandomPromotions(shopItemConfig);
        MarketController.GenerateMarketItems(shopItemConfig);

        UpdatePlayerCurrencyText(playerManagerInstance._data.CurrencyShop);
        CartController.SetCartPriceText(0);
        PlayerValidateCartButtonCheck();
        //SetDeliveryTime(shopItemConfig.AverageDeliveryTime);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.I))
        {
            playerManagerInstance._data.CurrencyShop += 50f;
        }
    }

    private void OnDestroy()
    {
        _onPlayerCurrencyChanged.handle -= _onPlayerCurrencyChanged_handle;
        PromotionsController.OnItemBuyCliqued -= PromotionsController_OnItemBuyCliqued;
        MarketController.OnItemBuyCliqued -= MarketController_OnItemBuyCliqued;
        CartController.CommandButton.onClick.RemoveAllListeners();
    }

    private void SortItems()
    {
        switch (shopItemConfig.TriFunction)
        {
            case TriDisplay.CostAsc:
                shopItemConfig.Items.Sort((a, b) => a.BaseCost.CompareTo(b.BaseCost));
                break;
            case TriDisplay.CostDesc:
                shopItemConfig.Items.Sort((a, b) => b.BaseCost.CompareTo(a.BaseCost));
                break;
            case TriDisplay.NameAZ:
                shopItemConfig.Items.Sort((a, b) => a.Name.CompareTo(b.Name));
                break;
            case TriDisplay.NameZA:
                shopItemConfig.Items.Sort((a, b) => b.Name.CompareTo(a.Name));
                break;
        }
    }


    private void MarketController_OnItemBuyCliqued(ShopCardBasic obj)
    {
        GenerateCartItem(obj.ItemDataInstance.BaseCost, obj.ItemDataInstance, obj.ItemState);
        PlayerValidateCartButtonCheck();
    }

    private void PromotionsController_OnItemBuyCliqued(ShopCardPromotions obj)
    {
        GenerateCartItem(obj.CostWithPromotion, obj.ItemDataInstance, obj.ItemState, true);
        PlayerValidateCartButtonCheck();
    }

    private void OnCommandCartCliqued()
    {
        //réinitialiser la thune
        playerManagerInstance._data.CurrencyShop -= CartController.TotalPriceInCart;
        CommandsController.MoveCartToCommands(CartController.Controllers);
        CartController.ResetCart();
        PlayerValidateCartButtonCheck();
    }

    public void GenerateCartItem(int itemPrice, ShopItemData itemData, ShopItemState boughtFromItemState, bool fromPromotion = false)
    {
        ShopCartItem generatedItem = CartController.OnItemAddedToCart(itemPrice, itemData, boughtFromItemState, (removedItem) =>
        {
            CartController.TotalPriceInCart -= removedItem.StoredPrice;
            CartController.UpdateCartPrice();
            CartController.Scroller.UpdateContentTransform();

            switch (removedItem.BoughtFromItemState)
            {
                case ShopItemState.Promotion:
                    ShopCardPromotions item = PromotionsController.FindItemByName(removedItem.ItemDataInstance.Name)[0];
                    item.AddAndUpdateStock(1);
                    break;
                case ShopItemState.MainMarket:
                    ShopCardBasic marketItm = MarketController.FindItemByName(removedItem.ItemDataInstance.Name)[0];
                    marketItm.AddAndUpdateStock(1);
                    break;
            }
            CartController.Controllers.Remove(removedItem);
            Destroy(removedItem.gameObject);
            PlayerValidateCartButtonCheck();
        });

        if (fromPromotion)
            generatedItem.ItemCost.color = Color.green;
        
    }

    public void UpdatePlayerCurrencyText(float value)
    {
        playerCurrencyTxt.text = value.ToString();
    }

    public bool PlayerValidateCartButtonCheck()
    {
        if (CartController.TotalPriceInCart <= 0 || CartController.Controllers.Count == 0)
        {
            CartController.CommandButton.interactable = false;
            return false;
        }
        if (CartController.TotalPriceInCart > playerManagerInstance._data.CurrencyShop && CartController.CommandButton.interactable == true)
        {
            //Désactiver
            CartController.CommandButton.interactable = false;
        }
        else if (CartController.TotalPriceInCart <= playerManagerInstance._data.CurrencyShop && CartController.CommandButton.interactable == false)
        {
            //activer
            CartController.CommandButton.interactable = true;
        }
        return CartController.CommandButton.interactable;
    }

    public override void OnShow()
    {
        
    }

    private void _onPlayerCurrencyChanged_handle(float obj)
    {
        UpdatePlayerCurrencyText(obj);
        PlayerValidateCartButtonCheck();
    }
}