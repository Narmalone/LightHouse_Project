using LightHouse.Inputs;
using LightHouse.Interactions;
using System;
using UnityEngine;
using LightHouse.Inventory;

public class Burger : MonoBehaviour, IInteractable, IInventoryItem, IInventoryItemUsable
{
    [SerializeField] private Collider _burgerCollider;
    [SerializeField] private Rigidbody _rb;
    [SerializeField] private string _burgerName = "Burger";

    public bool IsItemInInventory { get; set; }
    [field: SerializeField] public bool CanBeUsedFromInventory { get; set; } = true;
    [field: SerializeField] public bool CanBeInteracted { get; set; } = true;
    public bool IsItemRaycasted { get; set; }
    public bool CanBeRaycasted { get; set; } = true;
    [field: SerializeField] public bool IsItemOnHands { get; set; }

    [SerializeField] private Sprite _burgerSprite;
    public Sprite ItemSprite => _burgerSprite;

    [field: SerializeField] public ushort GlobalItemID { get; set; }
    [field: SerializeField] public ushort ItemSpecificID { get; set; }
    [field: SerializeField] public Vector3 InventoryLocalPositionOffset { get; set; }
    [field: SerializeField] public Vector3 InventoryEulerAnglesForLocalRotation { get; set; }

    public event Action OnNameUpdated;
    public event Action OnInteractionNameChanged;
    public event Action OnObjectInteracted;
    public event Action OnItemUsed;
    public event Action<ushort, ushort> CanBeUsedFromInventoryChanged;

    public event Action<ushort, ushort, Vector3, float, bool> ForceDropItemFromInventory;

    #region IInventoryItem Method

    public Collider GetCollider() => _burgerCollider;

    public string GetName() => _burgerName;

    public string GetPickupName()
    {
        return $"Press {InputManager.GetBindingName(InputManager.PickUp)} to pickup.";
    }

    public Rigidbody GetRigidBody() => _rb;
    public void UseFromInventory()
    {
        Eat();
        OnItemUsed?.Invoke();
    }
    #endregion

    public string GetInteractionName()
    {
        return $"Press {InputManager.GetBindingName(InputManager.Interact)} to eat.";
    }

    public void Interact()
    {
        OnObjectInteracted?.Invoke();
        Eat();
    }

    public void Eat()
    {
        if (IsItemInInventory)
            ForceDropItemFromInventory?.Invoke(this.GlobalItemID, this.ItemSpecificID, transform.position, 0f, false);
        this.gameObject.SetActive(false);
        Destroy(this.gameObject);
    }

    public string UseInInventoryText()
    {
        return $"Press {InputManager.GetBindingName(InputManager.InteractInInventory)} to eat.";
    }

    public GameObject GetGameObject() => this.gameObject;

    public void InvokeOnCanBeUsedFromInventoryChanged()
    {
        CanBeUsedFromInventoryChanged?.Invoke(this.GlobalItemID, this.ItemSpecificID);
    }

    public void InvokeForceDropItemFromInventory(Vector3 pos, float force, bool enablePhysics)
    {
        ForceDropItemFromInventory?.Invoke(this.GlobalItemID, this.ItemSpecificID, pos, force, enablePhysics);
    }
}
