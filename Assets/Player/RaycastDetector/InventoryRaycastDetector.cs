using LightHouse.Inventory;
using LightHouse.Utilities;
using System;
using UnityEngine;

public class InventoryRaycastDetector : MonoBehaviour
{
    [Header("Raycast Settings")]
    [SerializeField] private float raycastDistance = 3f;
    [SerializeField] private LayerMask targetMask = ~0;
    [SerializeField] private QueryTriggerInteraction queryTriggerInteraction = QueryTriggerInteraction.Ignore;

    public event Action<IInventoryItem> OnItemDetected;
    public event Action OnItemLost;

    private IInventoryItem _lastSeenItem;
    private GameObject _lastSeenObject;
    [SerializeField] private Camera _camera;

    public Vector3 RayOrigin;
    public Vector3 RayDirection;
    public bool RaycastHitItem { get; private set; }

    private void Update()
    {
        Ray cameraRay = _camera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0f));
        RayOrigin = _camera.transform.position;
        RayDirection = cameraRay.direction;
        RaycastHit hit;

        RaycastHitItem = RaycastUtility.TryRaycast(cameraRay, raycastDistance, targetMask, queryTriggerInteraction, out hit);

        if (RaycastHitItem)
        {
            HandleNewObject(hit.collider.gameObject);
        }
        else
        {
            ResetSeenObject();
        }

        Debug.DrawRay(_camera.transform.position, cameraRay.direction * raycastDistance, Color.cyan);
    }

    private void HandleNewObject(GameObject go)
    {
        if (_lastSeenObject == go)
            return;

        _lastSeenObject = go;
        go.TryGetComponent(out _lastSeenItem);

        if (_lastSeenItem != null)
        {
            OnItemDetected?.Invoke(_lastSeenItem);
        }
    }

    private void ResetSeenObject()
    {
        if (_lastSeenItem != null)
        {
            OnItemLost?.Invoke();
        }

        _lastSeenItem = null;
        _lastSeenObject = null;
    }

    public IInventoryItem GetCurrentItemSeen() => _lastSeenItem;
}
