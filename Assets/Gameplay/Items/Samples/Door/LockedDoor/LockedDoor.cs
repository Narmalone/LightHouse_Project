using LightHouse.Inputs;
using LightHouse.Interactions.Samples;
using LightHouse.Inventory;
using UnityEngine;

namespace LightHouse.Items.Samples
{
    public class LockedDoor : Door
    {
        #region SERIALIZED FIELDS
        [Header("Inventory")]
        [SerializeField] protected KeyType _key;

        [Header("Debug")]
        [SerializeField] protected bool _hasKey = false;
        [SerializeField] protected bool _isUnLocked = false;

        public bool HasKey => _hasKey;
        public bool IsUnLocked => _isUnLocked;

        #endregion

        #region MONO'S CALLBACK
        protected override void Awake()
        {
            base.Awake();
            
        }
        protected override void OnDestroy()
        {
            base.OnDestroy();
            PlayerInventory.OnItemAdded -= PlayerInventory_OnItemAdded;
            PlayerInventory.OnItemDropped -= PlayerInventory_OnItemDropped;
        }

        protected override void PlayerInventory_OnInventoryInitialized(PlayerInventory obj)
        {
            base.PlayerInventory_OnInventoryInitialized(obj);
            PlayerInventory.OnItemAdded += PlayerInventory_OnItemAdded;
            PlayerInventory.OnItemDropped += PlayerInventory_OnItemDropped;
        }

        #endregion

        #region Register Callbacks
        private void PlayerInventory_OnItemAdded(IInventoryItem obj)
        {
            if (_isUnLocked || !IsItemRaycasted) return;
            if (_key != null)
            {
                _hasKey = _inventory.HasItem(_key);
            }
            else
            {
                _hasKey = true;
            }
        }

        private void PlayerInventory_OnItemDropped(IInventoryItem obj)
        {
            if (_isUnLocked || !IsItemRaycasted) return;
            if (_key != null)
            {
                _hasKey = _inventory.HasItem(_key);

                if(IsItemRaycasted)
                    base.InvokeInteractionNameChanged();
            }
        }

        #endregion

        #region IInteractable
        public override string GetInteractionName()
        {
            if (_key != null)
            {
                _hasKey = _inventory.HasItem(_key);
                if (_hasKey && !_isUnLocked)
                {
                    return $"Unlock door with {InputManager.GetBindingName(InputManager.Interact)}";
                }
                else if (!_isUnLocked)
                {
                    return "The Door is locked you have to find the key.";
                }
            }

            return base.GetInteractionName();
        }

        public override string GetName()
        {
            return string.Empty;
        }

        public override void Interact()
        {
            if (_key != null)
            {
                if (_hasKey && !_isUnLocked)
                {
                    _isUnLocked = true;

                    if(IsItemRaycasted)
                        base.InvokeInteractionNameChanged(); //force update the GetInteractionName
                    return;
                }
                if (!_isUnLocked) return;
            }
            base.Interact();
        }

        #endregion

        public void SetIsUnlocked(bool value) => _isUnLocked = value;
    }
}
