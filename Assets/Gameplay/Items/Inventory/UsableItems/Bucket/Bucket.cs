using LightHouse.Interactions;
using LightHouse.Items.Interactable;
using UnityEngine;

namespace LightHouse.Items.Inventory
{
    public class Bucket : Key, IItemCallback
    {
        [Header(" --- BUCKET --- ")]
        [SerializeField] private MopBucketWetItemTracker _mopTracker;

        [SerializeField] private ItemColliderMarker _itemColliderMarker;
        public bool IsFilledWithWater = false;

        private void Start()
        {
            _mopTracker.gameObject.SetActive(false);
        }

        public void OnRaycastEnd()
        {
            if (IsFilledWithWater)
            {
            
            }
        }

        public void OnRaycastStart()
        {
            if (IsFilledWithWater)
            {
                _itemColliderMarker.TargetComponent = _mopTracker.gameObject;
                _itemColliderMarker.RegisterToItem();
                if (!_mopTracker.gameObject.activeInHierarchy)
                    _mopTracker.gameObject.SetActive(true);
            }
        }

        public override void UseFromInventory()
        {
            base.UseFromInventory();
        }
    }

}
