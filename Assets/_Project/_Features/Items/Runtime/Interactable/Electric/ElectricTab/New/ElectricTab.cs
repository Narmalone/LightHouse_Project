using AYellowpaper.SerializedCollections;
using LightHouse.Features.Electricity;
using UnityEngine;

public class ElectricTab : MonoBehaviour
{
    [SerializeField] private SerializedDictionary<ElectricityZones, ElectricalSlot> _electricalSlots;
    

    public ElectricalSlot GetElectricalSlot(ElectricityZones zone)
    {
        if (_electricalSlots.TryGetValue(zone, out ElectricalSlot slots))
        {
            return slots;
        }
        return null;
    }
}
