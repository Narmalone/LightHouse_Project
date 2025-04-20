using LightHouse.Utilities;
using System;
using UnityEngine;

namespace LightHouse.Items.Detection
{
    [System.Serializable]
    public class PlayerRaycastSystem
    {
        #region FIELDS & PROPERTIES
        public Action<GameObject> OnItemDetected;
        public Action OnItemLost;

        //Ray settings
        private Camera _camera;
        private float _raycastDistance = 3f;
        private LayerMask _targetMasks = ~0;
        private QueryTriggerInteraction _queryTriggerInteraction = QueryTriggerInteraction.Ignore;

        //Ray infos
        public Vector3 RayOrigin { get; private set; }
        public Vector3 RayDirection { get; private set; }

        private GameObject _lastSeenObject;
        public GameObject LastSeenObject => _lastSeenObject;
        #endregion

        #region Constructor
        public PlayerRaycastSystem(Camera cam, float distance, LayerMask masks, QueryTriggerInteraction qti)
        {
            _camera = cam;
            _raycastDistance = distance;
            _targetMasks = masks;
            _queryTriggerInteraction = qti;
        }
        #endregion

        #region MONO'S CALLBACK
        public virtual void UpdateRay()
        {
            Ray cameraRay = _camera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0f));
            RaycastHit hit;

            RayOrigin = _camera.transform.position;
            RayDirection = cameraRay.direction;
            if(RaycastUtility.TryRaycast(cameraRay, _raycastDistance, _targetMasks, _queryTriggerInteraction, out hit))
            {
                if (ItemRegistry.IsMarked(hit.collider, out GameObject markedObj))
                {
                    HandleNewObject(markedObj);
                }
                else 
                {
                    HandleNewObject(hit.collider.gameObject);
                }
            }
            else
                ResetSeenObject();
            Debug.DrawRay(_camera.transform.position, cameraRay.direction * _raycastDistance, Color.cyan);
        }
        #endregion

        #region RAYCAST METHODS
        protected virtual void HandleNewObject(GameObject go)
        {
            if (_lastSeenObject == go)
                return;
            _lastSeenObject = go;
            if(_lastSeenObject != null)
                OnItemDetected?.Invoke(go);
        }

        protected virtual void ResetSeenObject()
        {
            if(_lastSeenObject != null)
            {
                _lastSeenObject = null;
                OnItemLost?.Invoke();
            }
        }
        #endregion
    }
}

