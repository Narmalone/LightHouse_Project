using System;
using UnityEngine;

[CreateAssetMenu(fileName = "Event_", menuName = "CustomEvent/Event_Electric", order = 0)]
public class CustomEvent_ElectricZone : ScriptableObject
{
    public event Action<ElectricityZones> handle;

    public void Raise(ElectricityZones zone)
    {
        handle?.Invoke(zone);
    }
}
