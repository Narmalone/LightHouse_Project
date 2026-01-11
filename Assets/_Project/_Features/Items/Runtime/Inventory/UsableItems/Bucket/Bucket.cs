using LightHouse.Core.Player;
using LightHouse.Core.Player.Inventory.Callbacks;
using LightHouse.Features.Items.Interactable.Mop;
using UnityEngine;

namespace LightHouse.Features.Items.Inventory.Bucket
{
    public class Bucket : Key, IInventoryItemCallback
    {
        [Header(" --- BUCKET --- ")]
        [SerializeField] private MopBucketWetItemTracker _mopTracker;

        [SerializeField] private ItemColliderMarker _itemColliderMarker;
        public bool IsFilledWithWater = false;
        private bool _isInitialized = false;
        private void Start()
        {
            if (!_isInitialized)
            {
                IgnoreMopColliderCollision();
                _isInitialized = true;
            }
        }

        public void IgnoreMopColliderCollision()
        {
            if (PlayerHandlerData.MainPlayer == null) return;
            PlayerHandlerData.MainPlayer.Character.IgnoreCollider(_mopTracker.GetCollider());
            Physics.IgnoreCollision(_mopTracker.GetCollider(), PlayerHandlerData.MainPlayer.Character.Motor.Capsule, true);
        }

        public void OnItemAddedToInventory()
        {
            foreach(var col in _itemColliderMarker.Colliders)
            {
                col.enabled = false;
            }
            _mopTracker.GetCollider().enabled = false;
        }

        public void OnItemRemovedFromInventory()
        {
            foreach (var col in _itemColliderMarker.Colliders)
            {
                col.enabled = true;
            }
            _mopTracker.GetCollider().enabled = true;
        }
    }

}
