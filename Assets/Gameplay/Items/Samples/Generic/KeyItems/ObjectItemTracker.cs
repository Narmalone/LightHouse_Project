using LightHouse.Interactions;
using LightHouse.Inventory;
using LightHouse.Locators;
using System;
using UnityEngine;

namespace LightHouse.Items.Samples
{
    public abstract class ObjectItemTracker : MonoBehaviour, IInteractable, IItemCallback
    {
        [SerializeField] protected string _itemName;
        [SerializeField] protected string _interactionName;
        public string InteractionName 
        { 
            get => _interactionName;
            set 
            {
                _interactionName = value;
                OnInteractionNameChanged();
            }
        }
        public string ItemName { get => _itemName; set => _itemName = value; }

        [SerializeField] protected Collider _col;
        [field: SerializeField] public bool CanBeInteracted { get; set; }
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

        public abstract void OnRaycastStart();

        public abstract void OnRaycastEnd();

        public virtual T HasTargetItemOnHands<T>() where T : class, IInventoryItem
        {
            return Locator<PlayerInventory>.Instance.GetCurrentSelectedItemOfType<T>();
        }

        public virtual bool IsCurrentSelectedItemTypeOf<T>() where T : class, IInventoryItem
        {
            return Locator<PlayerInventory>.Instance.GetCurrentSelectedItemTypeOf<T>();
        }

        public virtual bool HasItemInInventoryOfType<T>() where T : class, IInventoryItem
        {
            return Locator<PlayerInventory>.Instance.GetItemsOfType<T>().Count > 0;
        }
    }

}
