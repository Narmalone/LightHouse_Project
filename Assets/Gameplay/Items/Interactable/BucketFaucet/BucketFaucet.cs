using LightHouse.Items.Inventory;

namespace LightHouse.Items.Interactable
{
    public class BucketFaucet : IDUseItemTracker
    {
        protected override void Usable_OnItemUsed()
        {
            if(_inventoryItemUsable is Bucket bucket)
            {
                bucket.IsFilledWithWater = true;
            }
        }
    }

}
