using System;
using UnityEngine;

namespace LightHouse.Features.Items.Inventory.Nail
{
    public class Nail : Key, IInventoryStackable
    {
        [SerializeField] private ushort _maxStack = 10;
        public ushort MaxStack => _maxStack;
    }
}
