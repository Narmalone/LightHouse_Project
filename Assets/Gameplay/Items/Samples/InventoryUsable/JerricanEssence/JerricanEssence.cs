using LightHouse.Inputs;
using LightHouse.Inventory;
using System;
using UnityEngine;

namespace LightHouse.Items.Samples
{
    public class JerricanEssence : MonoBehaviour, IInventoryItem, IInventoryItemUsable
    {
        [Header("Jerrican Fields")]
        [SerializeField] private string _itemName;
        [SerializeField] private Collider _jerricanCollider;
        [SerializeField] private Rigidbody _rb;
        [SerializeField] private float _essenceAmount = 100f;
        public float EssenceAmount => _essenceAmount;

        [Header("IInventory Items Fields")]
        [field: SerializeField] public bool IsItemInInventory { get; set; }
        [field: SerializeField] public bool IsItemOnHands { get; set; }
        [field: SerializeField] public bool CanBeRaycasted { get; set; }
        [field: SerializeField] public bool IsItemRaycasted { get; set; }

        [Header("IInventory Item Usable Field")]
        [field: SerializeField] public bool CanBeUsedFromInventory { get; set; } = false;

        public event Action OnItemUsed;
        public event Action<IInventoryItem> CanBeUsedFromInventoryChanged;
        public event Action ForceRemoveItemInInventory;
        public event Action OnNameUpdated;

        public string GetName() => _itemName;
        public string GetPickupName()
        {
            return $"Press {InputManager.GetBindingName(InputManager.PickUp)} to pick up.";
        }
        public Rigidbody GetRigidBody() => _rb;
        public GameObject GetGameObject() => this.gameObject;
        public Collider GetCollider() => _jerricanCollider;

        public void InvokeOnCanBeUsedFromInventoryChanged()
        {
            CanBeUsedFromInventoryChanged?.Invoke(this);
        }

        public void UseFromInventory()
        {
            OnItemUsed?.Invoke();
        }
        public string UseInInventoryText()
        {
            return $"Press {InputManager.GetBindingName(InputManager.InteractInInventory)} to use.";
        }

        public JerricanEssence InvokeRemoveItemInInventory()
        {
            ForceRemoveItemInInventory?.Invoke();
            return this;
        }

    }

}
