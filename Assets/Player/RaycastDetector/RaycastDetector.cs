using System;
using UnityEngine;

namespace LightHouse.Items.Detection
{
    public class RaycastDetector<T> : CameraRaycastDetector where T : class
    {
        public Action<T> OnDetected;
        private T _lastSeenComponent;
        public T LastSeenComponent => _lastSeenComponent;

        public RaycastDetector(Camera cam, float distance, LayerMask masks, QueryTriggerInteraction qti)
            : base(cam, distance, masks, qti) { }

        protected override void HandleNewObject(GameObject go)
        {
            base.HandleNewObject(go);
            go.TryGetComponent(out _lastSeenComponent);
            if (_lastSeenComponent != null)
                OnDetected?.Invoke(_lastSeenComponent);
        }

        protected override void ResetSeenObject()
        {
            base.ResetSeenObject();
            if (_lastSeenComponent != null)
                _lastSeenComponent = null;
        }
    }
}
