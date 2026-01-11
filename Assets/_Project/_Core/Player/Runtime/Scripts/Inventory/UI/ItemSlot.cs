using LightHouse.Core.Player.Inventory;
using LightHouse.Core.Player.Inventory.Pool;
using LightHouse.Features.Items.Inventory.Databases;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LightHouse.Features.Items.Inventory.UI
{
    [System.Serializable]
    public struct SlotData
    {
        #region Fields & Properties
        /// <summary>
        /// The slot ID or more likely the slot index <see cref="InventoryUIController._generatedSlots"/>
        /// </summary>
        public byte SlotID;

        /// <summary>
        /// The maximum items the player can stack in this slot, if the item is not stackable
        /// it will be 1 by default or cf "<see cref="IInventoryStackable"/>".
        /// </summary>
        public ushort MaxStack;

        /// <summary>
        /// Which item is stored in the slot <see cref="ItemDatabase"/>
        /// </summary>
        public short ItemGlobalID;

        /// <summary>
        /// A list of "Specific IDS", attribuated by the <see cref="PoolManager.InventoryItemPools"/> to directly 
        /// modify the <seealso cref="IInventoryItem.ItemSpecificID"/>
        /// </summary>
        public List<ushort> ItemSpecificIds;

        /// <summary>
        /// If there are any items in this slots
        /// </summary>
        public bool HasItem => ItemSpecificIds.Count > 0;

        /// <summary>
        /// The total numbers of items, representing the length of the list
        /// </summary>
        public int TotalItemsInSlots => ItemSpecificIds.Count;

        /// <summary>
        /// If this slot is selected by the <see cref="PlayerInventorManager"/>
        /// </summary>
        public bool IsSelected;

        /// <summary>
        /// The stored ItemSprite
        /// </summary>
        public Sprite ItemSprite;

        #endregion

        #region Init
        public void Init()
        {
            MaxStack = 1;
            IsSelected = false;
            ItemSpecificIds = new List<ushort>();
        }
        #endregion

        #region Retrieve Items
        /// <summary>
        /// Retrive first item in the pool with a globalID
        /// </summary>
        public bool GetFirstItemInSlot(out IInventoryItem item)
        {
            item = null;
            if (ItemSpecificIds.Count <= 0) return false;
            return InventoryPoolManager.GetWithoutRemovingFromPool((ushort)ItemGlobalID, ItemSpecificIds[0], out item);
        }

        /// <summary>
        /// Retried a specific item from this slot in the pool with a specific ID
        /// </summary>
        /// <param name="specificID"></param>
        public bool TryGetItemInSlot(ushort globalID, ushort specificID, out IInventoryItem item)
        {
            item = null;
            if (ItemSpecificIds.Count <= 0 || ItemGlobalID != globalID) return false;
            if (ItemSpecificIds.Contains(specificID))
            {
                return InventoryPoolManager.GetWithoutRemovingFromPool((ushort)ItemGlobalID, specificID, out item);
            }
            return false;
        }
        #endregion

        #region Can Stack
        /// <summary>
        /// Check if we can stack an item in this slot
        /// </summary>
        public bool CanStack()
        {
            return TotalItemsInSlots < MaxStack;
        }
        #endregion
    }

    public class ItemSlot : MonoBehaviour
    {
        #region FIELDS && PROPERTIES
        [SerializeField] private TextMeshProUGUI _itemName_TMP;
        [SerializeField] private TextMeshProUGUI _itemUseKey_TMP;
        [SerializeField] private TextMeshProUGUI _itemStack_TMP;
        [SerializeField] private TextMeshProUGUI _itemSlotID_TMP;
        [SerializeField] private Outline _outline;

        [SerializeField] private Image _spriteItem;

        public TextMeshProUGUI ItemName_TMP => _itemName_TMP;
        public TextMeshProUGUI ItemUseKey_TMP => _itemUseKey_TMP;
        public TextMeshProUGUI ItemStack_TMP => _itemStack_TMP;

        private ItemDatabase _itemDatabase;
        public SlotData SlotDatas;
        #endregion

        #region INIT
        public void Init(ItemDatabase itemDB, byte slotID)
        {
            SlotDatas.Init();
            SlotDatas.SlotID = slotID;
            _itemSlotID_TMP.text = (slotID + 1).ToString();
            _itemDatabase = itemDB;
            _outline.enabled = false;
        }
        #endregion

        #region ADD / REMOVE METHODS

        public void AddItemDatasToSlot(IInventoryItem item)
        {
            SlotDatas.ItemGlobalID = (short)item.GlobalItemID;
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
            if (SlotDatas.TotalItemsInSlots <= 0)
            {
                SlotDatas.ItemGlobalID = -1;
            }
        }

        #endregion

        #region IInventory Usable Callbacks
        public void SubscribeToIInventoryUsable(IInventoryItemUsable usable)
        {
            usable.CanBeUsedFromInventoryChanged += Usable_CanBeUsedFromInventoryChanged;
            usable.UseTextSlotChanged += Usable_UseTextSlotChanged;
        }

        public void UnsubscribeToIInventoryUsable(IInventoryItemUsable usable)
        {
            usable.CanBeUsedFromInventoryChanged -= Usable_CanBeUsedFromInventoryChanged;
            usable.UseTextSlotChanged -= Usable_UseTextSlotChanged;
        }

        private void Usable_UseTextSlotChanged(string obj)
        {
            SetUseKeyText(obj);
        }

        private void Usable_CanBeUsedFromInventoryChanged(ushort globalID, ushort specificID)
        {
            if (SlotManager.TryFindItemInCurrentSelectedSlot(globalID, specificID, out IInventoryItem item, out short slotID))
            {
                if (item is IInventoryItemUsable usable)
                    IsInventoryItemUsable(usable);
            }
        }
        #endregion

        public void SubscribeToIInventoryItem(IInventoryItem item)
        {
            item.OnNameUpdated += Item_OnNameUpdated;
        }

        private void Item_OnNameUpdated(string value)
        {
            _itemName_TMP.text = value;
        }

        public void UnsubscribeToIInventoryItem(IInventoryItem item)
        {
            item.OnNameUpdated -= Item_OnNameUpdated;
        }

        #region Refresh UI On Add or Removed Item
        public void RefreshUIWithCurrentDatas()
        {
            IInventoryItem item = _itemDatabase.Get((ushort)SlotDatas.ItemGlobalID);
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
        #endregion

        #region Show / Hide Methods
        public void Show()
        {
            SetEnableItemNameText(true);
            _outline.enabled = true;
            SlotDatas.GetFirstItemInSlot(out IInventoryItem itm);
            if (itm != null)
            {
                SetItemNameText(itm.GetName());
            }
            if (SlotDatas.TotalItemsInSlots > 1)
            {
                SetEnableItemStackCountText(true);
                UpdateItemStackCount();
            }
            IsInventoryItemUsable(itm as IInventoryItemUsable);
        }

        public void HideSelectedInfos()
        {
            SetEnableItemNameText(false);
            SetEnableUseKeyText(false);
            _outline.enabled = false;
        }
        #endregion

        #region ITEM NAME METHODS
        public void SetEnableItemNameText(bool value) => ItemName_TMP.gameObject.SetActive(value);
        public void SetItemNameText(string text) => ItemName_TMP.text = text;
        #endregion

        #region STACKABLE METHODS
        public void SetEnableItemStackCountText(bool value) => ItemStack_TMP.gameObject.SetActive(value);
        public void UpdateItemStackCount() => ItemStack_TMP.text = $"x{SlotDatas.TotalItemsInSlots}";
        #endregion

        #region INVENTORY USABLE METHODS
        public void IsInventoryItemUsable(IInventoryItemUsable itemUsable)
        {
            if (itemUsable == null) return;
            if (itemUsable.CanBeUsedFromInventory)
            {
                SetEnableUseKeyText(true);
                SetUseKeyText(itemUsable.UseTextSlot());
            }
            else
                SetEnableUseKeyText(false);
        }
        #endregion

        #region USE KEY
        public void SetEnableUseKeyText(bool value) => ItemUseKey_TMP.gameObject.SetActive(value);
        public void SetUseKeyText(string text) => ItemUseKey_TMP.text = text;
        #endregion

        #region RESET
        public void ResetSlot()
        {
            if(_spriteItem.sprite != null)
                _spriteItem.sprite = null;
        }
        #endregion
    }
}

