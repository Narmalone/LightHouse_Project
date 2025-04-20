using UnityEngine;
using LightHouse.Inputs;
using System;
using LightHouse.Inventory;

namespace LightHouse.Items.Samples
{
    //You should use this class to make inventory with key for other objects
    public class Key : InventoryItemBase, IInventoryItemUsable
    {
        #region SERIALIZED FIELDS
        [Header("KEY Fields")]
        [SerializeField] private bool _destroyOnUsed;
        [field: SerializeField] public bool CanBeUsedFromInventory { get; set; }
        #endregion

        #region IInventory Fields
        public event Action OnItemUsed;
        public event Action<ushort, ushort> CanBeUsedFromInventoryChanged;
        #endregion

        #region IInventoryItem Functions
        public virtual void UseFromInventory()
        {
            OnItemUsed?.Invoke();
            InvokeForceDropItemFromInventory(transform.position, 0.0f, false);
            if (_destroyOnUsed)
                Destroy(this.gameObject);
        }
        public virtual string UseInInventoryText() => $"Press {InputManager.GetBindingName(InputManager.InteractInInventory)} to use";
        public void InvokeOnCanBeUsedFromInventoryChanged() => CanBeUsedFromInventoryChanged?.Invoke(this.GlobalItemID, this.ItemSpecificID);

        #endregion
    }
}
