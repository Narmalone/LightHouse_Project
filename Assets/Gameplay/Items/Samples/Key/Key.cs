using UnityEngine;
using LightHouse.Inputs;
using System;

namespace LightHouse.Items.Samples
{
    //You should use this class to make inventory with key for other objects
    public class Key : ItemBase, IInventoryItem
    {
        #region SERIALIZED FIELDS
        [Header("Fields")]
        [SerializeField] protected string _Name;
        [SerializeField] protected Collider _Collider;
        [SerializeField] protected Rigidbody _Rigidbody;
        [SerializeField] protected KeyType _key;

        #endregion

        #region PROPERTIES
        public KeyType KeyType => _key;
        #endregion

        #region IInventory Fields
        [field: SerializeField] public bool IsItemInInventory { get; set; }
        public bool CanBeUsedFromInventory { get; set; }

        public event Action ForceRemoveItemInInventory;
        public event Action OnNameUpdated;

        #endregion

        #region IInventoryItem Functions
        public virtual ItemBase GetItem() => this;

        public virtual Collider GetCollider() => _Collider;

        public virtual Rigidbody GetRigidBody() => _Rigidbody;
        public virtual string GetName() => _Name;

        public virtual string GetPickupName() => $"Press {InputManager.GetBindingName(InputManager.PickUp)} to pick";

        public virtual void OnItemAddedToInventory() { }

        public virtual void OnItemRemovedFromInventory() { }

        public virtual void UseFromInventory() { }

        #endregion
    }
}
