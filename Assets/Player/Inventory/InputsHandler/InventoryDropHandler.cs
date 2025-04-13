using LightHouse.Inputs;
using LightHouse.Inventory;
using UnityEngine;

public class InventoryDropHandler : MonoBehaviour
{
    #region FIELDS
    [Header("Drop Settings")]
    [SerializeField] private float _maxDropPower = 10f;
    [SerializeField] private AnimationCurve _dropPowerCurve = AnimationCurve.Linear(0, 0, 1, 1);
    [SerializeField] private float _securityOverlapSphereRadius = 0.3f;
    [SerializeField] private LayerMask _securityObstacleMasks = 1 << 0;

    [Header("Debug only")]
    private ItemSlot[] _slots;
    [SerializeField] private Collider[] _dropOverlappingCollider = new Collider[0];
    private Transform _inventoryTarget;
    private float _dropChargeTimer = 0f;
    private float _dropPower = 0f;
    private bool _isChargingDrop = false;
    #endregion

    #region INIT
    public void Initialize(ItemSlot[] slots, Transform inventoryTarget)
    {
        _slots = slots;
        _inventoryTarget = inventoryTarget;
    }
    #endregion

    #region Main Drop Function
    public void DropItem(int slotID, ushort itemGlobalID, ushort specificID, Vector3 pos, float force, bool enablePhysicsOnDrop, out IInventoryItem droppedItem)
    {
        droppedItem = null;
        if (!_slots[slotID].SlotDatas.HasItem) return;
        IInventoryItem item = PoolManager.Get(itemGlobalID, specificID, enablePhysicsOnDrop);
        droppedItem = item;
        _slots[slotID].RemoveItemFromSlot(item.ItemSpecificID);

        Vector3 finalPos = GetAdjustedDropPosition(_inventoryTarget, pos);
        item.GetGameObject().transform.position = finalPos;
        item.IsItemInInventory = false;

        Rigidbody rb = item.GetRigidBody();
        if(finalPos == pos)
            rb.AddForce(_inventoryTarget.forward * force, ForceMode.Impulse);

        if (item is IInventoryItemCallback callback) callback.OnItemRemovedFromInventory();
        InventoryHandlerData.NotifyDrop(slotID);
    }
    #endregion

    #region Check Func
    private Vector3 GetAdjustedDropPosition(Transform target, Vector3 intendedDropPosition, int lengthToSafePos = 0)
    {
        Vector3 start = target.position;
        float sphereRadius = _securityOverlapSphereRadius; // Ajuste la taille en fonction des objets

        // 1. Vérifier si un obstacle est proche avec un OverlapSphere
        Collider[] colliders = Physics.OverlapSphere(start, sphereRadius, _securityObstacleMasks);
        _dropOverlappingCollider = colliders;
        if (colliders.Length > lengthToSafePos)
        {
            Vector3 adjustedPosition = start - target.forward * 0.5f; // Reculer légèrement pour ne pas être dans l'obstacle
            Debug.DrawRay(start, -target.forward * 0.5f, Color.yellow, 5.0f);
            Debug.Log("Adjusting position due to overlap result");
            return adjustedPosition;
        }

        return intendedDropPosition;
    }
    #endregion

    #region MONO CALLBACKS
    private void OnDrawGizmos()
    {
        if (_inventoryTarget == null) return;
        Vector3 start = _inventoryTarget.position;
        float sphereRadius = _securityOverlapSphereRadius;
        Collider[] colliders = Physics.OverlapSphere(start, sphereRadius, _securityObstacleMasks);
        _dropOverlappingCollider = colliders;
        Gizmos.color = _dropOverlappingCollider.Length > 0 ? Color.red : Color.green;
        Gizmos.DrawSphere(_inventoryTarget.position, 0.3f);
    }
    #endregion

    #region Input Handler
    public void HandleDropInput()
    {
        if (InputManager.Drop.IsPressed())
        {
            if (SlotManager.IsIndexInvalid(SlotManager.CurrentSlotIndex) || SlotManager.CurrentSelectedSlot.SlotDatas.ItemSpecificIds.Count <= 0)
                return;

            _isChargingDrop = true;
            _dropChargeTimer += Time.deltaTime;
            float curveValue = _dropPowerCurve.Evaluate(Mathf.Clamp01(_dropChargeTimer));
            _dropPower = curveValue * _maxDropPower;
        }
        else if (_isChargingDrop && InputManager.Drop.WasReleasedThisFrame())
        {
            if (SlotManager.IsIndexInvalid(SlotManager.CurrentSlotIndex) || SlotManager.CurrentSelectedSlot.SlotDatas.ItemSpecificIds.Count <= 0)
                return;
            DropItem
            (
                slotID: SlotManager.CurrentSelectedSlot.SlotDatas.SlotID,
                itemGlobalID: SlotManager.CurrentSelectedSlot.SlotDatas.ItemGlobalID,
                specificID: SlotManager.CurrentSelectedSlot.SlotDatas.ItemSpecificIds[0],
                pos: _inventoryTarget.position,
                force: _dropPower,
                enablePhysicsOnDrop: true,
                droppedItem: out IInventoryItem droppedItem
            );
            _dropPower = 0f;
            _dropChargeTimer = 0f;
            _isChargingDrop = false;
        }
        else if (InputManager.Drop.WasPerformedThisFrame())
        {
            if (SlotManager.IsIndexInvalid(SlotManager.CurrentSlotIndex) || SlotManager.CurrentSelectedSlot.SlotDatas.ItemSpecificIds.Count <= 0)
                return;
            DropItem
            (
                slotID: SlotManager.CurrentSelectedSlot.SlotDatas.SlotID,
                itemGlobalID: SlotManager.CurrentSelectedSlot.SlotDatas.ItemGlobalID,
                specificID: SlotManager.CurrentSelectedSlot.SlotDatas.ItemSpecificIds[0],
                pos: _inventoryTarget.position,
                force: 0.0f,
                enablePhysicsOnDrop: true,
                droppedItem: out IInventoryItem droppedItem
            );
        }
    }
    #endregion

}
