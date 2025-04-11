using System;
using UnityEngine;
using LightHouse.Interactions;

namespace LightHouse.Inventory
{
    public interface IInventoryItem : IItemName
    {
        public ushort GlobalItemID { get; set; }
        public ushort ItemSpecificID { get; set; }
        /// <summary>
        /// GlobalID, Specific ID, Position, Force, EnablePhysicsOnDrop
        /// </summary>
        public event Action<ushort, ushort, Vector3, float, bool> ForceDropItemFromInventory;
        public bool IsItemInInventory { get; set; }
        public bool IsItemOnHands { get; set; }
        public Sprite ItemSprite { get; }
        string GetPickupName();
        Rigidbody GetRigidBody();
        void InvokeForceDropItemFromInventory(Vector3 pos, float force, bool enablePhysics);
    }
}
