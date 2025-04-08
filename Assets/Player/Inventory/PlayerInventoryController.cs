using LightHouse.Inputs;
using LightHouse.Inventory;
using LightHouse.Utilities;
using UnityEngine;


//TO DOO ----
//Relier les systèmes suivants::
//lier les booléens, l'objet est dans l'inventaire / ne l'est pas
//lier les autres interfaces IInventoryUsable, IinventoryCallback, ne pas oublier les delegates etc..
//Lier le ItemDatabase / ajouter un script editor pour gérer les ID des objets ???
//Revoir pour séparer les logiques de Drop / pickup
//recréer les fonctions pour dropper de manière safe un objet
//Commenter le code, éviter effets de bords, surplus de variable, ajouter les events associés
//Revoir dans le pool manager le système de sorting par l'implémentation dans l'IInventoryItem
//finir d'assigner les infos dans l'ui avec les items
public class PlayerInventoryController : MonoBehaviour
{
    #region SERILIAZED FIELDS
    [Header("Inventory Settings")]
    [SerializeField] private byte _inventoryCapacity = 4;
    [SerializeField] private int _currentSlotIndex = -1;
    [SerializeField] private byte _currentSlotTakenInventory = 0;

    [Header("Inventory References")]
    [SerializeField] private Transform _inventoryParent = null;
    [SerializeField] private Transform _inventoryTarget = null;

    [Header("Raycast")]
    [SerializeField] private bool _enableInventoryRaycast = true;
    [SerializeField] private Camera _playerCamera;
    [SerializeField] private float _raycastDistance = 3.0f;
    [SerializeField] private LayerMask _targetMask = 1 << -1;
    [SerializeField] private QueryTriggerInteraction _queryTriggerInteraction = QueryTriggerInteraction.Ignore;

    [Header("UI")]
    [SerializeField] private InventoryUIController _inventoryCanvas;

    public static bool RaycastResult = false;
    #endregion

    private ItemSlot[] _slots;

    private ItemSlot CurrentSelectedSlot => _slots[_currentSlotIndex];
    private GameObject _lastObjectSeen;
    private IInventoryItem _lastInventoryItemSeen;

    private void Start()
    {
        _slots = _inventoryCanvas.GenerateItemSlot(_inventoryCapacity);
        InputManager.Scroll.performed += Scroll_performed;
    }

    private void OnDisable()
    {
        InputManager.Scroll.performed -= Scroll_performed;
    }

    private void Update()
    {
        if (_enableInventoryRaycast)
        {
            Ray cameraRay = _playerCamera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0f));
            RaycastResult =  RaycastUtility.TryRaycast(cameraRay, _raycastDistance, _targetMask, _queryTriggerInteraction, out RaycastHit hit);
            if (RaycastResult)
            {
                HandleNewObject(hit.collider.gameObject);

                if (HandlePickup()) PickupItem(_currentSlotIndex, _lastInventoryItemSeen);
            }
            else
                ResetSeenObject();

