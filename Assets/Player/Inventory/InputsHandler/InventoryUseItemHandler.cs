using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LightHouse.Inventory
{
    public class InventoryUseItemHandler
    {
        private IInventoryItemUsable _usableItem;

        public void SetTarget(IInventoryItemUsable inventoryUsable)
        {
            _usableItem = inventoryUsable;
        }
    }
}
