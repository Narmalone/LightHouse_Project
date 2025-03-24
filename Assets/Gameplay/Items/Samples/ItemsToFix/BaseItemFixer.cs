using LightHouse.Interactions;
using LightHouse.Inventory;
using NUnit;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace LightHouse.Items.Samples
{
    public abstract class BaseItemFixer : MonoBehaviour, IInteractable
    {
        [SerializeField] protected List<KeyType> _necesarryItemsInInventory = new List<KeyType>();
        [SerializeField] protected Collider _col;
        [field: SerializeField] public bool CanBeInteracted { get; set; }
        [field: SerializeField] public bool CanBeRaycasted { get; set; }
        [field: SerializeField] public bool IsItemRaycasted { get; set; }

        public event Action OnObjectInteracted;
        public event Action OnInteractionNameChanged;
        public event Action OnNameUpdated;

        protected PlayerInventory _playerInventoy;

        public Collider GetCollider() => _col;

        public GameObject GetGameObject() => this.gameObject;

        public abstract string GetInteractionName();

        public abstract string GetName();

        public abstract void Interact();

        protected virtual void Awake()
        {
            PlayerInventory.OnInventoryInitialized += PlayerInventory_OnInventoryInitialized;
        }

        protected virtual void PlayerInventory_OnInventoryInitialized(PlayerInventory obj)
        {
            _playerInventoy = obj;
        }

        protected virtual void OnDestroy()
        {
            PlayerInventory.OnInventoryInitialized -= PlayerInventory_OnInventoryInitialized;
        }

        public virtual bool HasKeysInInventory()
        {
            return _playerInventoy.HasItems(_necesarryItemsInInventory.ToArray());
        }
    }

}
