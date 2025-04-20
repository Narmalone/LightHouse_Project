using System;
using LightHouse.Inputs;
using LightHouse.Inventory;
using UnityEngine;

public abstract class InventoryItemBase : MonoBehaviour, IInventoryItem
{
    [Header("Inventory Item Base")]
    [SerializeField] protected string _name;
    [SerializeField] protected Sprite _inventorySprite;
    [SerializeField] protected Collider _detectionCollider;
    [SerializeField] protected Rigidbody _rb;
    [field: SerializeField] public Vector3 InventoryLocalPositionOffset { get; set; }
    [field: SerializeField] public Vector3 InventoryEulerAnglesForLocalRotation { get; set; }
    [field: SerializeField] public bool CanBeRaycasted { get; set; } = true;

    public Sprite ItemSprite => _inventorySprite;
    [field: SerializeField] public ushort GlobalItemID { get; set; }
    [field: SerializeField] public ushort ItemSpecificID { get; set; }
    public bool IsItemInInventory { get; set; }
    public bool IsItemOnHands { get; set; }
 
    public bool IsItemRaycasted { get; set; }

    public event Action<ushort, ushort, Vector3, float, bool> ForceDropItemFromInventory;

#pragma warning disable
    public event Action OnNameUpdated;

    public virtual string GetName() => _name;
    public virtual GameObject GetGameObject() => this.gameObject;
    public virtual Collider GetCollider() => _detectionCollider;
    public virtual Rigidbody GetRigidBody() => _rb;

    public virtual string GetPickupName()
        => $"Press {InputManager.GetBindingName(InputManager.PickUp)} to pick up.";

    public void InvokeForceDropItemFromInventory(Vector3 pos, float force, bool enablePhysics) 
        => ForceDropItemFromInventory?.Invoke(this.GlobalItemID, this.ItemSpecificID, pos, force, enablePhysics);

}
