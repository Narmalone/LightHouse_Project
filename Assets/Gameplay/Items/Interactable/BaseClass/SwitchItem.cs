using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LightHouse.Interactions;
using System;

namespace LightHouse.Items.Samples
{
    public abstract class SwitchItem : MonoBehaviour, IInteractable
    {
        [SerializeField] protected bool _switchValue = false;
        public bool CanBeInteracted { get; set; }
        public bool IsItemRaycasted { get; set; }
        [field: SerializeField] public bool CanBeRaycasted { get; set; } = true;

        public event Action OnObjectInteracted;
        public event Action OnInteractionNameChanged;
        public event Action OnNameUpdated;
        public abstract Collider GetCollider();
        public virtual GameObject GetGameObject() => this.gameObject;
        public abstract string GetInteractionName();
        public abstract string GetName();
        public abstract void Interact();
    }
}

