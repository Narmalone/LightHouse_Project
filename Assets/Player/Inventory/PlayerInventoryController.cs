using LightHouse.Inputs;
using LightHouse.Inventory;
using UnityEngine;


//TO DOO ----
//Relier les systèmes suivants::
//lier les booléens, l'objet est dans l'inventaire / ne l'est pas
//lier les autres interfaces IInventoryUsable, IinventoryCallback, ne pas oublier les delegates etc..
//refaire le système de key avec keytypes
//Commenter le code, éviter effets de bords, surplus de variable, ajouter les events associés
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

    [Header("UI")]
    [SerializeField] private InventoryUIController _inventoryUiController;

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
    }

    private void Start()
    {
        _slots = _inventoryUiController.GenerateItemSlot(_inventoryCapacity);
        _pickupHandler = new InventoryPickupHandler(_slots, _inventoryCapacity);
        _scrollHandler = new InventoryScrollHandler(_slots, _inventoryCapacity);
        _dropHandler.Initialize(_slots, _inventoryTarget);
        InputManager.Scroll.performed += Scroll_performed;
    }

    private void OnDisable()
    {
        InputManager.Scroll.performed -= Scroll_performed;
    }

    private void OnDestroy()
    {
        _raycastDetector.OnItemDetected -= HandleItemDetected;
        _raycastDetector.OnItemLost -= ResetSeenObject;
    }

    private void Update()
    {
        if (!_scrollHandler.IsIndexInvalid(_scrollHandler.CurrentIndex) && CurrentSelectedSlot.SlotDatas.ItemSpecificIds.Count > 0)
            HandleDropInputs();

        if (InputManager.PickUp.WasPerformedThisFrame())
            _pickupHandler.PickupItem(_scrollHandler.CurrentIndex, _lastInventoryItemSeen);
    }

    private void LateUpdate()
    {
        _inventoryTarget.position = _raycastDetector.RayOrigin + _raycastDetector.RayDirection.normalized;
    }

    #region RAYCAST DELEGATES
    private void HandleItemDetected(IInventoryItem item) => _lastInventoryItemSeen = item;

    private void ResetSeenObject()
    {
        if (_lastObjectSeen != null)
            _lastObjectSeen = null;

        if (_lastInventoryItemSeen != null)
            _lastInventoryItemSeen = null;
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
            _dropHandler.DropItem
            (
                slotID: CurrentSelectedSlot.SlotDatas.SlotID,
                itemGlobalID: CurrentSelectedSlot.SlotDatas.ItemGlobalID,
                pos: _inventoryTarget.position,
                force: _dropPower,
                enablePhysicsOnDrop: true,
                out IInventoryItem droppedItem
            );

            _dropPower = 0f;
            _dropChargeTimer = 0f;
            _isChargingDrop = false;
        }
        else if (InputManager.Drop.WasPerformedThisFrame())
        {
            _dropHandler.DropItem
            (
                slotID: CurrentSelectedSlot.SlotDatas.SlotID,
                itemGlobalID: CurrentSelectedSlot.SlotDatas.ItemGlobalID,
                pos: _inventoryTarget.position,
                force: 0.0f,
                enablePhysicsOnDrop: true,
                out IInventoryItem droppedItem
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
