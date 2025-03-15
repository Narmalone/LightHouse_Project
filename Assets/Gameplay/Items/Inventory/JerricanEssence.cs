using LightHouse.Inventory;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LightHouse.Items;
using LightHouse.Interactions;
using System;

public class JerricanEssence : MonoBehaviour, IInteractable, IDescribable, IItem
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

    public void Interact()
    {
        essenceValue -= 10;
        //récupérer dans l'inventaire
        PlayerInventory.Instance.AddItem(this);
    }

    public GameObject GetGameObject()
    {
        return this.gameObject;
    }

    public Collider GetCollider()
    {
        return _collider;
    }

    public Rigidbody HasRigidBody()
    {
        return _rb;
    }
}
