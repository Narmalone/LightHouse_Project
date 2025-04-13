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
        private static ItemSlot[] _slots => SlotManager.Slots;
        private static InventoryUIController _uiController;
        #endregion

        #region INIT & RESET
        public static void Initialize(InventoryUIController uiController, Transform inventoryTargetPosition)
        {
            InventoryTargetPosition = inventoryTargetPosition;
            _uiController = uiController;
        }
        public static void Reset()
        {
            _uiController = null;
            InventoryTargetPosition = null;
            CurrentHandedItemType = null;
            CurrentHandedItemID = null;
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

