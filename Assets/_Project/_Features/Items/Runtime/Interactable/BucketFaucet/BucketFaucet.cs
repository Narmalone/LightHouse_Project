using LightHouse.Features.Items.Inventory.Bucket;

namespace LightHouse.Features.Items.Interactable
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
