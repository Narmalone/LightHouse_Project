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
        [SerializeField] private ItemsDetectionSystem _raycastSystem;
        public bool IsEnabled = true;

        [Header("Inventory Settings")]
        [SerializeField] private byte _inventoryCapacity = 4;
        [SerializeField] private float _grabAndDropItemRange = 1.5f;

        [Header("Inventory References")]
        [SerializeField] private ItemDatabase _itemDatabase;
        [SerializeField] private Transform _inventoryTarget = null;

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

        [Header("AUDIO")]
        [SerializeField] private AudioCue _basePickupSound;
        [SerializeField] private AudioCue _baseDropSound;

        public RaycastDetector<IInventoryItem> RaycastDetector => _inventoryRaycastDetector;
        #endregion

        #region PRIVATE / HIDED FIELDS

        //generated Slots
        private ItemSlot[] _slots;

        private short CurrentSlotIndex => SlotManager.CurrentSlotIndex;

        //Raycast datas
        private GameObject _lastObjectSeen;
        private IInventoryItem _lastInventoryItemSeen;

        public bool FreezeScrollingInventory { get; set; } = false;
        private bool _isInitialized = false;
        #endregion

        #region MONO CALLBACKS
        private void Awake() => Initialize();

        private void Start()
        {
            _inventoryRaycastDetector = _raycastSystem.InventoryDetector;
            _inventoryRaycastDetector.OnDetected += HandleItemDetected;
            _inventoryRaycastDetector.OnItemLost += ResetSeenObject;
        }

        private void Update()
        {
            if (!_isInitialized) return;
            if (InputManager.PickUp.WasPerformedThisFrame() && _lastInventoryItemSeen != null)
                AddItemToInventory(CurrentSlotIndex, _lastInventoryItemSeen, soundToPlay: _basePickupSound);

            if (InventoryHandlerData.IsGrabbingObjectOrIndexInvalid())
                return;
            HandleDropInput();
            HandleInteractInInventoryInput();

        }

        private void LateUpdate()
        {
            if (!_isInitialized) return;

            Vector3 origin = ItemsDetectionSystem.RayOrigin;
            Vector3 direction = ItemsDetectionSystem.RayDirection.normalized;
            float maxDistance = _grabAndDropItemRange;

            Ray ray = new Ray(origin, direction);
            if (Physics.Raycast(ray, out RaycastHit hit, maxDistance, _securityObstacleMasks, QueryTriggerInteraction.Ignore))
            {
                // Obstacle détecté → place l’objet juste avant le mur
                _inventoryTarget.position = hit.point - direction * 0.2f; // recule légèrement pour éviter de toucher le mur
            }
            else
            {
                // Pas d’obstacle → place normalement à portée max
                _inventoryTarget.position = origin + direction * maxDistance;
            }

            _inventoryTarget.rotation = Quaternion.LookRotation(direction);
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
            InventoryHandlerData.SetInventoryTarget(_inventoryTarget);
            RegisterInputs();
            _slots = _inventoryUiController.GenerateItemSlot(_inventoryCapacity, _itemDatabase);
            SlotManager.Initialize(_slots);
            InitializeControllers();
            _isInitialized = true;
        }

        private void InitializeControllers()
        {
            _pickupHandler = new InventoryPickupHandler();
            _scrollHandler = new InventoryScrollHandler(_itemDatabase);
            _useFromInventoryHandler = new InventoryUseItemHandler(_inventoryUiController);
            _dropHandler = new InventoryDropHandler(_inventoryUiController, _inventoryTarget, _maxDropPower, _dropPowerCurve, _securityObstacleMasks, _securityOverlapSphereRadius);
        }
        #endregion

        #region UI
        public void Enable()
        {
            _inventoryUiController.gameObject.SetActive(true);
            IsEnabled = true;
        }
        public void Disable()
        {
            _inventoryUiController.gameObject.SetActive(false);
            IsEnabled = false;
        }

        public void Show()
        {
            _inventoryUiController.Show();
        }

        public void Hide()
        {
            _inventoryUiController.Hide();
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
            if(InputManager.Select != null)
                InputManager.Select.performed -= Select_performed;

            if(InputManager.Scroll != null)
                InputManager.Scroll.performed -= Scroll_performed;

            if(InputManager.InteractInInventory != null)
            {
                InputManager.InteractInInventory.started -= InteractInInventory_started;
                InputManager.InteractInInventory.canceled -= InteractInInventory_canceled;
            }
                
        }

        #endregion

        #region ADD / REMOVE ITEM 

        public void AddItemToInventory(short slotIndex, IInventoryItem item, bool playSound = true, AudioCue soundToPlay = null)
        {
            if (playSound && soundToPlay == null) soundToPlay = _basePickupSound;
            if (_pickupHandler.PickupItem(slotIndex, item, playSound, soundToPlay))
            {
                item.ForceDropItemFromInventory += IInventoryItem_ForceDropItemFromInventory;
                InventoryHandlerData.NotifyAddedToInventory(item);
            }
            else
            {
                Debug.Log("item non récupéré inventaire plein");
                
            }
        }

        public IInventoryItem GenerateAndAddItemToInventory(short slotIndex, ushort itemID, bool playSound = true, AudioCue soundToPlay = null)
        {
            if (playSound && soundToPlay == null) soundToPlay = _basePickupSound;
            var prefab = _itemDatabase.GetPrefab(itemID);
            var instance = Instantiate(prefab);
            var item = instance.GetComponent<IInventoryItem>();
            if (_pickupHandler.PickupItem(slotIndex, item, playSound, soundToPlay))
            {
                item.ForceDropItemFromInventory += IInventoryItem_ForceDropItemFromInventory;
                InventoryHandlerData.NotifyAddedToInventory(item);
            }
            else
            {
                Debug.Log("item non récupéré inventaire plein");
            }
            return item;
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

        private void HandleDropInput()
        {
            if (!IsEnabled)
            {
                if (_dropHandler.IsChargingDrop)
                    _dropHandler.CancelDrop();
                return;
            }
            _dropHandler.HandleDropInput(soundToPlay: _baseDropSound);
        }
        #endregion

        #region InteractInInventory Handling & Input callback
        private void InteractInInventory_started(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            if (!IsEnabled)
            {
                _useFromInventoryHandler.Canceled();
                return;
            }
            _useFromInventoryHandler.Started();
        }
        private void InteractInInventory_canceled(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            _useFromInventoryHandler.Canceled();
        }
        private void HandleInteractInInventoryInput()
        {
            if (!IsEnabled)
            {
                if (_useFromInventoryHandler.IsHolding && !_useFromInventoryHandler.HasBeenPerformed)
                {
                    _useFromInventoryHandler.Canceled();
                }
                return;
            }
            _useFromInventoryHandler.HandeInteractInInventoryInput();
        }
        #endregion

        #region SCROLL HANDLING
        private void Scroll_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            if (!IsEnabled || FreezeScrollingInventory) return;
            int direction = -Mathf.RoundToInt(obj.ReadValue<Vector2>().y);
            if (direction != 0)
                _scrollHandler.Scroll(direction);
        }
        #endregion

        #region SELECT HANDLING
        private void Select_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
        {
            if (!IsEnabled) return;
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
