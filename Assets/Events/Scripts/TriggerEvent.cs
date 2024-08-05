using System;
using UnityEngine;

public class TriggerEvent : MonoBehaviour
{
    public event Action<GameObject> OnEntered;
    public event Action<GameObject> OnStaying;
    public event Action<GameObject> OnExited;

    [Header("Detection")]
    [SerializeField] bool _useTriggerEnter = true;
    [SerializeField] bool _useTriggerStay = true;
    [SerializeField] bool _useTriggerExit = true;

    [Header("Detection")]
    [SerializeField] private bool _needMaskAndLayerToEvent = false;

    [SerializeField] private LayerMask _targetsMasks;

    [SerializeField, TagSelector] private string _targetTag;

    private void OnTriggerEnter(Collider other)
    {
        if (!_useTriggerEnter) return;
        if (GetConditions(other))
        {
            OnEntered?.Invoke(other.gameObject);
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (!_useTriggerStay) return;

        if (GetConditions(other))
        {
            OnStaying?.Invoke(other.gameObject);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!_useTriggerExit) return;

        if (GetConditions(other))
        {
            OnExited?.Invoke(other.gameObject);
        }
    }

    private bool GetConditions(Collider other)
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
}
