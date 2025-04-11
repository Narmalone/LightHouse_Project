using LightHouse.Inventory;
using UnityEngine;

public class InventoryDropHandler : MonoBehaviour
{
    private ItemSlot[] _slots;
    [SerializeField] private Collider[] _dropOverlappingCollider = new Collider[0];

    [Header("Drop Settings")]
    [SerializeField] private float _maxDropPower = 10f;
    [SerializeField] private float _securityOverlapSphereRadius = 0.3f;
    [SerializeField] private LayerMask _securityObstacleMasks = 1 << 0;

    private Transform _target;

    public void Initialize(ItemSlot[] slots, Transform target)
    {
        _slots = slots;
        _target = target;
    }

    public void DropItem(int slotID, ushort itemGlobalID, ushort specificID, Vector3 pos, float force, bool enablePhysicsOnDrop, out IInventoryItem droppedItem)
    {
        droppedItem = null;
        if (!_slots[slotID].SlotDatas.HasItem) return;
        IInventoryItem item = PoolManager.Get(itemGlobalID, specificID, enablePhysicsOnDrop);
        droppedItem = item;

        _slots[slotID].RemoveItemFromSlot(item.ItemSpecificID);

        Vector3 finalPos = GetAdjustedDropPosition(_target, pos);

        item.GetGameObject().transform.position = finalPos;

        item.IsItemInInventory = false;
        
        Rigidbody rb = item.GetRigidBody();

        if(finalPos == pos)
            rb.AddForce(_target.forward * force, ForceMode.Impulse);

        if (item is IInventoryItemCallback callback) callback.OnItemRemovedFromInventory();
        InventoryHandlerData.NotifyDrop(slotID);
    }

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

    private void OnDrawGizmos()
    {
        if (_target == null) return;
        Vector3 start = _target.position;
        float sphereRadius = _securityOverlapSphereRadius;
        Collider[] colliders = Physics.OverlapSphere(start, sphereRadius, _securityObstacleMasks);
        _dropOverlappingCollider = colliders;
        Gizmos.color = _dropOverlappingCollider.Length > 0 ? Color.red : Color.green;
        Gizmos.DrawSphere(_target.position, 0.3f);
    }

}
