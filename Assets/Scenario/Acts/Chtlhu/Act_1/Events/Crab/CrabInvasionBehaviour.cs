using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrabInvasionBehaviour : MonoBehaviour
{
    [SerializeField] private CrabInvasionEvent _event;

    private void Awake()
    {
        _event.EventAction += OnCrabInvasionStart; 
    }

    private void OnDestroy()
    {
        _event.EventAction -= OnCrabInvasionStart;
    }

    private void OnCrabInvasionStart()
    {
        GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
        obj.GetComponent<Renderer>().material.color = Color.cyan;

    }
}
