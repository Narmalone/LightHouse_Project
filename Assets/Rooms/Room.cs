using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Room : MonoBehaviour
{
    [SerializeField] private ElectricityZones _electricityRoom;
    [SerializeField] private CustomEvent_ElectricZone _onElecZoneEnabled;
    [SerializeField] private CustomEvent_ElectricZone _onElecZoneDisabled;
    public List<IElectricityItem> StaticItems;
    public bool IsElecItemEnabled = false;

    protected virtual void Awake()
    {
        _onElecZoneEnabled.handle += _onElecZoneEnabled_handle;
        _onElecZoneDisabled.handle += _onElecZoneDisabled_handle;
    }

    private void _onElecZoneDisabled_handle(ElectricityZones obj)
    {
        if (obj != _electricityRoom) return;
        IsElecItemEnabled = false;
    }

    private void _onElecZoneEnabled_handle(ElectricityZones obj)
    {
        if (obj != _electricityRoom) return;
        IsElecItemEnabled = true;
    }

    protected virtual void OnDestroy()
    {
        _onElecZoneEnabled.handle -= _onElecZoneEnabled_handle;
        _onElecZoneDisabled.handle -= _onElecZoneDisabled_handle; ;
    }
}
