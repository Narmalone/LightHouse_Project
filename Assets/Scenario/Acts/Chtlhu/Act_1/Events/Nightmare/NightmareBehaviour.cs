using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NightmareBehaviour : MonoBehaviour
{
    [SerializeField] private NightmareEvent _event;

    private void Awake()
    {
        _event.EventAction += OnNightmareStart;
    }

    private void OnNightmareStart()
    {
        // Créer et configurer la sphère bleue
        GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        sphere.GetComponent<Renderer>().material.color = Color.red;
    }

    private void OnDestroy()
    {
        _event.EventAction -= OnNightmareStart;
    }
}
