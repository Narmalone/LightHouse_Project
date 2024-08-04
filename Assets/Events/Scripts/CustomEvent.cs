using System;
using UnityEngine;

[CreateAssetMenu(fileName = "Event_", menuName ="CustomEvent/Event", order = 0)]
public class CustomEvent : ScriptableObject
{
    public event Action handle;

    public void Raise()
    {
        handle?.Invoke();
    }
}
