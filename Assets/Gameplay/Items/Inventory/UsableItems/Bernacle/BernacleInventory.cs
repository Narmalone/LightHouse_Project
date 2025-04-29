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

        public int CurrentStack { get; set; } = 1;

        public event Action OnItemUsed;
        public event Action<ushort, ushort> CanBeUsedFromInventoryChanged;
        public event Action<string> UseTextSlotChanged;

        public void InvokeOnCanBeUsedFromInventoryChanged()
        {
            
        }

        public void UseFromInventory()
        {
            
        }

        public string UseTextSlot()
        {
            return "";
        }
    }

}
