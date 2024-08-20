using System;
using UnityEngine;

[CreateAssetMenu(fileName = "Event_", menuName ="CustomEvent/Event_Float", order = 0)]
public class CustomEvent_Float : ScriptableObject
{
    public event Action<float> handle;

    public void Raise(float item)
    {
        handle?.Invoke(item);
    }
}
