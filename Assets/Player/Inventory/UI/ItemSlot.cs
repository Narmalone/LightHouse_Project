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

        /// <summary>
        /// Retrive first item in the pool with a globalID
        /// </summary>
        /// <returns></returns>
        public IInventoryItem GetFirstItemInSlot()
        {
            if (ItemSpecificIds.Count <= 0) return null;
            return PoolManager.GetWithoutRemovingFromPool(ItemGlobalID, ItemSpecificIds[0]);
        }

        public IInventoryItem GetItemInSlot(ushort specificID)
        {
            if (ItemSpecificIds.Count <= 0) return null;
            return PoolManager.GetWithoutRemovingFromPool(ItemGlobalID, ItemSpecificIds[specificID]);
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
        private ItemDatabase _itemDatabase;

        public void Init(ItemDatabase itemDB)
        {
            _itemDatabase = itemDB;
        }

        private void Start()
        {
            SlotDatas.Init();
        }

        #region UI Functions

        public void AddItemDatasToSlot(IInventoryItem item)
        {
            SlotDatas.ItemGlobalID = item.GlobalItemID;
            SlotDatas.ItemSpecificIds.Add(item.ItemSpecificID);
            SlotDatas.ItemSprite = item.ItemSprite;
            RefreshUIWithCurrentDatas();

            if(SlotDatas.IsSelected)
                IsInventoryItemUsable(item as IInventoryItemUsable);
        }

        public void RemoveItemFromSlot(ushort specificID)
        {
            if (!SlotDatas.ItemSpecificIds.Contains(specificID)) return;
            SlotDatas.ItemSpecificIds.Remove(specificID);
            RefreshUIWithCurrentDatas();
        }

        public void RefreshUIWithCurrentDatas()
        {
            IInventoryItem item = _itemDatabase.Get(SlotDatas.ItemGlobalID);
            if(item is IInventoryStackable)
            {
                if (!ItemStack_TMP.isActiveAndEnabled) SetEnableItemStackCountText(true);
                UpdateItemStackCount();
            }

            if (SlotDatas.HasItem)
            {
                _spriteItem.sprite = SlotDatas.ItemSprite;
                if (SlotDatas.IsSelected) Show();
            }
            else
            {
                _spriteItem.sprite = null;
                if (ItemStack_TMP.isActiveAndEnabled) SetEnableItemStackCountText(false);
                HideSelectedInfos();
            }

        }

        public void HideSelectedInfos()
        {
            SetEnableItemNameText(false);
            SetEnableUseKeyText(false);
        }

        public void Show()
        {
            SetEnableItemNameText(true);
            IInventoryItem item = _itemDatabase.Get(SlotDatas.ItemGlobalID);
            if(item != null)
                SetItemNameText(item.GetName());
            if (SlotDatas.TotalItemsInSlots > 1)
            {
                SetEnableItemStackCountText(true);
                UpdateItemStackCount();
            }
            IsInventoryItemUsable(item as IInventoryItemUsable);
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
            if(_spriteItem.sprite != null)
                _spriteItem.sprite = null;
        }
    }
}

