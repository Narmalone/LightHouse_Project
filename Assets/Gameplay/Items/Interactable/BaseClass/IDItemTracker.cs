using LightHouse.Interactions;
using System;
using UnityEngine;
using LightHouse.Inventory;
using LightHouse.Inputs;

public abstract class IDItemTracker : InteractableItemBase, IItemCallback
{
    #region FIELDS & PROPERTIES
    [Header(" --- ID ITEM TRACKER --- ")]
    [Header("Main Fields")]
    [SerializeField] protected ItemIDEnum _itemNeeded;

    [Header("Texts")]
    [SerializeField] protected string _noKeyInInventoryText;
    [SerializeField] protected string _keyInInventoryButNotOnHandsText;
    [SerializeField] protected string _hasKeyOnHandsText;

    [Header("Read Only / Debug purposes")]
    [SerializeField] protected bool _hasKeyInInventory = false;
    [SerializeField] protected bool _hasKeyOnHands = false;
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

    #region IInteractable
    public override string GetInteractionName()
    {
        if (!_hasKeyInInventory)
            return _noKeyInInventoryText;
        else if (_hasKeyInInventory && !_hasKeyOnHands)
            return _keyInInventoryButNotOnHandsText;
        else
            return $"Hold {InputManager.GetBindingName(InputManager.InteractInInventory)} {_hasKeyOnHandsText}";
    }

    public override void Interact() => InvokeObjectInteracted();

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
