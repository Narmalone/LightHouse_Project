using System;
using UnityEngine;

namespace LightHouse.Inventory
{
    public interface IInventoryStackable
    {
        public ushort MaxStack { get; }
    }
}
