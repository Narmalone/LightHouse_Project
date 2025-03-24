using LightHouse.Interactions;
using LightHouse.Inventory;
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

        public PlayerInventory PlayerInventory;
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
            PlayerInventory.OnInventoryInitialized += PlayerInventory_OnInventoryInitialized;
            PlayerInventory.OnHandsItemSelectedChanged += PlayerInventory_OnHandsItemSelectedChanged;
        }

        protected virtual void PlayerInventory_OnInventoryInitialized(PlayerInventory obj)
        {
            PlayerInventory = obj;
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
            PlayerInventory.OnInventoryInitialized -= PlayerInventory_OnInventoryInitialized;
            PlayerInventory.OnHandsItemSelectedChanged -= PlayerInventory_OnHandsItemSelectedChanged;
        }

        protected virtual bool HasKeyInInventory()
        {
            return _hasKey = PlayerInventory.HasItem(_keyNeeded); 
        }

        protected virtual bool HasKeyOnHands()
        {
            if (!_canCheck) return false;
            if (PlayerInventory.CurrentSelectedSlot == null || PlayerInventory.CurrentSelectedSlot.InventoryItem == null)
            {
                _hasKeyAndItemOnHand = false;
                TargetObject = null;
                OnInteractionNameChanged?.Invoke();
                return false;
            }
            if(PlayerInventory.CurrentSelectedItem is not Key)
            {
                _hasKeyAndItemOnHand = false;
                TargetObject = null;
                OnInteractionNameChanged?.Invoke();
                return false;
            }

            Key keyObj = PlayerInventory.CurrentSelectedItem as Key;
            TargetObject = keyObj;
            bool lastStoredResult = _hasKeyAndItemOnHand;
            _hasKeyAndItemOnHand = HasKeyInInventory() && keyObj.KeyType == _keyNeeded && keyObj.IsItemOnHands;
            if (lastStoredResult != _hasKeyAndItemOnHand)
            {
                OnInteractionNameChanged?.Invoke();
            }
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
