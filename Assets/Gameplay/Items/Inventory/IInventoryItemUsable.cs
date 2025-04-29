using System;

namespace LightHouse.Inventory
{
    public interface IInventoryItemUsable 
    {
        public event Action OnItemUsed;
        public event Action<ushort, ushort> CanBeUsedFromInventoryChanged;
        public event Action<string> UseTextSlotChanged;
        public bool CanBeUsedFromInventory { get; set; }
        string UseTextSlot();
        void UseFromInventory();
        void InvokeOnCanBeUsedFromInventoryChanged();
    }

}
