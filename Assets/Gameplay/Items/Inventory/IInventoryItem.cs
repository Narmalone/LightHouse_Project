using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IInventoryItem
{
    string GetPickupName();
    ItemBase GetItem();
    Collider GetCollider();
    Rigidbody GetRigidBody();
    void OnItemAddedToInventory();
    void OnItemRemovedFromInventory();
}