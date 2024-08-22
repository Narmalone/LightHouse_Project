using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class StorageItem : ItemBase
{
    public override string Name { get => "Open"; set => base.Name = value; }

    [SerializeField] private ItemSlotManager _slotManager;
    [SerializeField] private BoxCollider _itemCollider;
    [SerializeField] private Button _storageCloseCliqued;
    [SerializeField] private Transform _storageItemParent;
    [SerializeField] private StorePointController[] _storagePoints;
    [SerializeField] private List<StorePointController> _currentUsedPoints = new List<StorePointController>();
    [SerializeField] private List<StorePointController> _availableStoragePoints = new List<StorePointController>();
    //plus tard, faire une liste d'objet et de "points" de spawn pour qu'on voit les objets
    //si la liste est pleine on met juste les items dans l'inventaire 
    //limite maximale ?

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
    [SerializeField] private CustomEvent_InventorySlot _sendItemToStorageFromSlot;
    [SerializeField] private CustomEvent_ItemBase _sendItemInventoryFromStorage;

    private void Awake()
    {
        //recup del quand inverse
        _sendItemToStorageFromSlot.handle += _sendItemToStorageFromSlot_handle;
        _sendItemInventoryFromStorage.handle += _sendItemInventoryFromStorage_handle;
        _availableStoragePoints = _storagePoints.ToList();
        _storageCloseCliqued.onClick.AddListener(() =>
        {
            CloseStorage();
        });
    }

    //A FIXER
    private void _sendItemInventoryFromStorage_handle(ItemBase obj)
    {
        //Lors que le joueur reprend l'objet, trouver le moyen de récupérer le point qui perds l'objet ? 
        //Meilleure idée, custom event sur le ItemSlotController et on raise plutot un event qui renvoie (le item slot controller)
        //et chaque itemslotcontroller à l'info sur quel point est son objet !
        throw new NotImplementedException();
    }

    private void _sendItemToStorageFromSlot_handle(InventorySlot slot)
    {
        ItemBase itm = PlayerManager.Instance._inventory.DropItem(slot, true);
        ItemSlotController slotController = _slotManager.AddItem(itm);
        
        if(_availableStoragePoints.Count > 0)
        {
            if (itm.TryGetComponent(out Rigidbody body))
            {
                body.isKinematic = true;
            }
            itm._collider.enabled = false;

            int getRdm = UnityEngine.Random.Range(0, _availableStoragePoints.Count);
            StorePointController rdmPoint = _availableStoragePoints[getRdm];
            rdmPoint.Item = itm;
            itm.transform.SetParent(rdmPoint.transform);
            itm.transform.SetPositionAndRotation(rdmPoint.transform.position, rdmPoint.transform.rotation);
            slotController.SetStorePointController(rdmPoint);
            _availableStoragePoints.Remove(rdmPoint);
            _currentUsedPoints.Add(rdmPoint);
        }
        else
        {
            itm.transform.SetParent(_storageItemParent);
            itm.gameObject.SetActive(false);
        }
        
    }

    private void OnDestroy()
    {
        _sendItemToStorageFromSlot.handle -= _sendItemToStorageFromSlot_handle;
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
