using System;
using UnityEngine;
using TMPro;
using AYellowpaper.SerializedCollections;

namespace LightHouse.Features.Computer.LEO.Supplies.Order
{
    public class OrderController : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _totalOrderText;
        [SerializeField] private RectTransform _orderParent;
        [SerializeField] private SupplyItem _supplyItemPrefab;

        // id -> item instancié côté order
        [SerializeField] private SerializedDictionary<int, SupplyItem> _orderItems = new();
        public int NumberOfItemsInOrder => _orderItems.Keys.Count;
        public SerializedDictionary<int, SupplyItem> OrderItems => _orderItems;

        // Events émis vers le manager (quand on clique sur + / - de la liste de commande)
        public event Action<SupplyItemDatas> OnOrderPlus;
        public event Action<SupplyItemDatas> OnOrderMinus;

        public void UpdateOrderFor(SupplyItemDatas datas)
        {
            if (datas == null) return;

            // créer si > 0 et pas encore présent
            if (datas.SelectedAmountToBuy > 0 && !_orderItems.ContainsKey(datas.UniqueID))
            {
                var item = Instantiate(_supplyItemPrefab, _orderParent);
                item.Initialize(datas);
                item.PlusCliqued += HandlePlus;
                item.MinusCliqued += HandleMinus;

                _orderItems[datas.UniqueID] = item;
                item.UpdateSelectedCountText();
            }
            // mettre à jour si présent et > 0
            else if (datas.SelectedAmountToBuy > 0 && _orderItems.TryGetValue(datas.UniqueID, out var existing))
            {
                existing.UpdateSelectedCountText();
            }
            // supprimer si retombé à 0
            else if (datas.SelectedAmountToBuy <= 0 && _orderItems.TryGetValue(datas.UniqueID, out var toRemove))
            {
                toRemove.PlusCliqued -= HandlePlus;
                toRemove.MinusCliqued -= HandleMinus;
                _orderItems.Remove(datas.UniqueID);
                Destroy(toRemove.gameObject);
            }
        }

        public void Clear()
        {
            foreach (var kvp in _orderItems)
            {
                var item = kvp.Value;
                if (item == null) continue;
                item.Mydatas.SelectedAmountToBuy = 0;
                item.PlusCliqued -= HandlePlus;
                item.MinusCliqued -= HandleMinus;
                Destroy(item.gameObject);
            }
            _orderItems.Clear();
        }

        private void HandlePlus(SupplyItemDatas d) => OnOrderPlus?.Invoke(d);
        private void HandleMinus(SupplyItemDatas d) => OnOrderMinus?.Invoke(d);

        public void UpdateTotalOrderValue(float order)
        {
            _totalOrderText.text = order.ToString() + "$";
        }
    }
}