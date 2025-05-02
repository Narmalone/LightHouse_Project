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

        public void ProcessRaycastHit(RaycastHit hit)
        {
            GameObject go = hit.collider.gameObject;

            if (go == _lastSeenObject) return;

            if (ItemRegistry.IsMarked(hit.collider, out GameObject markedObj))
                go = markedObj;

            _lastSeenObject = go;
            go.TryGetComponent(out T component);
            if (component != null)
            {
                _lastSeenComponent = component;
                OnDetected?.Invoke(component);
                if (component is IDestroyable destroyable)
                    destroyable.OnDestroyed += Destroy_OnDestroyed;
            }
        }
        public void FinishFrame()
        {
            if (_lastSeenComponent != null)
                ResetSeenObject();
        }

        private void Destroy_OnDestroyed()
        {
            OnItemDestroyed?.Invoke();
        }

        protected override void ResetSeenObject()
        {
            base.ResetSeenObject();

            if(_lastSeenComponent != null)
            {
                if (_lastSeenComponent is IDestroyable destroyable)
                    destroyable.OnDestroyed -= Destroy_OnDestroyed;

                _lastSeenComponent = null;
            }
        }

    }
}
