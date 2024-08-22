using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StorageItem : ItemBase
{
    public override string Name { get => "Open"; set => base.Name = value; }

    [SerializeField] private ItemSlotManager _slotManager;
    [SerializeField] private CustomEvent _onStorageItemOpen;
    [SerializeField] private CustomEvent _lockPlayerMovement;
    [SerializeField] private CustomEvent _unlockPlayerMovement;
    [SerializeField] private CustomEvent _lockPlayerCam;
    [SerializeField] private CustomEvent _unlockPlayerCam;

    [SerializeField] private CustomEvent_ItemBase _fromInventoryToStorage;

    private void Awake()
    {
        _fromInventoryToStorage.handle += _fromInventoryToStorage_handle;
    }

    private void OnDestroy()
    {
        _fromInventoryToStorage.handle -= _fromInventoryToStorage_handle;
    }
    private void _fromInventoryToStorage_handle(ItemBase obj)
    {
        var newItem = _slotManager.AddItem(obj);
        obj.transform.SetParent(newItem.transform);
        PlayerManager.Instance._inventory.RemoveItemFromInventory(obj, false);
    }

    public override bool Use()
    {
        base.Use();
        OpenStorage();
        return false;
    }

    public virtual void OpenStorage()
    {
        _slotManager.MainGroup.alpha = 1f;
        _onStorageItemOpen?.Raise();
        _lockPlayerMovement?.Raise();
        _lockPlayerCam?.Raise();
    }

    public virtual void CloseStorage()
    {
        _unlockPlayerMovement?.Raise();
        _unlockPlayerCam?.Raise();
    }
}
