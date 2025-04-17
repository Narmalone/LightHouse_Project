using LightHouse.Inputs;
using LightHouse.Inventory;
using System;
using UnityEngine;

namespace LightHouse.Items
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

        [SerializeField] private Sprite _jerricanSprite;
        public Sprite ItemSprite => _jerricanSprite;

        [field: SerializeField] public ushort GlobalItemID { get; set; }
        [field: SerializeField] public ushort ItemSpecificID { get; set; }
        [field: SerializeField] public Vector3 InventoryLocalPositionOffset { get; set; }
        [field: SerializeField] public Vector3 InventoryEulerAnglesForLocalRotation { get; set; }

        public event Action OnItemUsed;
        public event Action<ushort, ushort> CanBeUsedFromInventoryChanged;
        public event Action OnNameUpdated;
        public event Action<ushort, ushort, Vector3, float, bool> ForceDropItemFromInventory;

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
            CanBeUsedFromInventoryChanged?.Invoke(this.GlobalItemID, this.ItemSpecificID);
        }

        public void UseFromInventory()
        {
            OnItemUsed?.Invoke();
        }
        public string UseInInventoryText()
        {
            return $"Press {InputManager.GetBindingName(InputManager.InteractInInventory)} to use.";
        }

        public void InvokeForceDropItemFromInventory(Vector3 pos, float force, bool enablePhysics)
        {
            ForceDropItemFromInventory?.Invoke(this.GlobalItemID, this.ItemSpecificID, pos, force, enablePhysics);
        }
    }

}
