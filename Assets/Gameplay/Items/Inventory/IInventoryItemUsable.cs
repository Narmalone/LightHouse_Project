using System;

namespace LightHouse.Inventory
{
    public interface IInventoryItemUsable 
    {
        public event Action OnItemUsed;
        public event Action<IInventoryItem> CanBeUsedFromInventoryChanged;
        public bool CanBeUsedFromInventory { get; set; }
        string UseInInventoryText();
        void UseFromInventory();
        void InvokeOnCanBeUsedFromInventoryChanged();
    }

}