            if(!IsCurrentSelectedIndexNotValid(_currentSlotIndex) && CurrentSelectedSlot.SlotDatas.ItemSpecificIds.Count > 0)
                HandleDrop(CurrentSelectedSlot.SlotDatas.SlotID, CurrentSelectedSlot.SlotDatas.ItemGlobalID, CurrentSelectedSlot.SlotDatas.ItemSpecificIds[0]);
            Debug.DrawRay(_playerCamera.transform.position, cameraRay.direction * _raycastDistance, Color.cyan);
        }
    }

    private void Scroll_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        int scrollDirection = Mathf.RoundToInt(obj.ReadValue<Vector2>().y);
        int inversedScroll = -scrollDirection; //invert scroll direction (scroll up to left, scroll down to right)

        if (inversedScroll == 0) return;

        if (!IsCurrentSelectedIndexNotValid(_currentSlotIndex))
        {
            _slots[_currentSlotIndex].SlotDatas.IsSelected = false;
            if (_slots[_currentSlotIndex].SlotDatas.HasItem)
                _slots[_currentSlotIndex].HideSelectedInfos();
        }

        _currentSlotIndex += inversedScroll;
        // Cyclic scrolling, -1 is for nothing selected
        if (_currentSlotIndex < -1)
            _currentSlotIndex = _inventoryCapacity - 1;
        else if (_currentSlotIndex >= _inventoryCapacity)
            _currentSlotIndex = -1;

        if (!IsCurrentSelectedIndexNotValid(_currentSlotIndex))
        {
            _slots[_currentSlotIndex].SlotDatas.IsSelected = true;
            if (_slots[_currentSlotIndex].SlotDatas.HasItem)
                _slots[_currentSlotIndex].Show();
        }
    }

    private void ResetSeenObject()
    {
        if (_lastObjectSeen != null)
            _lastObjectSeen = null;

        if (_lastInventoryItemSeen != null)
            _lastInventoryItemSeen = null;
    }

    private void HandleNewObject(GameObject lastObjectSeen)
    {
        if(_lastObjectSeen != lastObjectSeen)
        {
            _lastObjectSeen = lastObjectSeen;
            _lastObjectSeen.TryGetComponent(out _lastInventoryItemSeen);

            if (_lastInventoryItemSeen != null)
            {
                Debug.Log("nouvel inventory item détecté");
            }
        }
    }

    private bool HandlePickup()
    {
        return InputManager.PickUp.WasPerformedThisFrame();
    }

    private void PickupItem(int slotIndex, IInventoryItem item)
    {
        //TO DOO Check if the slot id is empty or null
        if (item == null) return;

        PoolManager.Add(item);

        ItemSlot targetSlot = null;

        if (IsSlotNullOrNotEmpty(slotIndex))
        {
            targetSlot = TryGetEmptySlotInInventory();
        }
        else
        {
            targetSlot = _slots[slotIndex];
        }

        if (item is IInventoryStackable stackableObject)
        {

            if (IsItemAlreadyExistInSlotAndPossibleToStack(item.ID, out ItemSlot slot))
            {
                if(slot.SlotDatas.TotalItemsInSlots < slot.SlotDatas.MaxStack)
                    targetSlot = slot;
            }
            
            if(targetSlot.SlotDatas.TotalItemsInSlots == 0) targetSlot.SlotDatas.MaxStack = stackableObject.MaxStack;
        }

        targetSlot.AddItemDatasToSlot(item);
    }

    private bool IsSlotNullOrNotEmpty(int slotIndex)
    {
        return slotIndex < 0 || slotIndex > _inventoryCapacity || _slots[slotIndex].SlotDatas.HasItem;
    }

    private bool IsCurrentSelectedIndexNotValid(int slotIndex)
    {
        return slotIndex < 0 || slotIndex > _inventoryCapacity;
    }

    private ItemSlot TryGetEmptySlotInInventory()
    {
        foreach(ItemSlot slot in _slots)
        {
            if (!slot.SlotDatas.HasItem)
            {
                return slot;
            }
        }
        return null;
    }

    private bool IsItemAlreadyExistInSlotAndPossibleToStack(ushort globalItemID, out ItemSlot findedSlot)
    {
        findedSlot = null;
        foreach (ItemSlot slot in _slots)
        {
            if (slot.SlotDatas.ItemGlobalID != globalItemID 
                || slot.SlotDatas.TotalItemsInSlots == 0 
                || !slot.SlotDatas.CanStack()) continue;
            findedSlot = slot;
            return true;
        }
        return false;
    }

    private void HandleDrop(byte slotID, ushort itemGlobalID, ushort itemSpecificID)
    {
        if (InputManager.Drop.WasPerformedThisFrame())
        {
            DropItemAtSlot(slotID, itemGlobalID, itemSpecificID);
        }
        /*if (InputManager.Drop.IsPressed())
        {
            _isChargingDrop = true;
            _dropPower = Mathf.Clamp(_dropPower + Time.deltaTime * _maxDropPower, 0, _maxDropPower);
        }
        else if (_isChargingDrop && InputManager.Drop.WasReleasedThisFrame())
        {
            DropItemAtSlot(_currentSelectedSlot, _inventoryTarget.position, _dropPower);
            _dropPower = 0f;
            _isChargingDrop = false;
        }
        else if (InputManager.Drop.WasPerformedThisFrame())
        {
            DropItemAtSlot(_currentSelectedSlot, _inventoryTarget.position, 1f);
        }*/
    }

    private void DropItemAtSlot(byte slotID, ushort itemGlobalID, ushort itemSpecificID)
    {
        if (!_slots[slotID].SlotDatas.HasItem) return;
        IInventoryItem item = PoolManager.Get(itemGlobalID, new SortingResult(SortingType.None, itemSpecificID));
        _slots[slotID].RemoveItemFromSlot(_slots[slotID].SlotDatas.ItemSpecificIds[0]);
        item.GetGameObject().transform.position = _inventoryTarget.position;
    }

    private Vector3 GetSafeDropPos()
    {
        return transform.position;
    }
}
