using System;
using LightHouse.Interactions;
using UnityEngine;

namespace LightHouse.Items.Interactable
{
    public abstract class InteractableItemBase : MonoBehaviour, IInteractable
    {
        [Header("Interactable Item Base")]
        [SerializeField] protected string _name;
        [SerializeField] protected Collider _detectionCollider;

        [field: SerializeField] public bool CanBeRaycasted { get; set; } = true;
        [field: SerializeField] public bool CanBeInteracted { get; set; } = true;
        [field: SerializeField] public bool IsItemRaycasted { get; set; }

        public event Action OnObjectInteracted;
        public event Action OnInteractionNameChanged;
        public event Action<string> OnNameUpdated;
        protected virtual void Awake()
        {
        }

        protected virtual void OnDestroy()
        {
        }

        public virtual string GetName() => this._name;
        public virtual Collider GetCollider() => this._detectionCollider;
        public virtual GameObject GetGameObject() => this.gameObject;
        public abstract string GetInteractionName();
        public abstract void Interact();

        public void InvokeNameUpdated() => OnNameUpdated?.Invoke(_name);
        public void InvokeInteractionDescriptionUpdated() => OnInteractionNameChanged?.Invoke();
        public void InvokeObjectInteracted() => OnObjectInteracted?.Invoke();
    }
}

