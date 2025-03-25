using UnityEngine;
using LightHouse.Interactions;
using System;
using LightHouse.Inventory;
using LightHouse.KinematicCharacterController;

namespace LightHouse.Items.Samples
{
    public class DestroyableItem : MonoBehaviour, IInteractable
    {
        #region FIELDS
        [Header("Main Settings")]
        [SerializeField] protected string _itemName = "Wooden Plank";
        [SerializeField] protected Collider _col;
        [SerializeField] protected KeyType _neededKey;
        [field: SerializeField] public bool CanBeInteracted { get; set; }
        [field: SerializeField] public bool CanBeRaycasted { get; set; }

        [Header("Read Only - DEBUG PURPOSES")]
        [field: SerializeField] public bool IsItemRaycasted { get; set; }
        [SerializeField] protected bool _hasKey = false;
        [SerializeField] protected bool _isKeyItemOnHandsSelected = false;
        [SerializeField] protected Key _keyObjSpotted;

        protected PlayerInventory _playerInventory;
        protected PlayerInteractions _playerInteractions;
        protected bool _wasRaycasted = false;

        #endregion

        //IItemName
        public event Action OnNameUpdated;

        //IInteractable
        public event Action OnObjectInteracted;
        public event Action OnInteractionNameChanged;

        #region IInteractable Methods
        public virtual Collider GetCollider() => _col;
        public virtual GameObject GetGameObject() => this.gameObject;
        public virtual string GetInteractionName()
        {
            if (_hasKey && _isKeyItemOnHandsSelected && _keyObjSpotted != null)
            {
                return _keyObjSpotted.UseInInventoryText();
            }
            else if (_hasKey && !_isKeyItemOnHandsSelected)
            {
                return "Take your hammer on your hands.";
            }
            else if (!_hasKey)
            {
                return "Find something to destroy it";
            }

            return string.Empty;
        }
        public virtual string GetName() => _itemName;
        public virtual void Interact() { }
        #endregion

        #region MONO CALLBACKS

        protected virtual void Awake()
        {
            PlayerInventory.OnInventoryInitialized += PlayerInventory_OnInventoryInitialized;
            PlayerInventory.OnHandsItemSelectedChanged += PlayerInventory_OnHandsItemSelectedChanged;
            PlayerInteractions.OnPlayerInteractionInitialized += PlayerInteractions_OnPlayerInteractionInitialized;
        }

        protected virtual void Update()
        {
            if (_playerInventory == null) return;

            //bool to check the frame we start to raycast the item
            if (_wasRaycasted != IsItemRaycasted)
            {
                _wasRaycasted = IsItemRaycasted;

                //If we are raycasting the item let's check 
                if (IsItemRaycasted)
                    HasKeyOnHands();
                //if we stop raycasting in case we are still seeing an other item we unsubscribe to avoid
                //multiple calls
                else
                {
                    if (_keyObjSpotted != null && _keyObjSpotted is IInventoryItemUsable usable)
                    {
                        usable.OnItemUsed -= Usable_OnItemUsed;

                        //if the current seeing object as the same type it means we are looking
                        //the same kind of objects so if not we wont to make it disapear
                        if (_playerInteractions.CurrentRaycastedIItemName is not IInventoryItemUsable)
                        {
                            usable.CanBeUsedFromInventory = false;
                            usable.InvokeOnCanBeUsedFromInventoryChanged();
                        }

                    }
                }

            }
        }
        protected virtual void OnDestroy()
        {
            PlayerInventory.OnInventoryInitialized -= PlayerInventory_OnInventoryInitialized;
            PlayerInventory.OnHandsItemSelectedChanged -= PlayerInventory_OnHandsItemSelectedChanged;
            PlayerInteractions.OnPlayerInteractionInitialized -= PlayerInteractions_OnPlayerInteractionInitialized;
        }
        #endregion

        #region PLAYER INVENTORY CALLBACKS
        protected virtual void PlayerInventory_OnInventoryInitialized(PlayerInventory obj)
        {
            _playerInventory = obj;
        }
        protected virtual void PlayerInventory_OnHandsItemSelectedChanged(IInventoryItem obj)
        {
            if (!IsItemRaycasted) return; //avoid to be called each time when we don't neet to know
            HasKeyOnHands();
        }
        #endregion

        #region PLAYER INTERACTIONS CALLBACKS
        private void PlayerInteractions_OnPlayerInteractionInitialized(PlayerInteractions obj)
        {
            _playerInteractions = obj;
        }
        #endregion

        #region IInventoryUsable Callbacks
        protected virtual void Usable_OnItemUsed()
        {
            if (_keyObjSpotted != null && _keyObjSpotted is IInventoryItemUsable usable)
            {
                usable.CanBeUsedFromInventory = false;
                usable.InvokeOnCanBeUsedFromInventoryChanged();
                usable.OnItemUsed -= Usable_OnItemUsed;
            }
            Destroy(this.gameObject);
        }
        #endregion

        #region CHECK CONDITIONS
        protected virtual bool HasKeyInInventory()
        {
            return _hasKey = _playerInventory.HasItem(_neededKey);
        }

        /// <summary>
        /// Check all possibilities to see what should we show to the player
        /// Called each time the player change it's current selected item, take or drop an object while he's raycasted
        /// </summary>
        protected bool HasKeyOnHands()
        {
            HasKeyInInventory();
            if (_playerInventory.CurrentSelectedSlot == null || _playerInventory.CurrentSelectedSlot.InventoryItem == null || _playerInventory.CurrentSelectedItem is not Key)
            {
                _isKeyItemOnHandsSelected = false;
                //if the player take nothing on hands we reset the usable item
                if (_keyObjSpotted != null && _keyObjSpotted is IInventoryItemUsable ss)
                {
                    ss.CanBeUsedFromInventory = false;
                    ss.InvokeOnCanBeUsedFromInventoryChanged();
                    ss.OnItemUsed -= Usable_OnItemUsed;
                }
                _keyObjSpotted = null;

                OnInteractionNameChanged?.Invoke();
                return false;
            }

            _keyObjSpotted = _playerInventory.CurrentSelectedItem as Key;
            bool wasKeyItemOnHandSelected = _isKeyItemOnHandsSelected;
            _isKeyItemOnHandsSelected = _hasKey && _keyObjSpotted.KeyType == _neededKey && _keyObjSpotted.IsItemOnHands;

            //if the player has the specific item on hand we subscribe to use 
            if (_keyObjSpotted is IInventoryItemUsable usable)
            {
                usable.CanBeUsedFromInventory = true;
                usable.InvokeOnCanBeUsedFromInventoryChanged();
                usable.OnItemUsed += Usable_OnItemUsed;
            }
            if (wasKeyItemOnHandSelected != _isKeyItemOnHandsSelected)
                OnInteractionNameChanged?.Invoke();

            return _isKeyItemOnHandsSelected;
        }

        #endregion
    }

}