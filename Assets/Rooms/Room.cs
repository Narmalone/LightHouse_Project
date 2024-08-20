using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Room : MonoBehaviour
{
    [SerializeField] private Transform _electricityItemParent;
    [SerializeField] protected ElectricityZones ElectricityRoom;

    [Header("--- EVENTS ---")]
    [Header("LISTENERS")]
    [SerializeField] private CustomEvent_ElectricZone _onElecZoneEnabled;
    [SerializeField] private CustomEvent_ElectricZone _onElecZoneDisabled;
    protected List<IElectricityItem> StaticItems = new List<IElectricityItem>();

    [Header("READ ONLY / DEBUG PURPOSE")]
    [SerializeField] protected bool IsElecItemsEnabled = false;

    protected virtual void Awake()
    {
        _onElecZoneEnabled.handle += _onElecZoneEnabled_handle;
        _onElecZoneDisabled.handle += _onElecZoneDisabled_handle;
    }

    private void _onElecZoneDisabled_handle(ElectricityZones obj)
    {
        if (obj != ElectricityRoom) return;
        IsElecItemsEnabled = false;
    }

    private void _onElecZoneEnabled_handle(ElectricityZones obj)
    {
        if (obj != ElectricityRoom) return;
        IsElecItemsEnabled = true;
    }

    protected virtual void OnDestroy()
    {
        _onElecZoneEnabled.handle -= _onElecZoneEnabled_handle;
        _onElecZoneDisabled.handle -= _onElecZoneDisabled_handle; ;
    }

    public void GetAllElecItemInRoom()
    {
        StaticItems = new List<IElectricityItem>();
        for(int i = 0; i < _electricityItemParent.childCount; i++)
        {
            if (_electricityItemParent.GetChild(i).TryGetComponent(out IElectricityItem item))
                StaticItems.Add(item);
        }

        Debug.Log(StaticItems.Count);
    }

    public void DebugForTest()
    {
        Debug.Log(StaticItems.Count);

    }
}
