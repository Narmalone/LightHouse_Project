using LightHouse.Inputs;
using LightHouse.Interactions;
using UnityEngine;

namespace LightHouse.Inventory
{
    /// <summary>
    /// Main controller of the inventory system
    /// </summary>
    public class PlayerInventoryController : MonoBehaviour
    {
        #region SERILIAZED FIELDS
        [Header("Inventory Settings")]
        [SerializeField] private byte _inventoryCapacity = 4;
        [SerializeField] private float _grabAndDropItemRange = 1.5f;

        [Header("Inventory References")]
        [SerializeField] private ItemDatabase _itemDatabase;
        [SerializeField] private Transform _inventoryTarget = null;

        [Header("Inventory Controllers")]
        [SerializeField] private InventoryRaycastDetector _raycastDetector;
        [SerializeField] private InventoryPickupHandler _pickupHandler;
        [SerializeField] private InventoryScrollHandler _scrollHandler;
        [SerializeField] private InventoryDropHandler _dropHandler;
        [SerializeField] private InventoryUseItemHandler _useFromInventoryHandler;

        [Header("UI")]
        [SerializeField] private InventoryUIController _inventoryUiController;
        [SerializeField] private CanvasInteraction _interactionUiController;

        public InventoryRaycastDetector RaycastDetector => _raycastDetector;
        #endregion

        #region PRIVATE / HIDED FIELDS

        //generated Slots
        private ItemSlot[] _slots;

        //Qui access
        private ItemSlot CurrentSelectedSlot => SlotManager.CurrentSelectedSlot;
        private short CurrentSlotIndex => SlotManager.CurrentSlotIndex;

        //Raycast datas
        private GameObject _lastObjectSeen;
        private IInventoryItem _lastInventoryItemSeen;

        #endregion

        #region MONO CALLBACKS
        private void Awake() => Initialize();

        private void Update()
        {
            if (InputManager.PickUp.WasPerformedThisFrame() && _lastInventoryItemSeen != null)
                AddItemToInventory(CurrentSlotIndex, _lastInventoryItemSeen);

            if (InventoryHandlerData.IsGrabbingObjectOrIndexInvalid())
                return;
            HandleDropInput();
            HandleInteractInInventoryInput();
        }

        private void LateUpdate()
        {
            _inventoryTarget.position = _raycastDetector.RayOrigin + (_raycastDetector.RayDirection.normalized * _grabAndDropItemRange);
        }

        private void OnDestroy()
        {
            UnregisterInputs();
            SlotManager.Clear();
            InventoryHandlerData.Reset();
            PoolManager.Clear();
        }

        #endregion

        #region INITIALIZE METHODS
        private void Initialize()
        {
            RegisterInputs();
            _slots = _inventoryUiController.GenerateItemSlot(_inventoryCapacity, _itemDatabase);
            SlotManager.Initialize(_slots);
            InventoryHandlerData.Initialize(_inventoryUiController, _inventoryTarget);
            InitializeControllers();
        }

        private void InitializeControllers()
        {
            _pickupHandler = new InventoryPickupHandler();
            _scrollHandler = new InventoryScrollHandler(_itemDatabase);
            _useFromInventoryHandler = new InventoryUseItemHandler(_inventoryUiController);
            _dropHandler.Initialize(_slots, _inventoryUiController, _inventoryTarget);
            
        }
        #endregion

        #region REGISTER / UNREGISTER INPUTS CALLBACKS
        public void RegisterInputs()
        {
            InputManager.OnInputManagerInitialized += InputManager_OnInputManagerInitialized;
            InputManager.OnInputManagerWillClear += InputManager_OnInputManagerWillClear;
            _raycastDetector.OnItemDetected += HandleItemDetected;
            _raycastDetector.OnItemLost += ResetSeenObject;
            InventoryHandlerData.OnItemDropped += InventoryHandlerData_OnItemDropped;
        }

        public void UnregisterInputs()
        {
            InputManager.OnInputManagerWillClear -= InputManager_OnInputManagerWillClear;
            InputManager.OnInputManagerInitialized -= InputManager_OnInputManagerInitialized;
            _raycastDetector.OnItemDetected -= HandleItemDetected;
            _raycastDetector.OnItemLost -= ResetSeenObject;
            InventoryHandlerData.OnItemDropped -= InventoryHandlerData_OnItemDropped;
        }

