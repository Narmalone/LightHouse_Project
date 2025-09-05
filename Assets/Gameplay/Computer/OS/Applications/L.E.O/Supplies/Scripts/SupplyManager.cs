using System.Collections.Generic;
using UnityEngine;
using LightHouse.Game.Computer.LEO;
using LightHouse.Game.Computer.LEO.Supplies;
using LightHouse.Money;
using UnityEngine.UI;
using System;

public class SupplyManager : LEOWindow
{
    [Header("Config")]
    [SerializeField] private SupplyConfigurator _configurator;

    [Header("Controllers")]
    [SerializeField] private ShopController _shopController;
    [SerializeField] private OrderController _orderController;

    [SerializeField] private Button _confirmOrderButton;
    [SerializeField] private Button _resetOrderButton;

    private float _totalOrderValue;

    // Copie runtime des datas (recommandé)
    private Dictionary<E_SupplyCategory, SupplyItemDatas[]> _runtimeConfig;

    public ShopController ShopController => _shopController;
    public OrderController OrderController => _orderController;

    private void Awake()
    {
        BuildRuntimeConfig(_configurator);

        // Brancher les events des controllers
        _shopController.OnShopPlus += OnShopPlus;
        _shopController.OnShopMinus += OnShopMinus;

        _orderController.OnOrderPlus += OnOrderPlus;
        _orderController.OnOrderMinus += OnOrderMinus;
        _confirmOrderButton.interactable = false;

        _resetOrderButton.onClick.AddListener(OnResetOrderCliqued);

        // Construire le shop à partir des datas runtime
        _shopController.BuildShop(_runtimeConfig);

        PlayerCurrency.OnBalanceChanged += PlayerCurrency_OnBalanceChanged;

        _shopController.SwitchTo(E_SupplyCategory.Tools);
    }

    protected void OnDestroy()
    {
        // Débrancher proprement
        _shopController.OnShopPlus -= OnShopPlus;
        _shopController.OnShopMinus -= OnShopMinus;
        _orderController.OnOrderPlus -= OnOrderPlus;
        _orderController.OnOrderMinus -= OnOrderMinus;
        _resetOrderButton.onClick.RemoveListener(OnResetOrderCliqued);
        PlayerCurrency.OnBalanceChanged -= PlayerCurrency_OnBalanceChanged;

        _shopController.Clear();
        _orderController.Clear();
    }

    /// <summary>
    /// Quand la currency change on vérifie déjà si il y'a quelque chose dans le panier
    /// 
    /// </summary>
    /// <param name="obj"></param>
    /// <exception cref="NotImplementedException"></exception>
    private void PlayerCurrency_OnBalanceChanged(float obj)
    {
        throw new NotImplementedException();
    }

    private void BuildRuntimeConfig(SupplyConfigurator src)
    {
        _runtimeConfig = new Dictionary<E_SupplyCategory, SupplyItemDatas[]>();
        foreach (var kvp in src.SupplyConfig)
        {
            var srcArray = kvp.Value;
            var clone = new SupplyItemDatas[srcArray.Length];
            for (int i = 0; i < srcArray.Length; i++)
            {
                var d = srcArray[i];
                clone[i] = new SupplyItemDatas
                {
                    UniqueID = d.UniqueID,
                    Name = d.Name,
                    Cost = d.Cost,
                    SelectedAmountToBuy = 0
                };
            }
            _runtimeConfig[kvp.Key] = clone;
        }
    }

    // ======== Gestion des clics émis par les controllers ========

    private void OnShopPlus(SupplyItemDatas d)
    {
        d.SelectedAmountToBuy = Mathf.Max(0, d.SelectedAmountToBuy + 1);
        _shopController.RefreshShopItem(d);
        _orderController.UpdateOrderFor(d);
        _totalOrderValue += d.Cost;
        UpdateOrderUI();
    }

    private void OnShopMinus(SupplyItemDatas d)
    {
        d.SelectedAmountToBuy = Mathf.Max(0, d.SelectedAmountToBuy - 1);
        _shopController.RefreshShopItem(d);
        _orderController.UpdateOrderFor(d);
        _totalOrderValue -= d.Cost;
        UpdateOrderUI();
    }

    private void OnOrderPlus(SupplyItemDatas d)
    {
        d.SelectedAmountToBuy = Mathf.Max(0, d.SelectedAmountToBuy + 1);
        _shopController.RefreshShopItem(d);
        _orderController.UpdateOrderFor(d);
        _totalOrderValue += d.Cost;
        UpdateOrderUI();
    }

    private void OnOrderMinus(SupplyItemDatas d)
    {
        d.SelectedAmountToBuy = Mathf.Max(0, d.SelectedAmountToBuy - 1);
        _shopController.RefreshShopItem(d);
        _orderController.UpdateOrderFor(d);
        _totalOrderValue -= d.Cost;
        UpdateOrderUI();
    }


    private void OnResetOrderCliqued()
    {
        _totalOrderValue = 0;
        _orderController.Clear();
        _shopController.RefreshAllShopItem();
        UpdateOrderUI();
    }

    private void UpdateOrderUI()
    {
        _orderController.UpdateTotalOrderValue(_totalOrderValue);
        if(_totalOrderValue > PlayerCurrency.Balance || _orderController.NumberOfItemsInOrder <= 0)
        {
            _confirmOrderButton.interactable = false;
        }
        else
        {
            _confirmOrderButton.interactable = true;
        }
    }
}
