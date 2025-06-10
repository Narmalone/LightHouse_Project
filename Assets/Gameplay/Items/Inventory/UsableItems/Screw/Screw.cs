using System;
using UnityEngine;
using LightHouse.Inventory;

namespace LightHouse.Items.Inventory
{
    public class Screw : InventoryItemBase, IInventoryItemUsable, IInventoryStackable
    {
        public event Action OnItemUsed;
        public event Action<ushort, ushort> CanBeUsedFromInventoryChanged;
        public event Action<string> UseTextSlotChanged;
        public bool CanBeUsedFromInventory { get; set; }
        [field: SerializeField] public float UseHoldTime { get; set; } = 0.5f;

        [SerializeField] private ushort _maxStack = 10;
        public ushort MaxStack => _maxStack;
        
        public string UseTextSlot()
        {
            return "Use Screw";
        }

        public void UseFromInventory()
        {
            OnItemUsed?.Invoke();
        }

        public void InvokeOnCanBeUsedFromInventoryChanged()
        {
            CanBeUsedFromInventoryChanged?.Invoke(this.GlobalItemID, this.ItemSpecificID);
        }
        
    }
}

