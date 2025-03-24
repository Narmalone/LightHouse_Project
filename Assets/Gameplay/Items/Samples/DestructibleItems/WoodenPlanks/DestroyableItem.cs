using UnityEngine;
using LightHouse.Interactions;
using System;
using LightHouse.Inputs;
using LightHouse.Inventory;

namespace LightHouse.Items.Samples
{
    public class DestroyableItem : MonoBehaviour, IInteractable
    {
        #region FIELDS
        [SerializeField] private string _itemName = "Wooden Plank";
        [SerializeField] private Collider _col;

        [SerializeField] private KeyType _neededKey;
        [field: SerializeField] public bool CanBeInteracted { get; set; }
        [field: SerializeField] public bool CanBeRaycasted { get; set; }
        [field: SerializeField] public bool IsItemRaycasted { get; set; }

        public event Action OnObjectInteracted;
        public event Action OnInteractionNameChanged;
        public event Action OnNameUpdated;

        [SerializeField] private bool _hasKey = false;
        [SerializeField] private bool _isKeyItemOnHandsSelected = false;
        private PlayerInventory _playerInventory;
        private bool _wasRaycasted = false;

        private Key _keyObjSpotted;

        #endregion
        public virtual Collider GetCollider() => _col;

        public virtual GameObject GetGameObject() => this.gameObject;

        public virtual string GetInteractionName()
        {
            if (_hasKey && _isKeyItemOnHandsSelected)
            {
                return $"Press {InputManager.GetBindingName(InputManager.Interact)} to destroy";
            }
            else if (_hasKey && !_isKeyItemOnHandsSelected)
            {
                return "Take your hammer on your hands.";
            }
            else if(!_hasKey)
            {
                return "Find something to destroy it";
            }
            
            return string.Empty;
        }

        public virtual string GetName()
        {
            return _itemName;
        }
        public virtual void Interact()
        {
            if (!CanBeInteracted) return;
            Destroy(this.gameObject);
        }

        protected virtual void Awake()
        {
            PlayerInventory.OnInventoryInitialized += PlayerInventory_OnInventoryInitialized;
            PlayerInventory.OnHandsItemSelectedChanged += PlayerInventory_OnHandsItemSelectedChanged;
        }

        protected virtual void Update()
        {
            if (_playerInventory == null) return;

            //bool to check the frame we start to raycast the item
            if(_wasRaycasted != IsItemRaycasted)
            {
                _wasRaycasted = IsItemRaycasted;

                if(IsItemRaycasted)
                    CanBeInteracted = HasKeyOnHands();
            }
        }

        protected virtual void OnDestroy()
        {
            PlayerInventory.OnInventoryInitialized -= PlayerInventory_OnInventoryInitialized;
            PlayerInventory.OnHandsItemSelectedChanged -= PlayerInventory_OnHandsItemSelectedChanged;
        }

        protected virtual void PlayerInventory_OnInventoryInitialized(PlayerInventory obj)
        {
            _playerInventory = obj;
        }

        protected virtual bool HasKeyInInventory()
        {
            return _playerInventory.HasItem(_neededKey);
        }

        /// <summary>
        /// Check all possibilities to see what should we show to the player
        /// </summary>
        protected bool HasKeyOnHands()
        {
            _hasKey = HasKeyInInventory();
            if (_playerInventory.CurrentSelectedSlot == null || _playerInventory.CurrentSelectedSlot.InventoryItem == null)
            {
                _isKeyItemOnHandsSelected = false;
                _keyObjSpotted = null;
                OnInteractionNameChanged?.Invoke();
                return false;
            }
            if (_playerInventory.CurrentSelectedItem is not Key)
            {
                _isKeyItemOnHandsSelected = false;
                _keyObjSpotted = null;
                OnInteractionNameChanged?.Invoke();
                return false;
            }

            Key keyObj = _playerInventory.CurrentSelectedItem as Key;
            _keyObjSpotted = keyObj;
            bool lastStoredResult = _isKeyItemOnHandsSelected;
            _isKeyItemOnHandsSelected = _hasKey && keyObj.KeyType == _neededKey && keyObj.IsItemOnHands;
            if (lastStoredResult != _isKeyItemOnHandsSelected)
            {
                OnInteractionNameChanged?.Invoke();
            }
            CanBeInteracted = _isKeyItemOnHandsSelected;
            return _isKeyItemOnHandsSelected;
        }

        protected virtual void PlayerInventory_OnHandsItemSelectedChanged(IInventoryItem obj)
        {
            if (!IsItemRaycasted) return; //avoid to be called each time when we don't neet to know
            CanBeInteracted = HasKeyOnHands();
        }
    }

}
