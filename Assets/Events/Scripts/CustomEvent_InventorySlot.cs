using System;
using UnityEngine;

[CreateAssetMenu(fileName = "Event_", menuName = "CustomEvent/Event_InventorySlot", order = 0)]
public class CustomEvent_InventorySlot : ScriptableObject
{
    public event Action<InventorySlot> handle;

    public void Raise(InventorySlot slot)
    {
        handle?.Invoke(slot);
    }
}
