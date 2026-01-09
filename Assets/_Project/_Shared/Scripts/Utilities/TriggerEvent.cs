using System;
using LightHouse.CustomAttributes;
using UnityEngine;

namespace LightHouse.Utilities
{
    public class TriggerEvent : MonoBehaviour
    {
        public event Action<GameObject> OnEntered;
        public event Action<GameObject> OnStaying;
        public event Action<GameObject> OnExited;

        [Header("Detection")]
        [SerializeField] protected bool _useTriggerEnter = true;
        [SerializeField] protected bool _useTriggerStay = false;
        [SerializeField] protected bool _useTriggerExit = true;

        [Header("Detection")]
        [SerializeField] protected bool _needMaskAndLayerToEvent = false;

        [SerializeField] protected LayerMask _targetsMasks;

        [SerializeField, TagSelector] protected string _targetTag;

        public Collider Collider;

        protected virtual void OnTriggerEnter(Collider other)
        {
            if (!_useTriggerEnter) return;
            
            if (GetConditions(other))
            {
                OnTriggerEntered();
                OnEntered?.Invoke(other.gameObject);
            }
        }

        protected virtual void OnTriggerStay(Collider other)
        {
            if (!_useTriggerStay) return;

            if (GetConditions(other))
            {
                OnTriggerStayed();
                OnStaying?.Invoke(other.gameObject);
            }
        }

        protected virtual void OnTriggerExit(Collider other)
        {
            if (!_useTriggerExit) return;

            if (GetConditions(other))
            {
                OnTriggerExited();
                OnExited?.Invoke(other.gameObject);
            }
        }

        protected virtual bool GetConditions(Collider other)
        {
            if (_needMaskAndLayerToEvent)
            {
                if ((_targetsMasks & (1 << other.gameObject.layer)) > 0 && other.CompareTag(_targetTag))
                {
                    return true;
                }
            }
            else
            {
                if ((_targetsMasks & (1 << other.gameObject.layer)) > 0 || other.CompareTag(_targetTag))
                {
                    return true;
                }
            }
            return false;
        }

        protected virtual void OnTriggerEntered() { }
        protected virtual void OnTriggerStayed() { }
        protected virtual void OnTriggerExited() { }
    }
}
