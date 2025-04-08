using System;
using UnityEngine;
using LightHouse.Interactions;

namespace LightHouse.Inventory
{
    public interface IInventoryItem : IItemName
    {
        public ushort ID { get; set; }
        public ushort SpecificID { get; set; }
        public event Action<Vector3, float, bool, bool, IInventoryItem> ForceDropItemInInventory;
        public bool IsItemInInventory { get; set; }
        public bool IsItemOnHands { get; set; }
        public Sprite ItemSprite { get; }
        string GetPickupName();
        Rigidbody GetRigidBody();
    }
}
