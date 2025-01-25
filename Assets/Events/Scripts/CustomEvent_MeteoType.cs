using System;
using UnityEngine;

[CreateAssetMenu(fileName = "Event_", menuName = "CustomEvent/Event_WeatherType", order = 0)]
public class CustomEvent_WeatherType : ScriptableObject
{
    public event Action<WeatherType> handle;

    public void Raise(WeatherType item)
    {
        handle?.Invoke(item);
    }
}
