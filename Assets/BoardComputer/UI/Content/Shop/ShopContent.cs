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
    [SerializeField] private CustomEvent_Float _onPlayerCurrencyChanged;
    [SerializeField] private List<ShopCardController> controllers = new List<ShopCardController>();

    private PlayerManager playerManagerInstance;

    private void Awake()
    {
        //SortItems();
        //InitShopCards();

        //_onPlayerCurrencyChanged.handle += _onPlayerCurrencyChanged_handle;
    }

    private void Start()
    {
        playerManagerInstance = PlayerManager.Instance;
        //SetDeliveryTime(shopItemConfig.AverageDeliveryTime);

        //CheckCostForPlayersMoney();
        //UpdatePlayerCurrencyText();
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
        //_onPlayerCurrencyChanged.handle -= _onPlayerCurrencyChanged_handle;
    }

    private void InitShopCards()
    {
        for (int i = 0; i < shopItemConfig.Items.Count; i++)
        {
            ShopCardController newItem = Instantiate(shopCardPrefab, shopCardsTransform);
            newItem.SetCardsInfos(shopItemConfig.Items[i].Name, shopItemConfig.Items[i].Cost);
            newItem.ShopItemData = shopItemConfig.Items[i];
            controllers.Add(newItem);
        }
        InitCardButtons();
    }

    private void InitCardButtons()
    {
        foreach(ShopCardController ctrl in controllers)
        {
            ctrl.BuyBtn.onClick.AddListener(() =>
            {
                playerManagerInstance._data.CurrencyShop -= ctrl.ShopItemData.Cost;
            });
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

    public void UpdatePlayerCurrencyText()
    {
        playerCurrencyTxt.text = "Currency: " + playerManagerInstance._data.CurrencyShop;
    }

    public override void OnShow()
    {
        
    }

    private void CheckCostForPlayersMoney()
    {
        var currency = playerManagerInstance._data.CurrencyShop;
        foreach (ShopCardController ctrl in controllers)
        {
            ctrl.BuyBtn.interactable = ctrl.ShopItemData.Cost <= currency; 
        }
    }

    private void _onPlayerCurrencyChanged_handle(float obj)
    {
        CheckCostForPlayersMoney();
        UpdatePlayerCurrencyText();
    }
}