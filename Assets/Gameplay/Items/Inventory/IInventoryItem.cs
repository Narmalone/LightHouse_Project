using System;
using UnityEngine;
using LightHouse.Interactions;

namespace LightHouse.Inventory
{
    public interface IInventoryItem : IItemName
    {
        public event Action ForceRemoveItemInInventory;
        public bool IsItemInInventory { get; set; }
        public bool IsItemOnHands { get; set; }
        public Sprite ItemSprite { get; }
        string GetPickupName();
        Rigidbody GetRigidBody();
    }
}
