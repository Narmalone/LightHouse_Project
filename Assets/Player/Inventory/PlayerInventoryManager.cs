using LightHouse.Inputs;
using LightHouse.Interactions;
using UnityEngine;
using LightHouse.Items.Detection;
using LightHouse.Inventory;

namespace LightHouse.KinematicCharacterController
{
    /// <summary>
    /// Main controller of the inventory system
    /// </summary>
    public class PlayerInventoryManager : MonoBehaviour
    {
        #region SERILIAZED FIELDS
        [Header("Inventory Settings")]
        [SerializeField] private byte _inventoryCapacity = 4;
        [SerializeField] private float _grabAndDropItemRange = 1.5f;

        [Header("Inventory References")]
        [SerializeField] private ItemDatabase _itemDatabase;
        [SerializeField] private Transform _inventoryTarget = null;

        [Header("Raycast")]
        [SerializeField] private Camera _playerCamera; //automatically attribuated from player ?
        [SerializeField] private float _raycastDetectionRange = 3.0f; //automatically attribuated from player ?
        [SerializeField] private LayerMask _inventoryItemsMask = 1 << 6;
        [SerializeField] private LayerMask _blockingLayers = 1 << 6;
        [SerializeField] private QueryTriggerInteraction _inventoryRaycastQti = QueryTriggerInteraction.Ignore;

        [Header("Drop Settings")]
        [SerializeField] private float _maxDropPower = 10f;
        [SerializeField] private AnimationCurve _dropPowerCurve = AnimationCurve.Linear(0, 0, 1, 1);
        [SerializeField] private float _securityOverlapSphereRadius = 0.3f;
        [SerializeField] private LayerMask _securityObstacleMasks = 1 << 0;

        [Header("Inventory Controllers")]
        private RaycastDetector<IInventoryItem> _inventoryRaycastDetector;
        [SerializeField] private InventoryPickupHandler _pickupHandler;
        [SerializeField] private InventoryScrollHandler _scrollHandler;
        [SerializeField] private InventoryDropHandler _dropHandler;
        [SerializeField] private InventoryUseItemHandler _useFromInventoryHandler;

        [Header("UI")]
        [SerializeField] private InventoryUIController _inventoryUiController;
        [SerializeField] private CanvasInteraction _interactionUiController;

        public RaycastDetector<IInventoryItem> RaycastDetector => _inventoryRaycastDetector;
        #endregion

        #region PRIVATE / HIDED FIELDS

        //generated Slots
        private ItemSlot[] _slots;

        private short CurrentSlotIndex => SlotManager.CurrentSlotIndex;

        //Raycast datas
        private GameObject _lastObjectSeen;
        private IInventoryItem _lastInventoryItemSeen;

        #endregion

        #region MONO CALLBACKS
        private void Awake() => Initialize();
        private void Start()
        {
            InventoryHandlerData.Initialize(_inventoryUiController, _inventoryTarget);
        }

        private void Update()
        {
            _inventoryRaycastDetector.UpdateRay();
            if (InputManager.PickUp.WasPerformedThisFrame() && _lastInventoryItemSeen != null)
                AddItemToInventory(CurrentSlotIndex, _lastInventoryItemSeen);

            if (InventoryHandlerData.IsGrabbingObjectOrIndexInvalid())
                return;
            HandleDropInput();
            HandleInteractInInventoryInput();
        }

        private void LateUpdate()
        {
            _inventoryTarget.position = _inventoryRaycastDetector.RayOrigin + (_inventoryRaycastDetector.RayDirection.normalized * _grabAndDropItemRange);
            _inventoryTarget.rotation = Quaternion.LookRotation(_inventoryRaycastDetector.RayDirection.normalized);
        }

        private void OnDrawGizmos()
        {
            _dropHandler?.OnDrawGizmos();
        }

        private void OnDestroy()
        {
            UnregisterInputs();
            _inventoryRaycastDetector.OnDetected -= HandleItemDetected;
            _inventoryRaycastDetector.OnItemLost -= ResetSeenObject;
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
            InitializeControllers();
        }

        private void InitializeControllers()
        {
            _pickupHandler = new InventoryPickupHandler();
            _scrollHandler = new InventoryScrollHandler(_itemDatabase);
            _useFromInventoryHandler = new InventoryUseItemHandler(_inventoryUiController);
            _dropHandler = new InventoryDropHandler(_inventoryUiController, _inventoryTarget, _maxDropPower, _dropPowerCurve, _securityObstacleMasks, _securityOverlapSphereRadius);

            _inventoryRaycastDetector = new RaycastDetector<IInventoryItem>(
                _playerCamera,
                _raycastDetectionRange,
                _inventoryItemsMask,
                _blockingLayers,
                _inventoryRaycastQti
            );
            _inventoryRaycastDetector.OnDetected += HandleItemDetected;
            _inventoryRaycastDetector.OnItemLost += ResetSeenObject;

        }
        #endregion

        #region REGISTER / UNREGISTER INPUTS CALLBACKS
        public void RegisterInputs()
        {
            InputManager.OnInitialized += InputManager_OnInputManagerInitialized;
            InputManager.OnInputManagerWillClear += InputManager_OnInputManagerWillClear;
            InventoryHandlerData.OnItemDropped += InventoryHandlerData_OnItemDropped;
        }

        public void UnregisterInputs()
        {
            InputManager.OnInputManagerWillClear -= InputManager_OnInputManagerWillClear;
            InputManager.OnInitialized -= InputManager_OnInputManagerInitialized;
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

        public void AddItemToInventory(short slotIndex, IInventoryItem item)
        {
            if(_pickupHandler.PickupItem(slotIndex, item))
            {
                item.ForceDropItemFromInventory += IInventoryItem_ForceDropItemFromInventory;
                InventoryHandlerData.NotifyAddedToInventory(item);
            }
            else
            {
                Debug.Log("item non récupéré inventaire plein");
            }
        }

        public void RemoveItemFromInventory(int slotIndex, ushort globalItemID, ushort specificItemID,
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
        }

        #endregion

        #region IInventoryItem && IInventoryUsable Callbacks

        private void IInventoryItem_ForceDropItemFromInventory(ushort globalItemID, ushort specificItemID, Vector3 position, float force, bool enablePhysicsOnDrop)
        {
            if(SlotManager.FindItemInSlot(globalItemID, specificItemID, out byte slotID))
                RemoveItemFromInventory(slotID, globalItemID, specificItemID, position, force, enablePhysicsOnDrop);
        }

        #endregion

        #region RAYCAST CALLBACKS
        private void HandleItemDetected(IInventoryItem item)
        {
            if (!item.CanBeRaycasted) return;
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
            _useFromInventoryHandler.Started();
        }
        private void InteractInInventory_canceled(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            _useFromInventoryHandler.Canceled();
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