        private void InputManager_OnInputManagerInitialized()
        {
            InputManager.Select.performed += Select_performed;
            InputManager.Scroll.performed += Scroll_performed;
            InputManager.InteractInInventory.started += InteractInInventory_started;
            InputManager.InteractInInventory.canceled += InteractInInventory_canceled; ;
        }

        private void InputManager_OnInputManagerWillClear() 
        {
            InputManager.Select.performed -= Select_performed;
            InputManager.Scroll.performed -= Scroll_performed;
            InputManager.InteractInInventory.started -= InteractInInventory_started;
            InputManager.InteractInInventory.canceled -= InteractInInventory_canceled;
        }

        #endregion

        #region ADD / REMOVE ITEM 

        private void AddItemToInventory(short slotIndex, IInventoryItem item)
        {
            _pickupHandler.PickupItem(slotIndex, item);
            item.ForceDropItemFromInventory += IInventoryItem_ForceDropItemFromInventory;
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
            IInventoryItemUsable usable = droppedItem as IInventoryItemUsable;
            if (usable != null)
                usable.CanBeUsedFromInventoryChanged -= Usable_CanBeUsedFromInventoryChanged;
        }

        #endregion

        #region IInventoryItem && IInventoryUsable Callbacks
        private void Usable_CanBeUsedFromInventoryChanged(ushort globalID, ushort specificID)
        {
            if (SlotManager.TryFindItemInCurrentSelectedSlot(globalID, specificID, out IInventoryItem item, out short slotID))
            {
                if (item is IInventoryItemUsable usable)
                    _slots[slotID].IsInventoryItemUsable(usable);
            }
        }

        private void IInventoryItem_ForceDropItemFromInventory(ushort globalItemID, ushort specificItemID, Vector3 position, float force, bool enablePhysicsOnDrop)
        {
            if(SlotManager.FindItemInSlot(globalItemID, specificItemID, out byte slotID))
                RemoveItemFromInventory(slotID, globalItemID, specificItemID, position, force, enablePhysicsOnDrop);
        }

        #endregion

        #region RAYCAST CALLBACKS
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

        #region DROP HANDLING && Callback
        /// <summary>
        /// when an item is dropped the <see cref="InventoryHandlerData"/> call an event
        /// it allow us to unsubscribe the item
        /// </summary>
        private void InventoryHandlerData_OnItemDropped(IInventoryItem obj)
        {
            obj.ForceDropItemFromInventory -= IInventoryItem_ForceDropItemFromInventory;
        }

        private void HandleDropInput() => _dropHandler.HandleDropInput();
        #endregion

        #region InteractInInventory Handling & Input callback
        private void InteractInInventory_started(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            if (InventoryHandlerData.IsGrabbingObjectOrIndexInvalid())
            {
                return;
            }
            if (SlotManager.CurrentSelectedSlot.SlotDatas.GetFirstItemInSlot(out IInventoryItem item))
            {
                if (item is IInventoryItemUsable usable)
                    _useFromInventoryHandler.SetTarget(usable);
            }
        }
        private void InteractInInventory_canceled(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            if (InventoryHandlerData.IsGrabbingObjectOrIndexInvalid())
            {
                return;
            }
            if (!SlotManager.CurrentSelectedSlot.SlotDatas.HasItem)
            {
                _useFromInventoryHandler.SetTarget(null);
                return;
            }

            if (SlotManager.CurrentSelectedSlot.SlotDatas.GetFirstItemInSlot(out IInventoryItem item))
            {
                if (item is IInventoryItemUsable)
                    _useFromInventoryHandler.SetTarget(null);
            }
        }
        private void HandleInteractInInventoryInput() => _useFromInventoryHandler.HandeInteractInInventoryInput();
        #endregion
        #region SCROLL HANDLING
        private void Scroll_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            int direction = -Mathf.RoundToInt(obj.ReadValue<Vector2>().y);
            if (direction != 0)
                _scrollHandler.Scroll(direction);
        }
        #endregion

        #region SELECT HANDLING
        private void Select_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            if (short.TryParse(obj.control.name, out short slotIndex))
            {
                short targetSlotIndex = slotIndex;
                //if slot already selected we just don't select anything
                if (slotIndex - 1 == CurrentSlotIndex)
                    targetSlotIndex = -1;
                else
                    targetSlotIndex = (short)(slotIndex - 1);
                SlotManager.ChangeSelectedSlot(_itemDatabase, targetSlotIndex);
            }
        }
        #endregion
    }

}
