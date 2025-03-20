using LightHouse.Inputs;
using LightHouse.Interactions;
using System;
using UnityEngine;

public class Burger : ItemBase, IInteractable, IInventoryItem
{
    [SerializeField] private Collider _burgerCollider;
    [SerializeField] private Rigidbody _rb;
    [SerializeField] private string _burgerName = "Burger";

    public bool IsItemInInventory { get; set; }
    [field: SerializeField] public bool CanBeUsedFromInventory { get; set; } = true;
    [field: SerializeField] public bool CanBeInteracted { get; set; } = true;

    public event Action ForceRemoveItemInInventory;
    public event Action OnNameUpdated;
    public event Action OnInteractionNameChanged;
    public event Action OnObjectInteracted;

    #region IInventoryItem Method

    public Collider GetCollider() => _burgerCollider;
    public ItemBase GetItem() => this;

    public string GetName() => _burgerName;

    public string GetPickupName()
    {
        return $"Press {InputManager.GetBindingName(InputManager.PickUp)} to pickup.";
    }

    public Rigidbody GetRigidBody() => _rb;

    public void OnItemAddedToInventory()
    {
        
    }

    public void OnItemRemovedFromInventory()
    {
        
    }

    public void UseFromInventory()
    {
        Eat();
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
        Debug.Log("Le burger a été mangé !");
        ForceRemoveItemInInventory?.Invoke();
        //gameObject.SetActive(false);
        Destroy(this.gameObject);
    }

}
