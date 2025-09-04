using AYellowpaper.SerializedCollections;
using LightHouse.Game.Computer.LEO;
using LightHouse.Game.Computer.LEO.Supplies;
using System;
using System.Collections.Generic;
using UnityEngine;

public class SupplyManager : LEOWindow
{
    [SerializeField] private SupplyConfigurator _configurator;
    [SerializeField] private RectTransform _orderParent;
    [SerializeField] private SupplyItem _supplyItemPrefab;
    [SerializeField] private SerializedDictionary<E_SupplyCategory, RectTransform> _supplyTransforms;

    private SerializedDictionary<int, SupplyItem> _spawnedShopItems = new(); //int == Unique Id, supply item stores the ref of the datas
    private SerializedDictionary<int, SupplyItem> _spawnedOrderItems = new(); //int == Unique Id, supply item stores the ref of the datas


    private void Start()
    {
        GenerateSuppliesByConfig(_configurator);
    }

    protected void OnDestroy()
    {
        ClearItems();
    }

    private void ClearItems()
    {
        foreach (var item in _spawnedShopItems)
        {
            if (item.Value == null) continue;

            item.Value.PlusCliqued -= ShopItem_PlusCliqued;
            item.Value.MinusCliqued -= ShopItem_MinusCliqued;

            Destroy(item.Value.gameObject);
        }

        foreach (var item in _spawnedOrderItems)
        {
            if (item.Value == null) continue;

            item.Value.PlusCliqued -= OrderItem_PlusCliqued;
            item.Value.MinusCliqued -= OrderItem_MinusCliqued;

            Destroy(item.Value.gameObject);
        }

        _spawnedShopItems.Clear();
        _spawnedOrderItems.Clear();
    }

    public void GenerateSuppliesByConfig(SupplyConfigurator config)
    {
        // Avant de régénérer, nettoyer les anciens items
        ClearItems();

        foreach (var kvp in config.SupplyConfig)
        {
            if (!_supplyTransforms.TryGetValue(kvp.Key, out var parent) || parent == null) continue;

            foreach (var data in kvp.Value)
            {
                if (data == null) continue;

                SupplyItem newItem = Instantiate(_supplyItemPrefab, parent);
                newItem.name = data.Name;
                newItem.Initialize(data);

                newItem.PlusCliqued += ShopItem_PlusCliqued;
                newItem.MinusCliqued += ShopItem_MinusCliqued;
                _spawnedShopItems.Add(data.UniqueID, newItem);
            }
        }
    }

    private void ShopItem_MinusCliqued(LightHouse.Game.Computer.LEO.Supplies.SupplyItemDatas obj)
    {
        UpdateOrderItem(obj);
    }

    private void ShopItem_PlusCliqued(LightHouse.Game.Computer.LEO.Supplies.SupplyItemDatas obj)
    {
        UpdateOrderItem(obj);
    }

    public void UpdateOrderItem(SupplyItemDatas datas)
    {
        //si le truc pour l'ordre n'éxiste pas on la génère sinon on l'a trouve
        if (datas.SelectedAmountToBuy > 0 && !_spawnedOrderItems.TryGetValue(datas.UniqueID, out SupplyItem undefinedItem))
        {
            var newSupplyItemInfos = Instantiate(_supplyItemPrefab, _orderParent);
            newSupplyItemInfos.Initialize(datas);
            newSupplyItemInfos.MinusCliqued += OrderItem_MinusCliqued;
            newSupplyItemInfos.PlusCliqued += OrderItem_PlusCliqued;
            _spawnedOrderItems.Add(datas.UniqueID, newSupplyItemInfos);
            newSupplyItemInfos.UpdateSelectedCountText();
        }
        else if (datas.SelectedAmountToBuy > 0 && _spawnedOrderItems.TryGetValue(datas.UniqueID, out SupplyItem findedItem))
        {
            findedItem.UpdateSelectedCountText();
        }
        //si on les a enlevé et qu'il existe alors on le détruit
        else if(datas.SelectedAmountToBuy <= 0 && _spawnedOrderItems.TryGetValue(datas.UniqueID, out SupplyItem existingItem))
        {
            existingItem.PlusCliqued -= OrderItem_PlusCliqued;
            existingItem.MinusCliqued -= OrderItem_MinusCliqued;
            _spawnedOrderItems.Remove(datas.UniqueID);
            Destroy(existingItem.gameObject);
        }
    }

    private void OrderItem_PlusCliqued(SupplyItemDatas datas)
    {
        //update shop item count
        UpdateShopItem(datas);
    }

    private void OrderItem_MinusCliqued(SupplyItemDatas datas)
    {
        //update shop item count
        UpdateShopItem(datas);
    }

    private void UpdateShopItem(SupplyItemDatas datas)
    {
        if (datas == null) return;

        // Rafraîchir l'item Shop s’il existe
        if (_spawnedShopItems.TryGetValue(datas.UniqueID, out var shopItem) && shopItem != null)
            shopItem.UpdateSelectedCountText();
    }
}
