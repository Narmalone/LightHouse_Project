using System;
using UnityEngine;

namespace LightHouse.Features.Items.Inventory.Scotch
{
    public class ScotchController : InventoryItemBase, IInventoryItemUsable
    {
        public event Action OnItemUsed;
        public event Action<ushort, ushort> CanBeUsedFromInventoryChanged;

#pragma warning disable
        public event Action<string> UseTextSlotChanged;
        public bool CanBeUsedFromInventory { get; set; }

        [field: SerializeField] public float UseHoldTime { get; set; } = 0.5f;
        

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
