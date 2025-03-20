using System;
using UnityEngine;
using LightHouse.Interactions;

public interface IInventoryItem : IItemName
{
    public event Action ForceRemoveItemInInventory;
    public bool IsItemInInventory { get; set; }
    public bool CanBeUsedFromInventory { get; set; }
    string GetPickupName();
    ItemBase GetItem();
    Collider GetCollider();
    Rigidbody GetRigidBody();
    void OnItemAddedToInventory();
    void OnItemRemovedFromInventory();
    void UseFromInventory();
}