using LightHouse.Features.Items.Inventory;
using LightHouse.Features.Items.Inventory.UI;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace LightHouse.Core.Player.Inventory
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

        public static bool IsGrabbingObject { get; private set; }

        // Events
        public static event Action<IInventoryItem> OnSelectedItemChanged;
        public static event Action<IInventoryItem> OnItemAddedToInventory;
        public static event Action<IInventoryItem> OnItemDropped;
        public static event Action OnInitialized;
        public static bool IsInitialized { get; private set; } = false;

        public static event Action OnGrabObjectChanged;

        /// <summary>
        /// The Generated Slots
        /// </summary>
        private static List<ItemSlot> _slots => SlotManager.Slots;
        #endregion

        #region INIT & RESET
        public static void SetInventoryTarget(Transform inventoryTargetPosition)
        {
            InventoryTargetPosition = inventoryTargetPosition;
        }

        public static void Initialize()
        {
            IsInitialized = true;
            OnInitialized?.Invoke();
        }

        public static void Reset()
        {
            IsInitialized = false;
            OnGrabObjectChanged = null;
            OnSelectedItemChanged = null;
            OnItemAddedToInventory = null;
            OnItemDropped = null;
            OnInitialized = null;
            CurrentHandedItemID = null;
            CurrentHandedItemType = null;
            IsGrabbingObject = false;
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

        #region Added To Inventory
        public static void NotifyAddedToInventory(IInventoryItem item)
        {
            OnItemAddedToInventory?.Invoke(item);
        }
        #endregion

        #region DROP
        public static void NotifyDrop(int slotID, IInventoryItem item)
        {
            if (!_slots[slotID].SlotDatas.HasItem)
            {
                ClearSelection();
            }
            OnItemDropped?.Invoke(item);
        }
        #endregion

        #region GRAB INFO
        public static void SetGrabbingObject(bool value)
        {
            if(value != IsGrabbingObject)
            {
                IsGrabbingObject = value;
                OnGrabObjectChanged?.Invoke();
            }
        }
        #endregion

        public static bool IsGrabbingObjectOrIndexInvalid()
        {
            return IsGrabbingObject || SlotManager.IsIndexInvalid(SlotManager.CurrentSlotIndex) || SlotManager.CurrentSelectedSlot.SlotDatas.ItemSpecificIds.Count <= 0;
        }
    }
}

