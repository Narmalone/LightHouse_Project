using System;
using UnityEngine;

namespace LightHouse.Items.Detection
{
    [System.Serializable]
    public class RaycastDetector<T> : PlayerRaycastSystem where T : class
    {
        public Action<T> OnDetected;
        private T _lastSeenComponent;
        public T LastSeenComponent => _lastSeenComponent;

        public RaycastDetector(Camera cam, float distance, LayerMask masks, QueryTriggerInteraction qti)
            : base(cam, distance, masks, qti) { }

        protected override void HandleNewObject(GameObject go)
        {
            base.HandleNewObject(go);
            go.TryGetComponent(out _lastSeenComponent); //try to get the target component
            //if we have something it's bcs its in the good layer
            if (_lastSeenComponent != null)
                OnDetected?.Invoke(_lastSeenComponent);
            //if not probably bcs we got default (avoid raycasting through walls)
            else
                ResetSeenObject();
        }

        protected override void ResetSeenObject()
        {
            base.ResetSeenObject();
            if (_lastSeenComponent != null)
                _lastSeenComponent = null;
        }
    }
}
