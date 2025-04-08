using LightHouse.Inventory;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LightHouse.Inventory
{
    [System.Serializable]
    public struct SlotData
    {
        public bool HasItem;
        public byte SlotID;
        public ushort ItemSpecificID;
        public ushort ItemID;

        public Sprite ItemSprite;
    }

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


        public SlotData SlotDatas;

        #region Inventory Items

        private IInventoryItem _inventoryItem;
        public IInventoryItem InventoryItem => _inventoryItem;

        private IInventoryItemUsable _inventoryItemUsable;
        public IInventoryItemUsable InventoryItemUsable => _inventoryItemUsable;

        private IInventoryItemCallback _inventoryItemCallback;
        public IInventoryItemCallback InventoryItemCallback => _inventoryItemCallback;

        #endregion

        #region UI Functions

        public void AddItemDatasToSlot(ushort itemID, ushort itemSpecificID, Sprite itemSprite)
        {
            SlotDatas.ItemID = itemID;
            SlotDatas.ItemSpecificID = itemSpecificID;
            SlotDatas.ItemSprite = itemSprite;
            SlotDatas.HasItem = true;
            
            RefreshUI();
        }

        public void RefreshUI()
        {
            _spriteItem.sprite = SlotDatas.ItemSprite;
            
        }

        public void SetInventoryItem(IInventoryItem item)
        {
            SlotDatas.ItemID = item.ID;
            _inventoryItem = item;
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
            //ItemStack_TMP.text = $"x{}";
        }

        public void SetEnableItemStackCountText(bool value)
        {
            ItemStack_TMP.gameObject.SetActive(value);
        }
        #endregion

        public void ResetSlot()
        {
            if(_inventoryItem != null)
            {
                _inventoryItem = null;
            }

            if(_inventoryItemUsable != null)
                _inventoryItemUsable = null;

            if(_inventoryItemCallback != null)
                _inventoryItemCallback = null;

            if(_spriteItem.sprite != null)
                _spriteItem.sprite = null;
        }
    }
}

