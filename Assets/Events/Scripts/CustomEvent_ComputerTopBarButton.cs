using System;
using UnityEngine;

[CreateAssetMenu(fileName = "Event_", menuName = "CustomEvent/Event_ComputerTopBarButton", order = 0)]
public class CustomEvent_ComputerTopBarButton : ScriptableObject
{
    public event Action<ComputerTabs> handle;

    public void Raise(ComputerTabs target)
    {
        handle?.Invoke(target);
    }
}