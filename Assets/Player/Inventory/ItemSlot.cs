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
        public ushort MaxStack;
        public bool HasItem => ItemSpecificIds.Count > 0;
        public byte SlotID;
        public ushort ItemGlobalID;

        public int TotalItemsInSlots => ItemSpecificIds.Count;
        public List<ushort> ItemSpecificIds;

        public bool IsSelected;

        public Sprite ItemSprite;

        public void Init()
        {
            MaxStack = 1;
            IsSelected = false;
            ItemSpecificIds = new List<ushort>();
        }

        public bool CanStack()
        {
            return TotalItemsInSlots < MaxStack;
        }
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

        private void Start()
        {
            SlotDatas.Init();
        }

        #region UI Functions

        public void AddItemDatasToSlot(IInventoryItem item)
        {
            SlotDatas.ItemGlobalID = item.ID;
            SlotDatas.ItemSpecificIds.Add(item.SpecificID);
            SlotDatas.ItemSprite = item.ItemSprite;            
            RefreshUI();
        }

        public void RemoveItemFromSlot(ushort specificID)
        {
            if (SlotDatas.ItemSpecificIds.Contains(specificID))
            {
                SlotDatas.ItemSpecificIds.Remove(specificID);
                RefreshUI();
            }
        }

        public void RefreshUI()
        {
            if (SlotDatas.HasItem)
            {
                _spriteItem.sprite = SlotDatas.ItemSprite;
                if (SlotDatas.IsSelected) Show();
            }
            else
            {
                _spriteItem.sprite = null;
                HideSelectedInfos();
            }

        }

        public void HideSelectedInfos()
        {
            SetEnableItemNameText(false);
            if (ItemStack_TMP.isActiveAndEnabled) SetEnableItemStackCountText(false);
        }

        public void Show()
        {
            SetEnableItemNameText(true);
            if (SlotDatas.TotalItemsInSlots > 1) SetEnableItemStackCountText(true);
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
            ItemStack_TMP.text = $"x{SlotDatas.TotalItemsInSlots}";
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

