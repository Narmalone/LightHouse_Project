using System;
using LightHouse.Inventory;
using UnityEngine;

namespace LightHouse.Items.Inventory
{
    public class BernacleInventory : InventoryItemBase, IInventoryItemUsable, IInventoryStackable
    {
        public bool CanBeUsedFromInventory { get; set; } = true;

        [SerializeField] private ushort _maxStack = 10;
        public ushort MaxStack => _maxStack;

        public event Action OnItemUsed;
        public event Action<ushort, ushort> CanBeUsedFromInventoryChanged;

#pragma warning disable
        public event Action<string> UseTextSlotChanged;

        public void InvokeOnCanBeUsedFromInventoryChanged()
        {
            CanBeUsedFromInventoryChanged?.Invoke(this.GlobalItemID, this.ItemSpecificID);
        }

        public void UseFromInventory()
        {
            OnItemUsed?.Invoke();
        }

        public string UseTextSlot()
        {
            return "";
        }
    }

}
