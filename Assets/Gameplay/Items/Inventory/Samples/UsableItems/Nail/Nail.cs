using LightHouse.Inventory;
using System;
using UnityEngine;

namespace LightHouse.Items.Inventory
{
    public class Nail : Key, IInventoryStackable
    {
        [SerializeField] private ushort _maxStack = 10;
        public ushort MaxStack => _maxStack;
    }
}
