using LightHouse.Inventory;
using System;
using UnityEngine;

namespace LightHouse.Items.Samples
{
    public class Vis : Key, IInventoryStackable
    {
        [SerializeField] private int _maxStack = 10;
        public int MaxStack => _maxStack;

        [field: SerializeField] public int CurrentStack { get; set; } = 1;
        public IInventoryItem Item { get => this; }

        public bool CanStackWith(IInventoryItem other)
        {
            return other is Vis;
        }

        public int AddToStack(int amount)
        {
            CurrentStack = Mathf.Min(CurrentStack + amount, MaxStack);
            return CurrentStack;
        }

        public int RemoveFromStack(int amount)
        {
            int toRemove = Mathf.Min(amount, CurrentStack);
            CurrentStack -= toRemove;
            return toRemove;
        }
    }
}
