using LightHouse.Inventory;
using UnityEngine;
using LightHouse.Items;
using LightHouse.Interactions;
using System;
using LightHouse.Inputs;

public class JerricanEssence : ItemBase, IInteractable, IDescribable, IInventoryItem
{
    [SerializeField] private string _name;
    [SerializeField] private Collider _collider;
    [SerializeField] private Rigidbody _rb;
    public float essenceValue;

    public event Action OnDescriptionUpdated;
    public event Action OnNameUpdated;

    public string GetDescription()
    {
        return "Pick Jerrican Essence with ";
    }

    public string GetName()
    {
        return _name;
    }

    /// <summary>
    /// Fonction qui est lancée quand le joueur regarde l'objet et appuie sur
    /// la touche d'interaction
    /// </summary>
    public void Interact()
    {
        essenceValue -= 10;
        //récupérer dans l'inventaire
        
    }

    public ItemBase GetItem()
    {
        return this;
    }

    public Collider GetCollider()
    {
        return _collider;
    }

    public Rigidbody GetRigidBody()
    {
        return _rb;
    }

    public string GetPickupName()
    {
        return $"Press {InputManager.GetBindingName(InputManager.PLAYER_INPUTS_ACTIONS.Player.Pickup)} to pickup";
    }

    public void OnItemAddedToInventory()
    {
        Debug.Log("Cet objet à été ajouté à l'inventaire " + gameObject.name);
    }

    public void OnItemRemovedFromInventory()
    {
        Debug.Log("Cet objet à été retiré de l'inventaire " + gameObject.name);

    }
}
