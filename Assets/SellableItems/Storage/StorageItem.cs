using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StorageItem : ItemBase
{
    public override string Name { get => "Open"; set => base.Name = value; }

    [SerializeField] private ItemSlotManager _slotManager;
    [SerializeField] private BoxCollider _itemCollider;
    [SerializeField] private Button _storageCloseCliqued;

    [Header("--- EVENTS ---")]
    [Header("RAISE")]
    [SerializeField] private CustomEvent _onStorageItemOpen;
    [SerializeField] private CustomEvent _onStorageItemClosed;

    [Header("Player Movements")]
    [SerializeField] private CustomEvent _lockPlayerMovement;
    [SerializeField] private CustomEvent _unlockPlayerMovement;

    [Header("Player Camera")]
    [SerializeField] private CustomEvent _lockPlayerCam;
    [SerializeField] private CustomEvent _unlockPlayerCam;

    [Header("LISTENERS")]
    [SerializeField] private CustomEvent_ItemBase _fromInventoryToStorage;

    [SerializeField] private CustomEvent_InventorySlot _sendItemToStorageFromSlot;

    private void Awake()
    {
        _fromInventoryToStorage.handle += _fromInventoryToStorage_handle;
        _sendItemToStorageFromSlot.handle += _sendItemToStorageFromSlot_handle;
        _storageCloseCliqued.onClick.AddListener(() =>
        {
            CloseStorage();
        });
    }

    private void _sendItemToStorageFromSlot_handle(InventorySlot slot, ItemBase item)
    {
      /*  var newItem = _slotManager.AddItem(item);
        item.transform.SetParent(newItem.transform);
        item.gameObject.SetActive(false);
*/
        var itm = PlayerManager.Instance._inventory.DropItem(slot, true);
        _slotManager.AddItem(itm);
        //PlayerManager.Instance._inventory.RemovePreviewItemFromInventoryToStore(item, slot);
    }

    private void OnDestroy()
    {
        _fromInventoryToStorage.handle -= _fromInventoryToStorage_handle;
        _sendItemToStorageFromSlot.handle -= _sendItemToStorageFromSlot_handle;
    }
    private void _fromInventoryToStorage_handle(ItemBase obj)
    {
        
    }

    public override bool Use()
    {
        base.Use();
        OpenStorage();
        return false;
    }

    public virtual void OpenStorage()
    {
        _slotManager.EnableUI();
        _itemCollider.enabled = false;
        _onStorageItemOpen?.Raise();
        _lockPlayerMovement?.Raise();
        _lockPlayerCam?.Raise();
    }

    public virtual void CloseStorage()
    {
        _itemCollider.enabled = true;
        _slotManager.DisableUI();
        _onStorageItemClosed?.Raise();
        _unlockPlayerMovement?.Raise();
        _unlockPlayerCam?.Raise();
    }
}
