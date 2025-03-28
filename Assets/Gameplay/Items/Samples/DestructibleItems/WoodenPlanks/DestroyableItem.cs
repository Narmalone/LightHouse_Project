using UnityEngine;

namespace LightHouse.Items.Samples
{
    public class DestroyableItem : KeyItemUseTracker
    {
        #region FIELDS
        [Header("Main Settings")]
        [SerializeField] protected string _itemName = "Wooden Plank";
        [SerializeField] protected KeyType _neededKey;
        #endregion

        #region IInventoryUsable Callbacks
         
        protected override void Usable_OnItemUsed()
        {
            base.Usable_OnItemUsed();
            Destroy(this.gameObject);
        }
        #endregion
    }

}