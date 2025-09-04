using System;
using System.Collections.Generic;
using UnityEngine;
using AYellowpaper.SerializedCollections;
using LightHouse.Game.Computer.LEO.Supplies;

public class ShopController : MonoBehaviour
{
    [SerializeField] private SupplyItem _supplyItemPrefab;
    [SerializeField] private SerializedDictionary<E_SupplyCategory, RectTransform> _categoryParents;

    // id -> item instancié côté shop
    private readonly Dictionary<int, SupplyItem> _shopItems = new();

    // Events émis vers le manager
    public event Action<SupplyItemDatas> OnShopPlus;
    public event Action<SupplyItemDatas> OnShopMinus;

    public void BuildShop(Dictionary<E_SupplyCategory, SupplyItemDatas[]> runtimeConfig)
    {
        Clear();

        foreach (var kvp in runtimeConfig)
        {
            if (!_categoryParents.TryGetValue(kvp.Key, out var parent) || parent == null)
                continue;

            foreach (var data in kvp.Value)
            {
                if (data == null) continue;

                var item = Instantiate(_supplyItemPrefab, parent);
                item.name = data.Name;
                item.Initialize(data);

                // route boutons vers events publics
                item.PlusCliqued += HandlePlus;
                item.MinusCliqued += HandleMinus;

                _shopItems[data.UniqueID] = item;
            }
        }
    }

    public void RefreshShopItem(SupplyItemDatas datas)
    {
        if (datas == null) return;
        if (_shopItems.TryGetValue(datas.UniqueID, out var item) && item != null)
            item.UpdateSelectedCountText();
    }

    public void Clear()
    {
        foreach (var kvp in _shopItems)
        {
            var item = kvp.Value;
            if (item == null) continue;
            item.PlusCliqued -= HandlePlus;
            item.MinusCliqued -= HandleMinus;
            Destroy(item.gameObject);
        }
        _shopItems.Clear();
    }

    private void HandlePlus(SupplyItemDatas d) => OnShopPlus?.Invoke(d);
    private void HandleMinus(SupplyItemDatas d) => OnShopMinus?.Invoke(d);
}
