using System;
using LightHouse.Inventory;
using UnityEngine;

namespace LightHouse.Items.Inventory
{
    public class Scotch : InventoryItemBase, IInventoryItemUsable
    {
        public event Action OnItemUsed;
        public event Action<ushort, ushort> CanBeUsedFromInventoryChanged;
        public event Action<string> UseTextSlotChanged;
        public bool CanBeUsedFromInventory { get; set; }
        
        public string UseTextSlot()
        {
            return "Use Scotch";
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
