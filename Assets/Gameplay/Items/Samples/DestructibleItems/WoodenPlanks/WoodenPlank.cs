using UnityEngine;
using LightHouse.Interactions;
using System;
using LightHouse.Inputs;
using LightHouse.Inventory;

namespace LightHouse.Items.Samples
{
    public class WoodenPlank : MonoBehaviour, IInteractable
    {
        [SerializeField] private string _itemName = "Wooden Plank";
        [SerializeField] private Collider _col;

        [SerializeField] private KeyType _hammerKey;
        [field: SerializeField] public bool CanBeInteracted { get; set; }
        [field: SerializeField] public bool CanBeRaycasted { get; set; }
        [field: SerializeField] public bool IsItemRaycasted { get; set; }

        public event Action OnObjectInteracted;
        public event Action OnInteractionNameChanged;
        public event Action OnNameUpdated;

        [SerializeField] private bool _hasKey = false;
        [SerializeField] private bool _isKeyItemOnHandsSelected = false;
        private PlayerInventory _playerInventory;

        public Collider GetCollider() => _col;

        public GameObject GetGameObject() => this.gameObject;

        private bool _wasRaycasted = false;
        public string GetInteractionName()
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

        public string GetName()
        {
            return _itemName;
        }
        public void Interact()
        {
            if (!CanBeInteracted) return;
            Destroy(this.gameObject);
        }

        private void Awake()
        {
            PlayerInventory.OnInventoryInitialized += PlayerInventory_OnInventoryInitialized;
            PlayerInventory.OnHandsItemSelectedChanged += PlayerInventory_OnHandsItemSelectedChanged;
        }

        private void Update()
        {
            if (_playerInventory == null) return;
            if(_wasRaycasted != IsItemRaycasted)
            {
                _wasRaycasted = IsItemRaycasted;

                if(IsItemRaycasted)
                    CheckConditionsForReal();
            }
        }

        private void OnDestroy()
        {
            PlayerInventory.OnInventoryInitialized -= PlayerInventory_OnInventoryInitialized;
            PlayerInventory.OnHandsItemSelectedChanged -= PlayerInventory_OnHandsItemSelectedChanged;
        }

        private void PlayerInventory_OnInventoryInitialized(PlayerInventory obj)
        {
            _playerInventory = obj;
        }

        private void CheckConditionsForReal()
        {
            _hasKey = _playerInventory.HasItem(_hammerKey);

            var currentSelectedSlot = _playerInventory.CurrentSelectedSlot;
            if(currentSelectedSlot != null)
            {
                if (currentSelectedSlot.InventoryItem != null)
                {
                    if (currentSelectedSlot.InventoryItem is Key)
                    {
                        Key key = (Key)_playerInventory.CurrentSelectedSlot.InventoryItem;
                        _isKeyItemOnHandsSelected = key.ItemKeyType == this._hammerKey ? true : false;
                    }
                    else
                    {
                        _isKeyItemOnHandsSelected = false;
                        CanBeInteracted = false;
                        OnInteractionNameChanged?.Invoke();
                        return;
                    }
                }
                else
                {
                    _isKeyItemOnHandsSelected = false;
                    CanBeInteracted = false;
                    OnInteractionNameChanged?.Invoke();
                    return;
                }
                
            }
            else
            {
                _isKeyItemOnHandsSelected = false;
                CanBeInteracted = false;
            }

            CanBeInteracted = _hasKey && _isKeyItemOnHandsSelected;
            OnInteractionNameChanged?.Invoke();
        }

        private void PlayerInventory_OnHandsItemSelectedChanged(IInventoryItem obj)
        {
            if (!IsItemRaycasted) return; //avoid to be called each time when we don't neet to know
            CheckConditionsForReal();
        }
    }

}
