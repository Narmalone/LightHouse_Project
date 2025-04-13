using LightHouse.Inventory;
using LightHouse.Utilities;
using System;
using UnityEngine;

namespace LightHouse.Inventory
{
    public class InventoryRaycastDetector : MonoBehaviour
    {
        #region FIELDS & PROPERTIES
        public event Action<IInventoryItem> OnItemDetected;
        public event Action OnItemLost;

        [Header("Raycast Settings")]
        [SerializeField] private Camera _camera;
        [SerializeField] private float _raycastDistance = 3f;
        [SerializeField] private LayerMask _targetMasks = ~0;
        [SerializeField] private QueryTriggerInteraction _queryTriggerInteraction = QueryTriggerInteraction.Ignore;

        public Vector3 RayOrigin { get; private set; }
        public Vector3 RayDirection { get; private set; }

        private IInventoryItem _lastSeenItem;
        private GameObject _lastSeenObject;
        #endregion

        #region MONO'S CALLBACK
        private void Update()
        {
            Ray cameraRay = _camera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0f));
            RaycastHit hit;

            RayOrigin = _camera.transform.position;
            RayDirection = cameraRay.direction;
            if(RaycastUtility.TryRaycast(cameraRay, _raycastDistance, _targetMasks, _queryTriggerInteraction, out hit))
                HandleNewObject(hit.collider.gameObject);
            else
                ResetSeenObject();
            Debug.DrawRay(_camera.transform.position, cameraRay.direction * _raycastDistance, Color.cyan);
        }
        #endregion

        #region RAYCAST METHODS
        private void HandleNewObject(GameObject go)
        {
            if (_lastSeenObject == go)
                return;

            _lastSeenObject = go;
            go.TryGetComponent(out _lastSeenItem);
            if (_lastSeenItem != null)
                OnItemDetected?.Invoke(_lastSeenItem);
        }

        private void ResetSeenObject()
        {
            if (_lastSeenItem != null)
            {
                OnItemLost?.Invoke();
                _lastSeenItem = null;
            }
            if(_lastSeenObject != null)
                _lastSeenObject = null;
        }
        #endregion
    }
}

