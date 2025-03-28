using UnityEngine;

namespace LightHouse.Items.Samples
{
    public class DestroyableItem : KeyItemUseTracker
    {

        #region IInventoryUsable Callbacks
         
        protected override void Usable_OnItemUsed()
        {
            base.Usable_OnItemUsed();
            Destroy(this.gameObject);
        }
        #endregion
    }

}