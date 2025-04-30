using LightHouse.Interactions;
using LightHouse.Inventory;
using LightHouse.Items.Detection;
using System;
using UnityEngine;

public class UnifiedRaycastSystem : MonoBehaviour
{
    public static Vector3 RayOrigin;
    public static Vector3 RayDirection;
    [SerializeField] private Camera _camera;
    [SerializeField] private float _raycastDistance;
    [SerializeField] private LayerMask _blockingLayers;
    [SerializeField] private QueryTriggerInteraction _qti;

    [SerializeField] private LayerMask _interactableLayers;
    [SerializeField] private LayerMask _inventoryLayers;
    [SerializeField] private LayerMask _nameLayers;

    private RaycastDetector<IItemName> _itemNameDetector;
    private RaycastDetector<IInteractable> _interactableDetector;
    private RaycastDetector<IInventoryItem> _inventoryDetector;
    public RaycastDetector<IItemName> ItemNameDetector => _itemNameDetector;
    public RaycastDetector<IInteractable> InteractableDetector => _interactableDetector;
    public RaycastDetector<IInventoryItem> InventoryDetector => _inventoryDetector;


    private void Awake()
    {
        _itemNameDetector = new RaycastDetector<IItemName>(_camera, _raycastDistance, _nameLayers, _blockingLayers, _qti);
        _interactableDetector = new RaycastDetector<IInteractable>(_camera, _raycastDistance, _interactableLayers, _blockingLayers, _qti);
        _inventoryDetector = new RaycastDetector<IInventoryItem>(_camera, _raycastDistance, _inventoryLayers, _blockingLayers, _qti);
    }

    private void Update()
    {
        Ray ray = _camera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2));
        var hits = Physics.RaycastAll(ray, _raycastDistance, _blockingLayers, _qti);
        Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));
        RayOrigin = ray.origin;
        RayDirection = ray.direction;

        LayerMask combinedTargetMask = _nameLayers | _interactableLayers | _inventoryLayers;
        bool anyDetected = false;

        foreach (var hit in hits)
        {
            int layer = hit.collider.gameObject.layer;
            int layerMask = 1 << layer;

            bool isBlocking = (_blockingLayers.value & layerMask) != 0;
            bool isTarget = (combinedTargetMask.value & layerMask) != 0;

            if (isBlocking && !isTarget)
            {
                // Premier élément rencontré est bloquant non interactif
                break;
            }

            if (isTarget)
            {
                _itemNameDetector.ProcessRaycastHit(hit);
                _interactableDetector.ProcessRaycastHit(hit);
                _inventoryDetector.ProcessRaycastHit(hit);
                anyDetected = true;
                break; // ✅ on s'arrête à la première cible valable
            }

            // Si ni bloquant ni target (ex: trigger invisible) → continue
        }


        if (!anyDetected)
        {
            _itemNameDetector.FinishFrame();
            _interactableDetector.FinishFrame();
            _inventoryDetector.FinishFrame();
        }


    }
}
