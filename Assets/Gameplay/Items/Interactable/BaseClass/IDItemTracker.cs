using LightHouse.Interactions;
using System;
using UnityEngine;
using LightHouse.Inventory;
using LightHouse.Inputs;

public abstract class IDItemTracker : MonoBehaviour, IInteractable, IItemCallback
{
    #region FIELDS & PROPERTIES
    [Header(" --- ID ITEM TRACKER --- ")]
    [Header("Main Fields")]
    [SerializeField] protected Collider _col;
    [SerializeField] protected ItemIDEnum _itemNeeded;

    [Header("Texts")]
    [SerializeField] protected string _name;
    [SerializeField] protected string _noKeyInInventoryText;
    [SerializeField] protected string _keyInInventoryButNotOnHandsText;
    [SerializeField] protected string _hasKeyOnHandsText;
    [field: SerializeField] public bool CanBeInteracted { get; set; } = false;
    [field: SerializeField] public bool CanBeRaycasted { get; set; } = true;

    [Header("Read Only / Debug purposes")]
    [field: SerializeField] public bool IsItemRaycasted { get; set; }
    [SerializeField] protected bool _hasKeyInInventory = false;
    [SerializeField] protected bool _hasKeyOnHands = false;
    #endregion

    #region EVENTS
    public event Action OnObjectInteracted;
    public event Action OnInteractionNameChanged;
    public event Action OnNameUpdated;
    #endregion

    #region MONO'S CALLBACK
    protected virtual void Awake()
    {
        InventoryHandlerData.OnSelectedItemChanged += InventoryHandlerData_OnSelectedItemChanged;
        InventoryHandlerData.OnItemDropped += InventoryHandlerData_OnItemDropped;
    }
    protected virtual void OnDestroy()
    {
        InventoryHandlerData.OnSelectedItemChanged -= InventoryHandlerData_OnSelectedItemChanged;
        InventoryHandlerData.OnItemDropped -= InventoryHandlerData_OnItemDropped;
    }
    #endregion

    #region IItemName
    public virtual string GetName() => _name;
    public virtual Collider GetCollider() => _col;

    public virtual GameObject GetGameObject() => this.gameObject;
    #endregion

    #region IInteractable
    public virtual string GetInteractionName()
    {
        if (!_hasKeyInInventory)
            return _noKeyInInventoryText;
        else if (_hasKeyInInventory && !_hasKeyOnHands)
            return _keyInInventoryButNotOnHandsText;
        else
            return $"Hold {InputManager.GetBindingName(InputManager.InteractInInventory)} {_hasKeyOnHandsText}";
    }

    public virtual void Interact() => OnObjectInteracted?.Invoke();

    public void InvokeNameUpdated() => OnNameUpdated?.Invoke();
    public void InvokeInteractionDescriptionUpdated() => OnInteractionNameChanged?.Invoke();
    #endregion

    #region INVENTORY CALLBACKS
    protected virtual void InventoryHandlerData_OnItemDropped(IInventoryItem itm)
    {
        if (!IsItemRaycasted) return;
        CheckConditions();
    }

    protected virtual void InventoryHandlerData_OnSelectedItemChanged(IInventoryItem obj)
    {
        if(!IsItemRaycasted) return;
        CheckConditions();
    }
    #endregion

    #region IItemCallback
    public virtual void OnRaycastStart()
    {
        CheckConditions();
    }

    public virtual void OnRaycastEnd()
    {
        _hasKeyInInventory = false;
        _hasKeyOnHands = false;
    }
    #endregion

    #region Check & Other Abstract functions
    protected abstract void CheckConditions();
    #endregion
}
