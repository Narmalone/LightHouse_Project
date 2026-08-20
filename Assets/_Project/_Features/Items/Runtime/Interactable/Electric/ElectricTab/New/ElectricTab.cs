using AYellowpaper.SerializedCollections;
using LightHouse.Features.Electricity;
using System;
using UnityEngine;

public class ElectricTab : MonoBehaviour
{
    [SerializeField] private SerializedDictionary<ElectricityZones, ElectricalSlot> _electricalSlots;

    /// <summary>
    /// Same role as the old ElectricalPannel.OnSwitchElectricityChanged, just re-broadcast
    /// from whichever ElectricalSlot's switch changed state.
    /// </summary>
    public event Action<bool, ElectricityZones> OnSwitchElectricityChanged;

    private void Awake()
    {
        foreach (ElectricalSlot slot in _electricalSlots.Values)
        {
            if (slot == null) continue;
            slot.OnElectricityStateChanged += Slot_OnElectricityStateChanged;
        }
    }

    private void OnDestroy()
    {
        foreach (ElectricalSlot slot in _electricalSlots.Values)
        {
            if (slot == null) continue;
            slot.OnElectricityStateChanged -= Slot_OnElectricityStateChanged;
        }
    }

    private void Slot_OnElectricityStateChanged(ElectricityZones zone, bool state)
    {
        OnSwitchElectricityChanged?.Invoke(state, zone);
    }

    public ElectricalSlot GetElectricalSlot(ElectricityZones zone)
    {
        if (_electricalSlots.TryGetValue(zone, out ElectricalSlot slots))
        {
            return slots;
        }
        return null;
    }

    public void OnEnablePannelInteractibility()
    {
        foreach (ElectricalSlot slot in _electricalSlots.Values)
            slot?.SetInteractable(true);
    }

    public void OnDisablePannelInteractibility()
    {
        foreach (ElectricalSlot slot in _electricalSlots.Values)
            slot?.SetInteractable(false);
    }

    public void DownAllSwitches()
    {
        foreach (ElectricalSlot slot in _electricalSlots.Values)
            slot?.Shutdown();
    }
}