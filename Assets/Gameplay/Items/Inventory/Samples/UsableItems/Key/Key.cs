using UnityEngine;
using LightHouse.Inputs;
using System;
using LightHouse.Inventory;

namespace LightHouse.Items.Samples
{
    //You should use this class to make inventory with key for other objects
    public class Key : MonoBehaviour, IInventoryItem, IInventoryItemUsable
    {
        #region SERIALIZED FIELDS
        [Header("KEY Fields")]
        [SerializeField] protected string _name;
        [SerializeField] protected Collider _col;
        [SerializeField] protected Rigidbody _rb;
        [SerializeField] private Sprite _keySprite;
        #endregion

        #region IInventory Fields
        [Header("ReadOnly")]
        [field: SerializeField] public ushort GlobalItemID { get; set; }
        [field: SerializeField] public ushort ItemSpecificID { get; set; }
        [field: SerializeField] public bool IsItemInInventory { get; set; }
        [field: SerializeField] public bool IsItemRaycasted { get; set; }
        [field: SerializeField] public bool CanBeRaycasted { get; set; } = true;
        [field: SerializeField] public bool IsItemOnHands { get; set; }
        [field: SerializeField] public bool CanBeUsedFromInventory { get; set; }

        public Sprite ItemSprite => _keySprite;

        public event Action OnNameUpdated;
        public event Action OnItemUsed;
        public event Action<ushort, ushort> CanBeUsedFromInventoryChanged;
        public event Action<ushort, ushort, Vector3, float, bool> ForceDropItemFromInventory;

        #endregion

        #region IInventoryItemName
        public virtual string GetName() => _name;
        public virtual Collider GetCollider() => _col;
        public GameObject GetGameObject() => this.gameObject;
        #endregion

        #region IInventoryItem Functions
        public virtual Rigidbody GetRigidBody() => _rb;
        public virtual string GetPickupName() => $"Press {InputManager.GetBindingName(InputManager.PickUp)} to pick";
        public virtual void UseFromInventory() => OnItemUsed?.Invoke();
        public virtual string UseInInventoryText() => $"Press {InputManager.GetBindingName(InputManager.InteractInInventory)} to use";
        public void InvokeOnCanBeUsedFromInventoryChanged() => CanBeUsedFromInventoryChanged?.Invoke(this.GlobalItemID, this.ItemSpecificID);
        public void InvokeForceDropItemFromInventory(Vector3 pos, float force, bool enablePhysics) => ForceDropItemFromInventory?.Invoke(this.GlobalItemID, this.ItemSpecificID, pos, force, enablePhysics);

        #endregion
    }
}
