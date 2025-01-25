using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LitaniaBehaviour : MonoBehaviour
{
    [SerializeField] private LitaniaEvent _event;

    private void Awake()
    {
        _event.EventAction += OnLitaniaEvent;
    }

    private void OnLitaniaEvent()
    {
        // Créer et configurer la sphère bleue
        GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        sphere.GetComponent<Renderer>().material.color = Color.yellow;
    }

    private void OnDestroy()
    {
        _event.EventAction -= OnLitaniaEvent;
    }
}
