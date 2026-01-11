using LightHouse.Features.Items.Inventory.Mop;

namespace LightHouse.Features.Items.Interactable.Mop
{
    public class MopBucketWetItemTracker : IDUseItemTracker
    {
        protected override void Usable_OnItemUsed()
        {
            base.Usable_OnItemUsed();
            if (_inventoryItemUsable is MopController mop)
            {
                mop.MakeMeWet();
            }
        }
    }

}
