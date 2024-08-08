using System;
using UnityEngine;

[CreateAssetMenu(fileName = "Event_", menuName ="CustomEvent/Event_String", order = 0)]
public class CustomEvent_String : ScriptableObject
{
    public event Action<String> handle;

    public void Raise(String item)
    {
        handle?.Invoke(item);
    }
}
