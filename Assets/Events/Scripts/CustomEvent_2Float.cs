using System;
using UnityEngine;

[CreateAssetMenu(fileName = "Event_", menuName ="CustomEvent/Event_2Float", order = 0)]
public class CustomEvent_2Float : ScriptableObject
{
    public event Action<float, float> handle;

    public void Raise(float value1, float value2)
    {
        handle?.Invoke(value1, value2);
    }
}
