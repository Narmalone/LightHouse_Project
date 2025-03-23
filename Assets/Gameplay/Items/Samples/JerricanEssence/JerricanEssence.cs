using LightHouse.Inputs;
using LightHouse.Inventory;
using System;
using UnityEngine;

namespace LightHouse.Items.Samples
{
    public class JerricanEssence : MonoBehaviour, IInventoryItem
    {
        [SerializeField] private string _itemName;
        [SerializeField] private Collider _jerricanCollider;
        [SerializeField] private Rigidbody _rb;
        [SerializeField] private float _essenceAmount = 100f;
        
        public float EssenceAmount => _essenceAmount;
        public bool IsItemInInventory { get; set; }
        public bool IsItemRaycasted { get; set; }
        [field: SerializeField] public bool CanBeRaycasted { get; set; } = true;

        public event Action ForceRemoveItemInInventory;
        public event Action OnNameUpdated;

        public Collider GetCollider() => _jerricanCollider;

        public GameObject GetGameObject() => this.gameObject;
        public Rigidbody GetRigidBody() => _rb;

        public string GetName()
        {
            return _itemName;
        }

        public string GetPickupName()
        {
            return $"Press {InputManager.GetBindingName(InputManager.PickUp)} to pick.";
        }

        public JerricanEssence InvokeRemoveItemInInventory()
        {
            ForceRemoveItemInInventory?.Invoke();
            return this;
        }
    }

}
