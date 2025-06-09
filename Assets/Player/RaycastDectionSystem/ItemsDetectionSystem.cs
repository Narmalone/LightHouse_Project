using LightHouse.Interactions;
using LightHouse.Inventory;
using UnityEngine;

namespace LightHouse.Items.Detection
{
    public class ItemsDetectionSystem : MonoBehaviour
    {
        public static Vector3 RayOrigin;
        public static Vector3 RayDirection;
        public static Transform CurrentHitedObjectPosition;

        [SerializeField] private Camera _camera;
        [SerializeField] private float _raycastDistance = 5f;
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
            _camera = Camera.main;
            _itemNameDetector = new RaycastDetector<IItemName>(_camera, _raycastDistance, _nameLayers, _blockingLayers, _qti);
            _interactableDetector = new RaycastDetector<IInteractable>(_camera, _raycastDistance, _interactableLayers, _blockingLayers, _qti);
            _inventoryDetector = new RaycastDetector<IInventoryItem>(_camera, _raycastDistance, _inventoryLayers, _blockingLayers, _qti);
        }

        private void Update()
        {
            Ray ray = _camera.ScreenPointToRay(new Vector3(Screen.width / 2f, Screen.height / 2f));
            RayOrigin = ray.origin;
            RayDirection = ray.direction;

            bool nameDetected = false;
            bool interactableDetected = false;
            bool inventoryDetected = false;

            if (Physics.Raycast(ray, out RaycastHit hit, _raycastDistance, _blockingLayers, _qti))
            {
                int layer = hit.collider.gameObject.layer;
                int layerMask = 1 << layer;

                bool isBlocking = (_blockingLayers.value & layerMask) != 0;
                bool isTarget = ((_nameLayers | _interactableLayers | _inventoryLayers) & layerMask) != 0;

                if (isBlocking && !isTarget)
                {
                    // Objet non interactif qui bloque la vue
                }
                else
                {
                    if ((_nameLayers.value & layerMask) != 0)
                    {
                        _itemNameDetector.ProcessRaycastHit(hit);
                        nameDetected = true;
                    }

                    if ((_interactableLayers.value & layerMask) != 0)
                    {
                        _interactableDetector.ProcessRaycastHit(hit);
                        interactableDetected = true;
                    }

                    if ((_inventoryLayers.value & layerMask) != 0)
                    {
                        _inventoryDetector.ProcessRaycastHit(hit);
                        inventoryDetected = true;
                    }

                    CurrentHitedObjectPosition = _itemNameDetector.LastSeenObject?.transform;
                }
            }

            if (!nameDetected) _itemNameDetector.FinishFrame();
            if (!interactableDetected) _interactableDetector.FinishFrame();
            if (!inventoryDetected) _inventoryDetector.FinishFrame();
        }
    }
}
