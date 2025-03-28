using LightHouse.Interactions;
using LightHouse.Inventory;
using LightHouse.Locators;
using System;
using UnityEngine;

namespace LightHouse.Items.Samples
{
    public abstract class KeyItemTracker : MonoBehaviour, IInteractable, IItemCallback
    {
        //Maybe upgrade it to an enum
        public bool HasItemOnHands = false;
        public bool HasItemInInventoryButNotOnHands = false;

        [SerializeField] protected KeyType _keyNeededOnHands;
        [SerializeField] protected Collider _col;
        [field :SerializeField] public bool CanBeInteracted { get; set; }
        [field: SerializeField] public bool CanBeRaycasted { get; set; }
        [field: SerializeField] public bool IsItemRaycasted { get; set; }

        public event Action OnObjectInteracted;
        public event Action OnInteractionNameChanged;
        public event Action OnNameUpdated;

        public virtual Collider GetCollider() => _col;
        public virtual GameObject GetGameObject() => this.gameObject;
        public abstract string GetInteractionName();
        public abstract string GetName();
        public abstract void Interact();
        protected virtual void Awake() => PlayerInventory.OnHandsItemSelectedChanged += PlayerInventory_OnHandsItemSelectedChanged;
        protected virtual void OnDestroy() => PlayerInventory.OnHandsItemSelectedChanged -= PlayerInventory_OnHandsItemSelectedChanged;

        protected virtual void PlayerInventory_OnHandsItemSelectedChanged(IInventoryItem obj)
        {
            if (!IsItemRaycasted) return;
            HasItemInInventoryButNotOnHands = HasKeyInInventoryButNotOnHands();
            HasItemOnHands = HasTargetItemOnHands();
        }

        public virtual bool HasKeyInInventoryButNotOnHands()
        {
            return Locator<PlayerInventory>.Instance.HasItem(_keyNeededOnHands);
        }

        public virtual bool HasTargetItemOnHands()
        {
            PlayerInventory currInventoryInstance = Locator<PlayerInventory>.Instance;
            bool isOnHandItemIsKey = currInventoryInstance.CurrentSelectedSlot != null && currInventoryInstance.CurrentSelectedItem != null && currInventoryInstance.CurrentSelectedItem is Key;
            if (!isOnHandItemIsKey) return false;

            Key keyItm = currInventoryInstance.CurrentSelectedItem as Key;
            return keyItm.KeyType == _keyNeededOnHands;
        }

        public virtual Key TryGetKeyItem()
        {
            if(Locator<PlayerInventory>.Instance.CurrentSelectedSlot == null) return null;
            return Locator<PlayerInventory>.Instance.CurrentSelectedItem as Key;
        }

        public virtual void OnRaycastStart()
        {
            HasItemInInventoryButNotOnHands = HasKeyInInventoryButNotOnHands();
            HasItemOnHands = HasTargetItemOnHands();
        }

        public virtual void OnRaycastEnd() { }

        public void InvokeOnInteractionNameChanged()
        {
            OnInteractionNameChanged?.Invoke();
        }

        public void InvokeOnNameChanged()
        {
            OnNameUpdated?.Invoke();
        }
    }

}
