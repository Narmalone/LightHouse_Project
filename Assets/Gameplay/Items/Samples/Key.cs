using UnityEngine;
using LightHouse.Interactions;
using LightHouse.Inputs;
using System;
using LightHouse.Inventory;

namespace LightHouse.Items.Samples
{
    public class Key : ItemBase, IInteractable, IDescribable, IInventoryItem
    {
        [SerializeField] private string _keyName;
        [SerializeField] private Collider _keyCollider;
        [SerializeField] private Rigidbody _keyRigidbody;

        public event Action OnDescriptionUpdated;
        public event Action OnNameUpdated;

        public string GetName()
        {
            return _keyName;
        }

        public string GetDescription()
        {
            return $"";
        }

        public void Interact()
        {
            Debug.Log("Le joueur interagit avec la clé: " + gameObject.name);
            //le joueur récupère la clée
        }

        public string GetPickupName()
        {
            return $"Press {InputManager.GetBindingName(InputManager.PickUp)} to pick";
        }

        public ItemBase GetItem() => this;

        public Collider GetCollider() => _keyCollider;

        public Rigidbody GetRigidBody() => _keyRigidbody;

        public void OnItemAddedToInventory()
        {
            
        }

        public void OnItemRemovedFromInventory()
        {
            
        }
    }

}
