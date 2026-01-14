using LightHouse.Features.Interactions;
using System;
using UnityEngine;

namespace LightHouse.Features.Items.Inventory
{
    public interface IInventoryItem : IItemName
    {
        /// <summary>
        /// This is the main ID of the item, each woodplanks for example have ID 3.
        /// <para> Mainly setted by the <see cref="ItemDatabase"/> </para>
        /// </summary>
        public ushort GlobalItemID { get; set; }

        /// <summary>
        /// The Specific ID of the item while in inventory.
        /// It's setted via the <see cref="InventoryPool"/> and to recover
        /// the intance of the item in the inventory.
        /// </summary>
        public ushort ItemSpecificID { get; set; }

        /// <summary>
        /// An event to drop an item where the inventory automatically subscribe with.
        /// <para>Parameters: GlobalID, Specific ID, Position, Force, EnablePhysicsOnDrop </para>
        /// </summary>
        public event Action<ushort, ushort, Vector3, float, bool> ForceDropItemFromInventory;

        /// <summary>
        /// A boolean to pickup or not the item. it's checked by the PlayerInventoryManayer
        /// </summary>
        public bool CanBePickedUp { get; set; }

        public event Action<string> OnPickupTextUpdated;

        /// <summary>
        /// Automatically setted by the <see cref="PlayerInventorManager"/> to let you know if the instance
        /// is inside the inventory / pool
        /// </summary>
        public bool IsItemInInventory { get; set; }

        /// <summary>
        /// Currently not used.
        /// </summary>
        public bool IsItemOnHands { get; set; }

        /// <summary>
        /// The sprite that appears to the <see cref="ItemSlot"/> in the inventory.
        /// </summary>
        public Sprite ItemSprite { get; }

        /// <summary>
        /// While holded in inventory, representing an offset local Position
        /// Used by <see cref="VisualItemInventory"/>
        /// </summary>
        public Vector3 InventoryLocalPositionOffset { get; set; }

        /// <summary>
        /// While holded in inventory, representing the target Local Euler angles
        /// Used by <see cref="VisualItemInventory"/>
        /// </summary>
        public Vector3 InventoryEulerAnglesForLocalRotation { get; set; }


        /// <summary>
        /// Automatically retrieved by the <see cref="PlayerInventory"/> and <seealso cref="CameraRaycastDetector"/>.
        /// </summary>
        /// <returns>The text to display to pickup the item.</returns>
        string GetPickupName();

        /// <summary>
        /// Put the rigidbody to manipulate on the pool (The main rigidbody of an item).
        /// </summary>
        Rigidbody GetRigidBody();

        /// <summary>
        /// Force to Invoke the event <see cref="ForceDropItemFromInventory"/>.
        /// </summary>
        void InvokeForceDropItemFromInventory(Vector3 pos, float force, bool enablePhysics);
    }
}
