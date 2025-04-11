using LightHouse.Inputs;
using LightHouse.Interactions;
using LightHouse.Inventory;
using UnityEngine;

//TO DOO ----
//lier les autres interfaces IInventoryUsable
//refaire le système de key avec keytypes
//Revoir dans le pool manager le système de sorting par l'implémentation dans l'IInventoryItem
//finir d'assigner les infos dans l'ui avec les items
public class PlayerInventoryController : MonoBehaviour
{
    #region SERILIAZED FIELDS
    [Header("Inventory Settings")]
    [SerializeField] private byte _inventoryCapacity = 4;

    [Header("Inventory References")]
    [SerializeField] private Transform _inventoryTarget = null;

    [Header("Inventory Controllers")]
    [SerializeField] private InventoryRaycastDetector _raycastDetector;
    [SerializeField] private InventoryPickupHandler _pickupHandler;
    [SerializeField] private InventoryScrollHandler _scrollHandler;
    [SerializeField] private InventoryDropHandler _dropHandler;

    [SerializeField] private ItemDatabase _itemDatabase;

    [Header("UI")]
    [SerializeField] private InventoryUIController _inventoryUiController;
    [SerializeField] private CanvasInteraction _interactionUiController;

    [Header("Drop Settings")]
    [SerializeField] private float _maxDropPower = 10f;
    [SerializeField] private AnimationCurve _dropPowerCurve = AnimationCurve.Linear(0, 0, 1, 1);
    private float _dropChargeTimer = 0f;
    #endregion

    private float _dropPower = 0f;
    private bool _isChargingDrop = false;

    private ItemSlot[] _slots;

    private ItemSlot CurrentSelectedSlot => _slots[_scrollHandler.CurrentIndex];
    private GameObject _lastObjectSeen;
    private IInventoryItem _lastInventoryItemSeen;

    private void Awake()
    {
        _raycastDetector.OnItemDetected += HandleItemDetected;
        _raycastDetector.OnItemLost += ResetSeenObject;
        _slots = _inventoryUiController.GenerateItemSlot(_inventoryCapacity, _itemDatabase);
    }

    private void Start()
    {
        _pickupHandler = new InventoryPickupHandler(_slots, _inventoryCapacity);
        _scrollHandler = new InventoryScrollHandler(_slots, _inventoryCapacity, _itemDatabase);
        _dropHandler.Initialize(_slots, _inventoryTarget);
        InputManager.Scroll.performed += Scroll_performed;
        InventoryHandlerData.Initialize(_slots, _inventoryTarget);
    }

    private void OnDisable()
    {
        InputManager.Scroll.performed -= Scroll_performed;
    }

    private void OnDestroy()
    {
        _raycastDetector.OnItemDetected -= HandleItemDetected;
        _raycastDetector.OnItemLost -= ResetSeenObject;
        InventoryHandlerData.Reset();
        PoolManager.Clear();
    }

    private void Update()
    {
        if (!_scrollHandler.IsIndexInvalid(_scrollHandler.CurrentIndex))
        {
            if (CurrentSelectedSlot.SlotDatas.ItemSpecificIds.Count > 0)
                HandleDropInputs();
        }

        if (InputManager.PickUp.WasPerformedThisFrame() && _lastInventoryItemSeen != null)
        {
            AddItemToInventory(_scrollHandler.CurrentIndex, _lastInventoryItemSeen);
        }

        if (InputManager.InteractInInventory.WasPerformedThisFrame())
        {
            if (_scrollHandler.IsIndexInvalid(_scrollHandler.CurrentIndex)) return;
            var s = CurrentSelectedSlot.SlotDatas.GetFirstItemInSlot();
            if (s != null && s is IInventoryItemUsable usable)
            {
                if (!usable.CanBeUsedFromInventory) return;
                usable.UseFromInventory();
            }
        }
            
    }

    private void AddItemToInventory(int slotIndex, IInventoryItem item)
    {
        _pickupHandler.PickupItem(slotIndex, item);
        item.ForceDropItemFromInventory += IinventoryItem_ForceDropItemInInventory;
        IInventoryItemUsable usable = item as IInventoryItemUsable;
        if (usable != null)
            usable.CanBeUsedFromInventoryChanged += Usable_CanBeUsedFromInventoryChanged;
    }

