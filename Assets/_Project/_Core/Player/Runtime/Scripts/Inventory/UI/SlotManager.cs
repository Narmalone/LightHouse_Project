using LightHouse.Features.Items.Inventory;
using LightHouse.Features.Items.Inventory.Databases;
using LightHouse.Features.Items.Inventory.UI;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace LightHouse.Core.Player.Inventory
{
    public static class SlotManager
    {
        #region FIELDS & PROPTERTIES
        public static List<ItemSlot> Slots;
        public static short CurrentSlotIndex = -1;
        public static byte NumberOfSlotsTaken
        {
            get
            {
                byte count = 0;
                foreach (var slot in Slots)
                {
                    if (slot.SlotDatas.TotalItemsInSlots > 0)
                        count++;
                }
                return count;
            }
        }
        public static byte SlotLength => (byte)Slots.Count;
        public static ItemSlot CurrentSelectedSlot => Slots[CurrentSlotIndex];

        public static event Action OnSlotSelectedChanged;
        #endregion

        #region Init & Clear
        public static void Initialize()
        {
            Slots = new List<ItemSlot>();
            CurrentSlotIndex = -1;
        }

        public static void AddSlots(List<ItemSlot> slotsToAdd)
        {
            if (Slots == null)
                Slots = new List<ItemSlot>();
            Debug.Log("Adding slots: " + slotsToAdd.Count);
            Slots.AddRange(slotsToAdd);
        }

        public static void AddSlot(ItemSlot slotToAdd)
        {
            if (Slots == null)
                Slots = new List<ItemSlot>();
            Slots.Add(slotToAdd);
        }

        public static void Clear() => Slots = null;
        #endregion

        #region Change Selecte Slot / index
        public static void ChangeSelectedSlotIndex(short slotIndex) => CurrentSlotIndex = slotIndex;

        public static void ChangeSelectedSlot(ItemDatabase db, short slotIndex)
        {
            if (!IsIndexInvalid(CurrentSlotIndex))
            {
                CurrentSelectedSlot.SlotDatas.IsSelected = false;
                if (CurrentSelectedSlot.SlotDatas.HasItem)
                    CurrentSelectedSlot.HideSelectedInfos();
            }

            ChangeSelectedSlotIndex(slotIndex);

            if (!IsIndexInvalid(CurrentSlotIndex))
            {
                CurrentSelectedSlot.SlotDatas.IsSelected = true;
                if (CurrentSelectedSlot.SlotDatas.HasItem)
                    CurrentSelectedSlot.Show();

                if (CurrentSelectedSlot.SlotDatas.HasItem)
                {
                    //IInventoryItem item = db.Get(CurrentSelectedSlot.SlotDatas.ItemGlobalID);
                    CurrentSelectedSlot.SlotDatas.GetFirstItemInSlot(out IInventoryItem itm);
                    InventoryHandlerData.SetSelectedItem(itm);
                }
                else
                    InventoryHandlerData.ClearSelection();
            }
            else
                InventoryHandlerData.ClearSelection();

            OnSlotSelectedChanged?.Invoke();
        }

        #endregion

        #region Check Validity
        /// <summary>
        /// If the index is inside the bounds of the slots
        /// </summary>
        public static bool IsIndexInvalid(short slotIndex) => slotIndex < 0 || slotIndex >= Slots.Count;

        /// <summary>
        /// If the target slot has an item and a invalid ID
        /// </summary>
        public static bool IsSlotInvalidOrOccupied(short slotIndex) => IsIndexInvalid(slotIndex) || Slots[CurrentSlotIndex].SlotDatas.HasItem;

        #endregion

        #region Query Methods

        /// <summary>
        /// Check if there is an empty slot
        /// </summary>
        /// <param name="slot"> the empty slot</param>
        /// <returns> the first empty slot </returns>
        public static bool FindFirstEmptySlot(out ItemSlot slot)
        {
            slot = null;
            foreach (ItemSlot checkSlot in Slots)
            {
                if (checkSlot.SlotDatas.HasItem) continue;
                slot = checkSlot;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Try find a slot where the item can be stackable
        /// </summary>
        /// <param name="globalID"> the target globalID</param>
        /// <param name="slot"></param>
        /// <returns></returns>
        public static bool TryFindStackableSlot(ushort globalID, out ItemSlot slot)
        {
            slot = null;
            foreach (ItemSlot itemSlot in Slots)
            {
                if (itemSlot.SlotDatas.ItemGlobalID != globalID || !itemSlot.SlotDatas.CanStack()) continue;
                slot = itemSlot;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Try to find an item in the slots.
        /// </summary>
        public static bool FindItemInSlot(ushort globalId, ushort specificID, out byte slotID)
        {
            slotID = 100;
            foreach(ItemSlot itemSlot in Slots)
            {
                if (itemSlot.SlotDatas.ItemGlobalID != globalId) continue;
                if (itemSlot.SlotDatas.TryGetItemInSlot(globalId, specificID, out IInventoryItem item))
                {
                    slotID = itemSlot.SlotDatas.SlotID;
                    return true;
                }
            }
            return false;
        }
        
        /// <summary>
        /// Check if there is an item in the slots with the given global ID and retrieve the first item in that slot.
        /// </summary>
        public static bool GetFirstItemInSlot(ushort globalId, out IInventoryItem item)
        {
            item = null;
            foreach (ItemSlot itemSlot in Slots)
            {
                if (itemSlot.SlotDatas.ItemGlobalID != globalId || !itemSlot.SlotDatas.HasItem) continue;
                if (itemSlot.SlotDatas.GetFirstItemInSlot(out item))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Check if there is the matching items in the current selected slot
        /// </summary>
        public static bool TryFindItemInCurrentSelectedSlot(ushort globalID, ushort specificID, out IInventoryItem item, out short slotID)
        {
            item = null;
            slotID = -1;
            if (Slots == null) return false;
            foreach (ItemSlot slot in Slots)
            {
                if (slot.SlotDatas.IsSelected && slot.SlotDatas.HasItem && slot.SlotDatas.ItemGlobalID == globalID)
                {
                    if (slot.SlotDatas.TryGetItemInSlot(globalID, specificID, out item))
                    {
                        slotID = slot.SlotDatas.SlotID;
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Check if there is the matching items in the current selected slot
        /// </summary>
        public static bool TryFindFirstItemInCurrentSelectedSlot(out IInventoryItem item)
        {
            item = null;
            if (IsIndexInvalid(CurrentSlotIndex) || CurrentSelectedSlot == null || !CurrentSelectedSlot.SlotDatas.HasItem) return false;
            if (CurrentSelectedSlot.SlotDatas.TryGetItemInSlot((ushort)CurrentSelectedSlot.SlotDatas.ItemGlobalID, CurrentSelectedSlot.SlotDatas.ItemSpecificIds[0], out item))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Check if there is an item matching with it's global id
        /// </summary>
        /// <param name="globalID"> the global id of the item you want to check</param>
        /// <returns></returns>
        public static bool IsItemExistInInventory(ushort globalID)
        {
            if (Slots == null) return false;
            foreach (ItemSlot slot in Slots)
            {
                if (slot.SlotDatas.HasItem && slot.SlotDatas.ItemGlobalID == globalID)
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Retrive if the item with globalID is on Hands by checking the selectedSlotDatas
        /// </summary>
        /// <param name="globalID"> global id of the item you want to check</param>
        /// <param name="item"> the first instance that is store in the slot </param>
        /// <returns> if it found the item </returns>
        public static bool IsItemOnHands(ushort globalID, out IInventoryItem item)
        {
            item = null;
            if (Slots == null) return false;
            foreach (ItemSlot slot in Slots)
            {
                if (slot.SlotDatas.HasItem && slot.SlotDatas.ItemGlobalID == globalID && slot.SlotDatas.IsSelected)
                {
                    slot.SlotDatas.GetFirstItemInSlot(out item);
                    return true;
                }
            }
            return false;
        }
        #endregion
    }

}
