using System;
using UnityEngine;

[CreateAssetMenu(fileName = "Event_", menuName = "CustomEvent/Event_WindDirection", order = 0)]
public class CustomEvent_WindDirection : ScriptableObject
{
    public event Action<WindDirection> handle;

    public void Raise(WindDirection windDirection)
    {
        handle?.Invoke(windDirection);
    }
}
