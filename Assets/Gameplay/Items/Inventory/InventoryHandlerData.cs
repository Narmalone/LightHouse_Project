using System;
using UnityEngine;

namespace LightHouse.Inventory
{
    public static class InventoryHandlerData
    {
        #region FIELDS
        /// <summary>
        /// ID de l’objet actuellement sélectionné (main du joueur)
        /// </summary>
        public static ushort? CurrentHandedItemID { get; private set; }

        /// <summary>
        /// Référence directe à l’item, setted when scrolling
        /// </summary>
        public static IInventoryItem CurrentHandedItemType { get; private set; }

        /// <summary>
        /// Usefull to have the reference to drop an item 
        /// </summary>
        public static Transform InventoryTargetPosition { get; private set; }

        // Events
        public static event Action<IInventoryItem> OnSelectedItemChanged;
        public static event Action OnItemDropped;

        /// <summary>
        /// The Generated Slots
        /// </summary>
        private static ItemSlot[] _slots;
        #endregion

        #region INIT & RESET
        public static void Initialize(ItemSlot[] slots, Transform inventoryTargetPosition)
        {
            _slots = slots;
            InventoryTargetPosition = inventoryTargetPosition;
        }
        public static void Reset()
        {
            InventoryTargetPosition = null;
            CurrentHandedItemType = null;
            CurrentHandedItemID = null;
            _slots = null;
        }
        #endregion

        #region QUERY ITEMS
        public static bool TryFindSpecificItem(ushort globalID, ushort specificID,out IInventoryItem item, out short slotID)
        {
            item = null;
            slotID = -1;
            if (_slots == null) return false;
            foreach (ItemSlot slot in _slots)
            {
                if (!slot.SlotDatas.HasItem || slot.SlotDatas.ItemGlobalID != globalID) return false;
                item = slot.SlotDatas.GetItemInSlot(specificID);
                if (item != null)
                {
                    slotID = slot.SlotDatas.SlotID;
                    return true;
                }
            }
            return false;
        }

        public static bool TryFindItem(ushort globalID, out IInventoryItem item)
        {
            item = null;
            if (_slots == null) return false;
            foreach (ItemSlot slot in _slots)
            {
                if (slot.SlotDatas.HasItem && slot.SlotDatas.ItemGlobalID == globalID)
                {
                    item = slot.SlotDatas.GetFirstItemInSlot();
                    return true;
                }
            }
            return false;
        }

        public static bool TryFindItemInCurrentSlotFirst(ushort slotID, ushort globalID, out IInventoryItem item)
        {
            item = null;
            if (_slots == null) return false;
            if (_slots[slotID].SlotDatas.HasItem && _slots[slotID].SlotDatas.ItemGlobalID == globalID 
                && _slots[slotID].SlotDatas.IsSelected)
            {
                item = _slots[slotID].SlotDatas.GetFirstItemInSlot();
                return true;
            }
            return false;
        }

        public static bool IsItemOnHands(ushort globalID, out IInventoryItem item)
        {
            item = null;
            if (_slots == null) return false;
            foreach (ItemSlot slot in _slots)
            {
                if (slot.SlotDatas.HasItem && slot.SlotDatas.ItemGlobalID == globalID && slot.SlotDatas.IsSelected)
                {
                    item = slot.SlotDatas.GetFirstItemInSlot(); 
                    return true;
                }
            }
            return false;
        }
        #endregion

        #region MANAGEMENT
        public static void SetSelectedItem(IInventoryItem item)
        {
            CurrentHandedItemType = item;
            CurrentHandedItemID = item?.GlobalItemID;
            OnSelectedItemChanged?.Invoke(item);
        }

        public static void ClearSelection()
        {
            if (CurrentHandedItemType == null) return; // called only once
            CurrentHandedItemType = null;
            CurrentHandedItemID = null;
            OnSelectedItemChanged?.Invoke(null);
        }
        #endregion

        #region DROP
        public static void NotifyDrop(int slotID)
        {
            if (!_slots[slotID].SlotDatas.HasItem)
            {
                ClearSelection();
            }
            OnItemDropped?.Invoke();
        }
        #endregion
    }
}

