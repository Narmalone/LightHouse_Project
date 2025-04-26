using System;
using LightHouse.Interactions;
using UnityEngine;

namespace LightHouse.Items.Detection
{
    [System.Serializable]
    public class RaycastDetector<T> : PlayerRaycastSystem where T : class
    {
        public Action<T> OnDetected;
        private T _lastSeenComponent;
        public T LastSeenComponent => _lastSeenComponent;

        public Action OnItemDestroyed;

        public RaycastDetector(Camera cam, float distance, LayerMask masks, LayerMask blockingLayers, QueryTriggerInteraction qti) : base(cam, distance, masks, blockingLayers, qti)
        {
        }

        protected override void HandleNewObject(GameObject go)
        {
            if (_lastSeenObject == go)
                return;
            _lastSeenObject = go;
            go.TryGetComponent(out _lastSeenComponent); //try to get the target component
            //if we have something it's bcs its in the good layer
            if (_lastSeenComponent != null)
            {
                OnDetected?.Invoke(_lastSeenComponent);
                if(_lastSeenComponent is IDestroyable destroy)
                {
                    destroy.OnDestroyed += Destroy_OnDestroyed;
                }
            }
        }

        private void Destroy_OnDestroyed()
        {
            OnItemDestroyed?.Invoke();
        }

        protected override void ResetSeenObject()
        {
            base.ResetSeenObject();
            if (_lastSeenComponent != null)
            {
                _lastSeenComponent = null;
                if (_lastSeenComponent is IDestroyable destroy)
                {
                    destroy.OnDestroyed -= Destroy_OnDestroyed;
                }
            }
        }
    }
}
