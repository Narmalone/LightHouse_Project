using System;
using UnityEngine;

[CreateAssetMenu(fileName = "Event_", menuName = "CustomEvent/Event_ItemBase", order = 0)]
public class CustomEvent_ItemBase: ScriptableObject
{
    public event Action<ItemBase> handle;

    public void Raise(ItemBase item)
    {
        handle?.Invoke(item);
    }
}
