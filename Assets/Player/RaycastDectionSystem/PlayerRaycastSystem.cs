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
        protected Camera _camera;
        protected float _raycastDistance = 3f;
        protected LayerMask _targetMasks = ~0;
        protected LayerMask _blockingLayers = ~0;
        protected QueryTriggerInteraction _queryTriggerInteraction = QueryTriggerInteraction.Ignore;

        //Ray infos
        public Vector3 RayOrigin { get; private set; }
        public Vector3 RayDirection { get; private set; }

        protected GameObject _lastSeenObject;
        public GameObject LastSeenObject => _lastSeenObject;
        #endregion

        #region Constructor
        public PlayerRaycastSystem(Camera cam, float distance, LayerMask masks, LayerMask blockingLayers, QueryTriggerInteraction qti)
        {
            _camera = cam;
            _raycastDistance = distance;
            _targetMasks = masks;
            _queryTriggerInteraction = qti;
            _blockingLayers = blockingLayers;
        }
        #endregion

        #region MONO'S CALLBACK
       
        #endregion

        #region RAYCAST METHODS

        protected virtual void ResetSeenObject()
        {
            if (_lastSeenObject != null)
            {
                _lastSeenObject = null;
                OnItemLost?.Invoke();
            }
        }
        #endregion
    }
}

