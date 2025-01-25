using System;
using UnityEngine;

[CreateAssetMenu(fileName = "Event_", menuName ="CustomEvent/Event_IItem", order = 0)]
public class CustomEvent_IItem : ScriptableObject
{
    public event Action<IItem> handle;

    public void Raise(IItem item)
    {
        handle?.Invoke(item);
    }
}
