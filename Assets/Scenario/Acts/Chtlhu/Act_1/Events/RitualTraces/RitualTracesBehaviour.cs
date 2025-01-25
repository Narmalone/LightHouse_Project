using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RitualTracesBehaviour : MonoBehaviour
{
    [SerializeField] private RitualTracesEvent _event;

    private void Awake()
    {
        _event.EventAction += OnRitualTracesEvent;
    }

    private void OnRitualTracesEvent()
    {
        // Créer et configurer la sphère bleue
        GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        sphere.GetComponent<Renderer>().material.color = Color.magenta;
    }

    private void OnDestroy()
    {
        _event.EventAction -= OnRitualTracesEvent;
    }
}
