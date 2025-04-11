namespace LightHouse.Items.Samples
{
    public class IDUseItemHideTracker : IDUseItemTracker
    {
        #region IInventoryUsable Callbacks
        protected override void Usable_OnItemUsed()
        {
            if (_inventoryItemUsable == null) return;
            base.Usable_OnItemUsed();
            this.gameObject.SetActive(false);
        }
        #endregion
    }

}