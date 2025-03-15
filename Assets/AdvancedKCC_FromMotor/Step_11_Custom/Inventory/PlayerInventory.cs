using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LightHouse.Interactions;

namespace LightHouse.Inventory
{
    public class PlayerInventory : MonoBehaviour
    {
        private byte _inventoryCapacity = 4;

        //Inventory data, copy of the item and int is for the stacks
        public Dictionary<System.Type, InventorySlot> Inventory = new();

        public static PlayerInventory Instance;

        private void Awake()
        {
            Instance = this;
        }

        public void AddItem()
        {

        }

        public void RemoveItem()
        {

        }
    }

}
