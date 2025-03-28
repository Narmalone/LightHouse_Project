using LightHouse.Inventory;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LightHouse.Inventory
{
    public class ItemSlot : MonoBehaviour
    {
        #region Texts Fields
        [SerializeField] private TextMeshProUGUI _itemName_TMP;
        [SerializeField] private TextMeshProUGUI _itemUseKey_TMP;
        [SerializeField] private TextMeshProUGUI _itemStack_TMP;
        [SerializeField] private Image _spriteItem;

        public Image SpriteItemImage => _spriteItem;
        public Sprite SpriteItem => _spriteItem.sprite;

        public TextMeshProUGUI ItemName_TMP => _itemName_TMP;
        public TextMeshProUGUI ItemUseKey_TMP => _itemUseKey_TMP;
        public TextMeshProUGUI ItemStack_TMP => _itemStack_TMP;
        #endregion

        #region Inventory Items

        private IInventoryItem _inventoryItem;
        public IInventoryItem InventoryItem => _inventoryItem;

        #region Iventory Usable Items
        private IInventoryItemUsable _inventoryItemUsable;
        public IInventoryItemUsable InventoryItemUsable => _inventoryItemUsable;
        #endregion

        #region Inventory Callbacks Items
        private IInventoryItemCallback _inventoryItemCallback;
        public IInventoryItemCallback InventoryItemCallback => _inventoryItemCallback;
        #endregion

        #region STACKING ITEMS
        private readonly List<IInventoryItem> _stackedItems = new();
        public List<IInventoryItem> StackedItems => _stackedItems;
        public int StackCount => _stackedItems.Count;
        public int StackedItemsCount => _stackedItems.Count + 1; // +1 pour l'item visible
        #endregion

        #endregion

        #region UI Functions

        public void SetInventoryItem(IInventoryItem inventoryItem)
        {
            _inventoryItem = inventoryItem;
        }

        public void SetSpriteItem(IInventoryItem inventoryItem)
        {
            if (inventoryItem.ItemSprite == null) return;
            _spriteItem.sprite = inventoryItem.ItemSprite;
        }

        public void SetItemCallback(IInventoryItemCallback inventoryItemCallback) 
        {
            _inventoryItemCallback = inventoryItemCallback;
        }

        public void SetItemUsable(IInventoryItemUsable inventoryItemUsable)
        {
            _inventoryItemUsable = inventoryItemUsable;
        }

        public void SetEnableItemNameText(bool value)
        {
            ItemName_TMP.gameObject.SetActive(value);
        }

        public void SetEnableUseKeyText(bool value)
        {
            ItemUseKey_TMP.gameObject.SetActive(value);
        }

        public void IsInventoryItemUsable(IInventoryItemUsable itemUsable)
        {
            if (itemUsable == null) return;
            if (itemUsable.CanBeUsedFromInventory)
            {
                SetEnableUseKeyText(true);
                ItemUseKey_TMP.text = itemUsable.UseInInventoryText();
            }
            else
            {
                SetEnableUseKeyText(false);
            }
        }

        public void SetItemNameText(string text)
        {
            ItemName_TMP.text = text;
        }

        public void UpdateItemStackCount()
        {
            ItemStack_TMP.text = $"x{StackedItemsCount.ToString()}";
        }

        public void SetEnableItemStackCountText(bool value)
        {
            ItemStack_TMP.gameObject.SetActive(value);
        }
        #endregion

        public void ResetSlot()
        {
            if(_inventoryItem != null)
                _inventoryItem = null;

            if(_inventoryItemUsable != null)
                _inventoryItemUsable = null;

            if(_inventoryItemCallback != null)
                _inventoryItemCallback = null;

            if(_spriteItem.sprite != null)
                _spriteItem.sprite = null;

            if (_stackedItems.Count > 0)
            {
                foreach(var item in _stackedItems)
                {
                    item.GetGameObject().transform.SetParent(null);
                }
                ClearStack();
            }
        }

        #region Stack
        public void AddStackedItem(IInventoryItem item)
        {
            item.GetGameObject().SetActive(false);
            item.GetCollider().enabled = false;
            item.GetRigidBody().isKinematic = true;
            item.IsItemInInventory = true;
            _stackedItems.Add(item);
        }

        public IInventoryItem RemoveStackedItem()
        {
            if (_stackedItems.Count == 0) return null;
            IInventoryItem item = _stackedItems[^1];
            _stackedItems.RemoveAt(_stackedItems.Count - 1);
            return item;
        }

        public void ClearStack()
        {
            _stackedItems.Clear();
        }

        #endregion
    }
}

