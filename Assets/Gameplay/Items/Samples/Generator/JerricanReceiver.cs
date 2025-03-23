using LightHouse.Interactions;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LightHouse.Items.Samples
{
    public class JerricanReceiver : MonoBehaviour, IInteractable
{
        [SerializeField] private Collider _col;
        [SerializeField] private string _itemDescription;
        public string ItemDescription
        {
            get => _itemDescription;
            set => _itemDescription = value;
        }
        [field: SerializeField] public bool CanBeInteracted { get; set; }
        [field: SerializeField] public bool IsItemRaycasted { get; set; }
        public bool CanBeRaycasted { get; set; } = true;

        public event Action OnObjectInteracted;
        public event Action OnInteractionNameChanged;
        public event Action OnNameUpdated;

        public Collider GetCollider() => _col;

        public GameObject GetGameObject() => this.gameObject;

        public string GetInteractionName()
        {
            return _itemDescription;
        }

        public string GetName()
        {
            return string.Empty;
        }

        public void Interact()
        {
            OnObjectInteracted?.Invoke();
        }

        public void InvokeInteractionNameChanged()
        {
            OnInteractionNameChanged?.Invoke();
        }
    }
}

