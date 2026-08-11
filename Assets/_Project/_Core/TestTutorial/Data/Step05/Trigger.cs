using System;
using UnityEngine;

public class Trigger : MonoBehaviour
{
    public event Action<Collider> OnEntered;

    private void OnTriggerEnter(Collider other)
    {
        OnEntered?.Invoke(other);
    }
}