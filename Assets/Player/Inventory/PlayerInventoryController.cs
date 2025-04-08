using LightHouse.Inputs;
using LightHouse.Inventory;
using LightHouse.Utilities;
using UnityEngine;

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

                if (HandlePickup()) PickupItem(_currentSlotIndex);
            }
            else
                ResetSeenObject();

            if(!IsCurrentSelectedIndexNotValid(_currentSlotIndex))
                HandleDrop(CurrentSelectedSlot.SlotDatas.SlotID, CurrentSelectedSlot.SlotDatas.ItemID, CurrentSelectedSlot.SlotDatas.ItemSpecificID);
            Debug.DrawRay(_playerCamera.transform.position, cameraRay.direction * _raycastDistance, Color.cyan);
        }
    }

    private void Scroll_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        int scrollDirection = Mathf.RoundToInt(obj.ReadValue<Vector2>().y);
        int inversedScroll = -scrollDirection; //invert scroll direction (scroll up to left, scroll down to right)

        if (inversedScroll == 0) return;

        _currentSlotIndex += inversedScroll;
        // Cyclic scrolling, -1 is for nothing selected
        if (_currentSlotIndex < -1)
            _currentSlotIndex = _inventoryCapacity - 1;
        else if (_currentSlotIndex >= _inventoryCapacity)
            _currentSlotIndex = -1;
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

    private void PickupItem(int slotID)
    {
        //TO DOO Check if the slot id is empty or null
        if (_lastInventoryItemSeen == null) return;

        ItemSlot targetSlot = null;
        if (IsSlotNullOrNotEmpty(slotID))
        {
            targetSlot = TryGetEmptySlotInInventory();
        }
        else
        {
            targetSlot = _slots[slotID];
        }
        PoolManager.Add(_lastInventoryItemSeen);
        targetSlot.AddItemDatasToSlot(_lastInventoryItemSeen.ID, _lastInventoryItemSeen.SpecificID, _lastInventoryItemSeen.ItemSprite);
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

    private void DropItemAtSlot(byte slodID, ushort itemGlobalID, ushort itemSpecificID)
    {
        var item = PoolManager.GetSpecificItem(itemGlobalID, itemSpecificID);
        Debug.Log(item.GetGameObject().name);
    }

    private Vector3 GetSafeDropPos()
    {
        return transform.position;
    }
}
