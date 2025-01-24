using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LighthouseTorchsBehaviour : MonoBehaviour
{
    [SerializeField] private LighthouseTorchsEvent _event;

    private void Awake()
    {
        _event.EventAction += OnLighthouseTorchsEvent;
    }

    private void OnLighthouseTorchsEvent()
    {
        // Créer et configurer la sphère bleue
        GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        sphere.GetComponent<Renderer>().material.color = Color.green;
    }

    private void OnDestroy()
    {
        _event.EventAction -= OnLighthouseTorchsEvent;
    }
}
