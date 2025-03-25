using LightHouse.Inputs;
using LightHouse.Inventory;
using System;
using UnityEngine;

namespace LightHouse.Items.Samples
{
    public class Hammer : Key, IInventoryItemUsable
    {
        [field : SerializeField] public bool CanBeUsedFromInventory { get; set; }

        public event Action OnItemUsed;
        public event Action<IInventoryItem> CanBeUsedFromInventoryChanged;

        public void InvokeOnCanBeUsedFromInventoryChanged()
        {
            CanBeUsedFromInventoryChanged?.Invoke(this);
        }

        public override void UseFromInventory()
        {
            if (!CanBeUsedFromInventory) return;
            OnItemUsed?.Invoke();
        }

    }

}
