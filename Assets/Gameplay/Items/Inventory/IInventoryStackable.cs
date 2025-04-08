using System;
using UnityEngine;

namespace LightHouse.Inventory
{
    public interface IInventoryStackable
    {
        public IInventoryItem Item { get; }
        public ushort MaxStack { get; }
        public int CurrentStack { get; set; }
        public bool CanStackWith(IInventoryItem otherItem);
        public int AddToStack(int amount);
        public int RemoveFromStack(int amount);
    }
}
