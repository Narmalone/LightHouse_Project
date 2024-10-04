using System;
using UnityEngine;

[CreateAssetMenu(fileName = "Event_", menuName = "CustomEvent/Event_Int", order = 0)]
public class CustomEvent_Int : ScriptableObject
{
    public event Action<int> handle;

    public void Raise(int item)
    {
        handle?.Invoke(item);
    }
}
