using LightHouse.Inventory;
using LightHouse.Items.Inventory;
using UnityEngine;

namespace LightHouse.Items.Interactable
{
    public class BucketFaucet : IDUseItemTracker
    {
        protected override void Usable_OnItemUsed()
        {
            Debug.Log("on used");
            if(_inventoryItemUsable is Bucket bucket)
            {
                Debug.Log("il va être remplis le con");
                bucket.IsFilledWithWater = true;
            }
        }
    }

}
