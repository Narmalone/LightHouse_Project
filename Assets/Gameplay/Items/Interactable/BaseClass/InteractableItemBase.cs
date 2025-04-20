using System;
using LightHouse.Inputs;
using LightHouse.Interactions;
using UnityEngine;

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
    public event Action OnNameUpdated;

    public virtual string GetName() => this._name;
    public virtual Collider GetCollider() => this._detectionCollider;
    public virtual GameObject GetGameObject() => this.gameObject;
    public virtual string GetInteractionName() => $"Press {InputManager.GetBindingName(InputManager.Interact)} to interact.";
    public abstract void Interact();

    public void InvokeNameUpdated() => OnNameUpdated?.Invoke();
    public void InvokeInteractionDescriptionUpdated() => OnInteractionNameChanged?.Invoke();
    public void InvokeObjectInteracted() => OnObjectInteracted?.Invoke();

}
