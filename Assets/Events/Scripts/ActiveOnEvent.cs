using System;
using UnityEngine;

public class ActiveOnEvent : MonoBehaviour
{
    [SerializeField] private CustomEvent _eventActive;
    [SerializeField] private bool _activeOnStart;

    private void Awake()
    {
        _eventActive.handle += OnActive;
    }

    private void Start()
    {
        gameObject.SetActive(_activeOnStart);
    }

    private void OnDestroy()
    {
        _eventActive.handle -= OnActive;
    }

    private void OnActive()
    {
        gameObject.SetActive(!gameObject.activeSelf);
    }
}
