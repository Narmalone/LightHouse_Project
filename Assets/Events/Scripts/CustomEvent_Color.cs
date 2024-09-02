using System;
using UnityEngine;

[CreateAssetMenu(fileName = "Event_", menuName ="CustomEvent/Event_Color", order = 0)]
public class CustomEvent_Color : ScriptableObject
{
    public event Action<Color> handle;

    public void Raise(Color item)
    {
        handle?.Invoke(item);
    }
}
