using System;
using LightHouse.Inventory;
using LightHouse.Items.Inventory;
using UnityEngine;

namespace LightHouse.Items.Interactable
{
    public class MopBucketWetItemTracker : IDUseItemTracker
    {

        protected override void Usable_OnItemUsed()
        {
            base.Usable_OnItemUsed();
            if (_inventoryItemUsable is Mop mop)
            {
                mop.MakeMeWet();
            }
        }
    }

}
