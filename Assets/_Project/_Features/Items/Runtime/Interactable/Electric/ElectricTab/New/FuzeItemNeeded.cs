using LightHouse.Features.Items.Interactable;
using LightHouse.Features.Items.Inventory;
using System;
using UnityEngine;

public class FuzeItemNeeded : IDUseItemTracker
{
    [SerializeField] private Transform _targetParent;
    private bool _hasItem;
    public bool HasFuze => _hasItem;
    private FuzeItem _inventoryItemStoredInFuze;
    public event Action<float, float> OnFuzeSet;
    public event Action OnFuzeRemoved;
    protected override void Usable_OnItemUsed()
    {
        if(_hasItem) return;
        _inventoryItemStoredInFuze = _inventoryItemUsable as FuzeItem;
        base.Usable_OnItemUsed();
        _hasItem = true;
        _inventoryItemStoredInFuze.InvokeForceDropItemFromInventory(this.transform.position, 0f, false);
        //Dropper le fuze et le mettre à l'emplacement de l'objet interactif
        _inventoryItemStoredInFuze.GetGameObject().transform.SetParent(_targetParent);
        _inventoryItemStoredInFuze.GetGameObject().transform.position = this.transform.position;
        _inventoryItemStoredInFuze.GetGameObject().transform.rotation = this.transform.rotation;
        _inventoryItemStoredInFuze.GetCollider().enabled = true;
        _inventoryItemStoredInFuze.OnItemPickedUp += OnFuzePickedUp;
        _detectionCollider.enabled = false;

        OnFuzeSet?.Invoke(_inventoryItemStoredInFuze.Durability, _inventoryItemStoredInFuze.MaxDurabilityItem);
        this.gameObject.SetActive(false);
    }

    private void OnFuzePickedUp()
    {
        if (!_hasItem) return;
        _inventoryItemStoredInFuze.OnItemPickedUp -= OnFuzePickedUp;
        _inventoryItemStoredInFuze = null;
        _detectionCollider.enabled = true;
        OnFuzeRemoved?.Invoke();
        this.gameObject.SetActive(true);
        _hasItem = false;
    }
}
