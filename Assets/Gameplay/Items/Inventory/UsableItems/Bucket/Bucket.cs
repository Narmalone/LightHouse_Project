using System;
using System.Collections;
using LightHouse.Game.BootStrap;
using LightHouse.Handlers;
using LightHouse.Interactions;
using LightHouse.Inventory;
using LightHouse.Items.Interactable;
using LightHouse.KinematicCharacterController;
using UnityEngine;

namespace LightHouse.Items.Inventory
{
    public class Bucket : Key, IItemCallback, IInventoryItemCallback
    {
        [Header(" --- BUCKET --- ")]
        [SerializeField] private MopBucketWetItemTracker _mopTracker;

        [SerializeField] private ItemColliderMarker _itemColliderMarker;
        public bool IsFilledWithWater = false;
        private bool isMopShowed = false;

        private bool _isInitialized = false;
        protected override void Awake()
        {
            base.Awake();

            _mopTracker.OnConditionChecked += OnConditionChecked;
            BootStrap.OnGameAssetsLoaded += BootStrap_OnGameAssetsLoaded;
        }

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
        private void BootStrap_OnGameAssetsLoaded()
        {
            _isInitialized = true;
            IgnoreMopColliderCollision();
        }

        private void OnConditionChecked()
        {
            Debug.Log("Condition checked");
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            _mopTracker.OnConditionChecked -= OnConditionChecked;
            BootStrap.OnGameAssetsLoaded -= BootStrap_OnGameAssetsLoaded;
        }

        public void OnRaycastEnd() { }

        public void OnRaycastStart()
        {
            Debug.Log("on raycast start");
        }

        public override void UseFromInventory()
        {
            base.UseFromInventory();
        }

        public void OnItemAddedToInventory()
        {
            foreach(var col in _itemColliderMarker.Colliders)
            {
                col.enabled = false;
            }
        }

        public void OnItemRemovedFromInventory()
        {
            foreach (var col in _itemColliderMarker.Colliders)
            {
                col.enabled = true;
            }
        }
    }

}
