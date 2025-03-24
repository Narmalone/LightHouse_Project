using System;

namespace LightHouse.Inventory
{
    public interface IInventoryStackable
    {
        int MaxStack { get; }
        int CurrentStack { get; set; }
        bool CanStackWith(IInventoryItem otherItem);
        int AddToStack(int amount);
        int RemoveFromStack(int amount);
    }
}
