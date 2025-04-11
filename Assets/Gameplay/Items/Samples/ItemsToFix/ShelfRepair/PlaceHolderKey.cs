using LightHouse.Interactions;
using LightHouse.Inventory;
using LightHouse.KinematicCharacterController;
using LightHouse.Locators;
using System;
using UnityEngine;

namespace LightHouse.Items.Samples
{
    public abstract class PlaceHolderKey : MonoBehaviour, IInteractable
    {
        [SerializeField] protected string _placeHolderName = "Place Holder";
        [SerializeField] protected Collider _col;
        [SerializeField] protected KeyType _keyNeeded;
        [field: SerializeField] public bool CanBeInteracted { get; set; }
        [field: SerializeField] public bool CanBeRaycasted { get; set; } = true;
        [field: SerializeField] public bool IsItemRaycasted { get; set; }

        public event Action OnObjectInteracted;
        public event Action OnInteractionNameChanged;
        public event Action OnNameUpdated;

        [SerializeField] protected bool _hasKey = false;
        private bool _wasRecaysting = false;
        [SerializeField] protected bool _canCheck = true;
        [SerializeField] protected bool _hasKeyAndItemOnHand = false;
        [SerializeField] public Key TargetObject = null;
        public event Action<PlaceHolderKey> OnPlaceHolderComplete;

        public Collider GetCollider() => _col;

        public GameObject GetGameObject() => this.gameObject;

        public abstract string GetInteractionName();

        public virtual string GetName() => _placeHolderName;

        public abstract void Interact();

        protected virtual void Awake()
        {
            //PlayerInventory.OnHandsItemSelectedChanged += PlayerInventory_OnHandsItemSelectedChanged;
        }

        private void PlayerInventory_OnHandsItemSelectedChanged(IInventoryItem obj)
        {
            if (!IsItemRaycasted) return;
            HasKeyOnHands();
        }

        protected virtual void Update()
        {
            if (_wasRecaysting != IsItemRaycasted)
            {
                _wasRecaysting = IsItemRaycasted;

                if(IsItemRaycasted) HasKeyOnHands();
            }
        }

        protected virtual void OnDestroy()
        {
            //PlayerInventory.OnHandsItemSelectedChanged -= PlayerInventory_OnHandsItemSelectedChanged;
        }

        protected virtual bool HasKeyInInventory()
        {
            return false; 
        }

        protected virtual bool HasKeyOnHands()
        {
            if (!_canCheck) return false;
            /*if (Locator<PlayerInventory>.Instance.CurrentSelectedSlot == null || Locator<PlayerInventory>.Instance.CurrentSelectedSlot.InventoryItem == null)
            {
                _hasKeyAndItemOnHand = false;
                TargetObject = null;
                OnInteractionNameChanged?.Invoke();
                return false;
            }
            if(Locator<PlayerInventory>.Instance.CurrentSelectedItem is not Key)
            {
                _hasKeyAndItemOnHand = false;
                TargetObject = null;
                OnInteractionNameChanged?.Invoke();
                return false;
            }

            Key keyObj = Locator<PlayerInventory>.Instance.CurrentSelectedItem as Key;
            TargetObject = keyObj;
            bool lastStoredResult = _hasKeyAndItemOnHand;
            _hasKeyAndItemOnHand = HasKeyInInventory() && keyObj.KeyType == _keyNeeded && keyObj.IsItemOnHands;
            if (lastStoredResult != _hasKeyAndItemOnHand)
            {
                OnInteractionNameChanged?.Invoke();
            }*/
            return _hasKeyAndItemOnHand;
        }

        public void InvokePlaceHolderCompleted()
        {
            OnPlaceHolderComplete?.Invoke(this);
        }

        public void InvokeObjectInteracted()
        {
            OnObjectInteracted?.Invoke();
        }
    }
}