    private void RemoveItemFromInventory(int slotIndex, ushort globalItemID, ushort specificItemID, 
        Vector3 position, float force, bool enablePhysicsOnDrop)
    {
        _dropHandler.DropItem
            (
                slotID: slotIndex,
                itemGlobalID: globalItemID,
                specificID: specificItemID,
                pos: position,
                force: force,
                enablePhysicsOnDrop: enablePhysicsOnDrop,
                out IInventoryItem droppedItem
            );
        droppedItem.ForceDropItemFromInventory -= IinventoryItem_ForceDropItemInInventory;

        IInventoryItemUsable usable = droppedItem as IInventoryItemUsable;
        if (usable != null)
            usable.CanBeUsedFromInventoryChanged -= Usable_CanBeUsedFromInventoryChanged;
    }

    private void Usable_CanBeUsedFromInventoryChanged(ushort globalID, ushort specificID)
    {
        if(InventoryHandlerData.TryFindSpecificItem(globalID, specificID, out IInventoryItem item, out short slotID))
        {
            if(item is IInventoryItemUsable usable)
                _slots[slotID].IsInventoryItemUsable(usable);
        }
    }

    private void IinventoryItem_ForceDropItemInInventory(ushort globalItemID, ushort specificItemID, Vector3 position, float force, bool enablePhysicsOnDrop)
    {
        RemoveItemFromInventory(_slots[_scrollHandler.CurrentIndex].SlotDatas.SlotID, globalItemID, specificItemID, position, force, enablePhysicsOnDrop);
    }

    private void LateUpdate()
    {
        _inventoryTarget.position = _raycastDetector.RayOrigin + _raycastDetector.RayDirection.normalized;
    }

    #region RAYCAST DELEGATES
    private void HandleItemDetected(IInventoryItem item)
    {
        _lastInventoryItemSeen = item;
        _interactionUiController.ShowItemPickup();
        _interactionUiController.SetItemPickupText(item.GetPickupName());
    }

    private void ResetSeenObject()
    {
        if (_lastObjectSeen != null)
            _lastObjectSeen = null;

        if (_lastInventoryItemSeen != null)
        {
            _interactionUiController.HideItemPickup();
            _lastInventoryItemSeen = null;
        }
    }
    #endregion

    #region DROP HANDLING

    private void HandleDropInputs()
    {
        if (InputManager.Drop.IsPressed())
        {
            _isChargingDrop = true;
            _dropChargeTimer += Time.deltaTime;

            float curveValue = _dropPowerCurve.Evaluate(Mathf.Clamp01(_dropChargeTimer));
            _dropPower = curveValue * _maxDropPower;
        }
        else if (_isChargingDrop && InputManager.Drop.WasReleasedThisFrame())
        {
            RemoveItemFromInventory
            (
                slotIndex: CurrentSelectedSlot.SlotDatas.SlotID,
                globalItemID: CurrentSelectedSlot.SlotDatas.ItemGlobalID,
                specificItemID: _slots[_scrollHandler.CurrentIndex].SlotDatas.ItemSpecificIds[0],
                position: _inventoryTarget.position,
                force: _dropPower,
                enablePhysicsOnDrop: true
            );
            _dropPower = 0f;
            _dropChargeTimer = 0f;
            _isChargingDrop = false;
        }
        else if (InputManager.Drop.WasPerformedThisFrame())
        {
            RemoveItemFromInventory
            (
                slotIndex: CurrentSelectedSlot.SlotDatas.SlotID,
                globalItemID: CurrentSelectedSlot.SlotDatas.ItemGlobalID,
                specificItemID: _slots[_scrollHandler.CurrentIndex].SlotDatas.ItemSpecificIds[0],
                position: _inventoryTarget.position,
                force: 0.0f,
                enablePhysicsOnDrop: true
            );
        }
    }
    #endregion

    #region SCROLL HANDLING
    private void Scroll_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        int direction = -Mathf.RoundToInt(obj.ReadValue<Vector2>().y);
        if (direction != 0)
            _scrollHandler.Scroll(direction);
    }
    #endregion
}
