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
        private LayerMask _blockingLayers = ~0;
        private QueryTriggerInteraction _queryTriggerInteraction = QueryTriggerInteraction.Ignore;

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
        public virtual void UpdateRay()
        {
            Ray cameraRay = _camera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0f));
            RaycastHit[] hits = Physics.RaycastAll(cameraRay, _raycastDistance, _blockingLayers, _queryTriggerInteraction);
            Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance)); // tri par distance

            RayOrigin = cameraRay.origin;
            RayDirection = cameraRay.direction;
            foreach (var hit in hits)
            {
                int hitLayer = hit.collider.gameObject.layer;

                // 🎯 Si c’est une couche bloquante mais non interactive, on stoppe là
                if (((1 << hitLayer) & _blockingLayers) != 0 && ((1 << hitLayer) & _targetMasks) == 0)
                {
                    ResetSeenObject();
                    return;
                }

                // 🎯 Si c’est une couche interactive (Inventory, Interactable)
                if (((1 << hitLayer) & _targetMasks) != 0)
                {
                    GameObject go = hit.collider.gameObject;

                    if (ItemRegistry.IsMarked(hit.collider, out GameObject markedObj))
                        HandleNewObject(markedObj);
                    else
                        HandleNewObject(go);

                    return; // On traite le premier interactif qu’on rencontre
                }
            }

            // Aucun interactif trouvé
            ResetSeenObject();
            Debug.DrawRay(_camera.transform.position, cameraRay.direction * _raycastDistance, Color.cyan);
        }
        #endregion

        #region RAYCAST METHODS
        protected virtual void HandleNewObject(GameObject go)
        {
            if (_lastSeenObject == go)
                return;
            Debug.Log("ray changed");
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

