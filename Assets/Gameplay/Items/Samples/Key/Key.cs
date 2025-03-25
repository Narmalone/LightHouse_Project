using UnityEngine;
using LightHouse.Inputs;
using System;
using LightHouse.Inventory;

namespace LightHouse.Items.Samples
{
    //You should use this class to make inventory with key for other objects
    public class Key : MonoBehaviour, IInventoryItem
    {
        #region SERIALIZED FIELDS
        [Header("Fields")]
        [SerializeField] protected string _Name;
        [SerializeField] protected Collider _Collider;
        [SerializeField] protected Rigidbody _Rigidbody;
        [SerializeField] protected KeyType _key;
        private bool _hasKey = false;

        public bool HasKeyInInventory => _hasKey;

        #endregion

        #region PROPERTIES
        public KeyType KeyType => _key;
        #endregion

        #region IInventory Fields
        [field: SerializeField] public bool IsItemInInventory { get; set; }
        [field: SerializeField] public bool IsItemRaycasted { get; set; }
        [field: SerializeField] public bool CanBeRaycasted { get; set; } = true;

        public KeyType ItemKeyType => _key;

        [field: SerializeField] public bool IsItemOnHands { get; set; }

        public event Action ForceRemoveItemInInventory;
        public event Action OnNameUpdated;

        #endregion

        #region IInventoryItemName
        public virtual Collider GetCollider() => _Collider;
        public GameObject GetGameObject() => this.gameObject;

        #endregion

        #region IInventoryItem Functions

        public virtual Rigidbody GetRigidBody() => _Rigidbody;
        public virtual string GetName() => _Name;

        public virtual string GetPickupName() => $"Press {InputManager.GetBindingName(InputManager.PickUp)} to pick";

        public virtual void OnItemAddedToInventory() { }

        public virtual void OnItemRemovedFromInventory() { }

        public virtual void UseFromInventory() { }

        public virtual string UseInInventoryText() => $"Press {InputManager.GetBindingName(InputManager.InteractInInventory)} to use";

        public virtual void ForceRemoveItemFromInventory()
        {
            ForceRemoveItemInInventory?.Invoke();
        }


        #endregion
    }
}
