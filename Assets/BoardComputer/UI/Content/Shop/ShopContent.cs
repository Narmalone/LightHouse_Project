using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ShopContent : ContentWindow
{
    [SerializeField] private TextMeshProUGUI playerCurrencyTxt;
    [SerializeField] private TextMeshProUGUI deliveryTimeTxt;
    [SerializeField] private ShopCardController buyableCardsPrefab;
    [SerializeField] private ShopCardController cartCardsPrefab;
    [SerializeField] private ShopCardController commandCardsPrefab;
    [SerializeField] private Transform shopCardsTransform;
    [SerializeField] private ShopItemConfig shopItemConfig;
    public Promotions PromotionsController;
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
        PromotionsController.GenerateRandomPromotions(shopItemConfig);
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
            ShopCardController newItem = Instantiate(buyableCardsPrefab, shopCardsTransform);
            //newItem.SetCardsInfos(shopItemConfig.Items[i].Name, shopItemConfig.Items[i].Cost);
            newItem.ItemDataInstance = shopItemConfig.Items[i];
            controllers.Add(newItem);
        }
        InitCardButtons();
    }

    private void InitCardButtons()
    {
        foreach(ShopCardController ctrl in controllers)
        {
            /*ctrl.BuyBtn.onClick.AddListener(() =>
            {
                playerManagerInstance._data.CurrencyShop -= ctrl.ShopItemData.Cost;
            });*/
        }
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

   

    public void UpdatePlayerCurrencyText(float value)
    {
        playerCurrencyTxt.text = value.ToString();
    }

    public override void OnShow()
    {
        
    }

    private void CheckCostForPlayersMoney()
    {
        var currency = playerManagerInstance._data.CurrencyShop;
        foreach (ShopCardController ctrl in controllers)
        {
            //ctrl.BuyBtn.interactable = ctrl.ShopItemData.Cost <= currency; 
        }
    }

    private void _onPlayerCurrencyChanged_handle(float obj)
    {
        CheckCostForPlayersMoney();
        UpdatePlayerCurrencyText(obj);
    }
}