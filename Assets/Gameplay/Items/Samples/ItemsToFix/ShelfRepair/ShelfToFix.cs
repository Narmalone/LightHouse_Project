using LightHouse.Inventory;
using LightHouse.KinematicCharacterController;
using LightHouse.Locators;
using System;
using UnityEngine;

namespace LightHouse.Items.Samples
{
    public class ShelfToFix : BaseItemFixer
    {
        [SerializeField] protected PlaceHolderKeyMover[] _placeHolderPlanks = new PlaceHolderKeyMover[0];
        [SerializeField] private PlaceHolderKeyMover[] _visPlaceHolders = new PlaceHolderKeyMover[0];
        [SerializeField] private short _missingHolderPlanksToFill = short.MaxValue;
        [SerializeField] private short _missingHolderVisToFill = short.MaxValue;
        [SerializeField] private LockedDoor LockedDoor;

        public event Action OnShelfRepaired;

        private bool _hasSettedAllPlanks = false;
        private bool _hasSettedAllVis = false;

        protected void Awake()
        {
            _missingHolderPlanksToFill = (short)_placeHolderPlanks.Length;
            _missingHolderVisToFill = (short)_visPlaceHolders.Length;
            Init();
        }

        private void Start()
        {
            foreach(var vholder in _visPlaceHolders)
            {
                vholder.gameObject.SetActive(false);
            }
        }

        private void Init()
        {
            foreach(PlaceHolderKeyMover s in _placeHolderPlanks)
            {
                s.PlaceHolderKeyInteracted += S_OnPlaceHolderInteracted;
                s.OnPlaceHolderComplete += S_OnPlaceHolderComplete;

            }

            foreach(PlaceHolderKeyMover m in _visPlaceHolders)
            {
                m.PlaceHolderKeyInteracted += M_OnPlaceHolderComplete;
                m.OnPlaceHolderComplete += M_OnPlaceHolderComplete;
            }
        }

        private void S_OnPlaceHolderComplete(PlaceHolderKey key)
        {
            if (Array.Find(_placeHolderPlanks, x => x == key) != null)
            {
                _missingHolderPlanksToFill--;
                key.gameObject.SetActive(false);
                if (_missingHolderPlanksToFill <= 0)
                {
                    _hasSettedAllPlanks = true;
                    OnAllPlanksSetted();
                }
            }
        }

        private void OnAllPlanksSetted()
        {
            foreach(PlaceHolderKeyMover obj in _visPlaceHolders)
            {
                obj.gameObject.SetActive(true);
            }
        }

        private void OnAllVisSetted()
        {
            LockedDoor.CanBeInteracted = true;
            LockedDoor.CanBeRaycasted = true;
            LockedDoor.SetIsUnlocked(true);
            LockedDoor.Interact();
        }

        private void M_OnPlaceHolderComplete(PlaceHolderKey key)
        {
            if (Array.Find(_visPlaceHolders, x => x == key) != null)
            {
                _missingHolderVisToFill--;
                key.gameObject.SetActive(false);
                if (_missingHolderVisToFill <= 0)
                {
                    _hasSettedAllVis = true;
                    OnAllVisSetted();
                }
            }
        }

        private void M_OnPlaceHolderComplete(PlaceHolderKeyMover mover)
        {
            if (mover.TargetObject is IInventoryStackable stack && stack.CurrentStack > 1)
                mover.TargetObject = Locator<PlayerInventory>.Instance.TryRemoveStackedItem<Key>(mover.TargetObject, stack, mover.TargetObject as IInventoryItemCallback);
            else
                mover.TargetObject.ForceRemoveItemFromInventory();

            mover.CanObjectMoveToPosition = true;
        }

        protected void OnDestroy()
        {
            foreach (PlaceHolderKeyMover s in _placeHolderPlanks)
            {
                s.PlaceHolderKeyInteracted -= S_OnPlaceHolderInteracted;
            }
            foreach (PlaceHolderKeyMover m in _visPlaceHolders)
            {
                m.PlaceHolderKeyInteracted -= M_OnPlaceHolderComplete;
            }
        }

        private void S_OnPlaceHolderInteracted(PlaceHolderKeyMover mover)
        {
            if (mover.TargetObject is IInventoryStackable stack && stack.CurrentStack > 1)
                mover.TargetObject = Locator<PlayerInventory>.Instance.TryRemoveStackedItem<Key>(mover.TargetObject, stack, mover.TargetObject as IInventoryItemCallback);
            else
                mover.TargetObject.ForceRemoveItemFromInventory();

            mover.CanObjectMoveToPosition = true;
        }

        public override string GetInteractionName()
        {
            return "Repair it please";
        }

        public override string GetName()
        {
            return gameObject.name;
        }

        public override void Interact()
        {
            
        }
    }

}
