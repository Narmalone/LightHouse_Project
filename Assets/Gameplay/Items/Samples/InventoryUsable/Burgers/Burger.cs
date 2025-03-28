using LightHouse.Inputs;
using LightHouse.Interactions;
using System;
using UnityEngine;
using LightHouse.Inventory;

public class Burger : ItemBase, IInteractable, IInventoryItem, IInventoryItemUsable
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

    public event Action ForceRemoveItemInInventory;
    public event Action OnNameUpdated;
    public event Action OnInteractionNameChanged;
    public event Action OnObjectInteracted;
    public event Action OnItemUsed;
    public event Action<IInventoryItem> CanBeUsedFromInventoryChanged;

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
        Eat();
        OnObjectInteracted?.Invoke();
    }

    public void Eat()
    {
        ForceRemoveItemInInventory?.Invoke();
        Destroy(this.gameObject);
    }

    public string UseInInventoryText()
    {
        return $"Press {InputManager.GetBindingName(InputManager.InteractInInventory)} to eat.";
    }

    public GameObject GetGameObject() => this.gameObject;

    public void InvokeOnCanBeUsedFromInventoryChanged()
    {
        throw new NotImplementedException();
    }
}
