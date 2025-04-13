using System;
using UnityEngine;

namespace LightHouse.Items.Samples
{
    public class ShelfToFix : MonoBehaviour
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
                s.OnPlaceHolderKeyCompleted += OnPlankPlaced;

            }

            foreach(PlaceHolderKeyMover m in _visPlaceHolders)
            {
                m.OnPlaceHolderKeyCompleted += OnNailPlaced;
            }
        }

        private void OnPlankPlaced(PlaceHolderKeyMover key)
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

        private void OnNailPlaced(PlaceHolderKeyMover key)
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
            LockedDoor.Open();
        }

        protected void OnDestroy()
        {
            foreach (PlaceHolderKeyMover s in _placeHolderPlanks)
            {
                s.OnPlaceHolderKeyCompleted -= OnPlankPlaced;
            }
            foreach (PlaceHolderKeyMover m in _visPlaceHolders)
            {
                m.OnPlaceHolderKeyCompleted -= OnNailPlaced;
            }
        }

        private void S_OnPlaceHolderInteracted(PlaceHolderKeyMover mover)
        {
            mover.CanObjectMoveToPosition = true;
        }
    }

}
