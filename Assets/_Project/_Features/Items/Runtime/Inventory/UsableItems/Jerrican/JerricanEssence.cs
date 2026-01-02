using LightHouse.Inputs;
using LightHouse.Inventory;
using System;
using UnityEngine;

namespace LightHouse.Items.Inventory
{
    public class JerricanEssence : InventoryItemBase, IInventoryItemUsable
    {
        [Header("Jerrican Fields")]
        [SerializeField] private float _essenceAmount = 100f;
        public float EssenceAmount => _essenceAmount;

        [Header("IInventory Item Usable Field")]
        [field: SerializeField] public bool CanBeUsedFromInventory { get; set; } = false;

        [field: SerializeField] public float UseHoldTime { get; set; } = 0.5f;

        public event Action OnItemUsed;
        public event Action<ushort, ushort> CanBeUsedFromInventoryChanged;

#pragma warning disable
        public event Action<string> UseTextSlotChanged;

        public void InvokeOnCanBeUsedFromInventoryChanged()
        {
            CanBeUsedFromInventoryChanged?.Invoke(this.GlobalItemID, this.ItemSpecificID);
        }

        public void UseFromInventory()
        {
            OnItemUsed?.Invoke();
        }
        public string UseTextSlot()
        {
            return $"Press {InputManager.GetBindingName(InputManager.InteractInInventory)} to use.";
        }
    }

}
