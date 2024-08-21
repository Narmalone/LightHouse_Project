using System.Collections.Generic;
using UnityEngine;

public class Room : MonoBehaviour
{
    [SerializeField] public Transform _electricityItemParent;
    [SerializeField] protected GameZone ElectricityRoom;
    public List<ElectricItem> ElectricityItems;

    [Header("--- EVENTS ---")]
    [Header("LISTENERS")]
    [SerializeField] private CustomEvent_ElectricZone _onElecZoneEnabled;
    [SerializeField] private CustomEvent_ElectricZone _onElecZoneDisabled;

    [Header("READ ONLY / DEBUG PURPOSE")]
    [SerializeField] protected bool IsElecItemsEnabled = false;

    protected virtual void Awake()
    {
        _onElecZoneEnabled.handle += _onElecZoneEnabled_handle;
        _onElecZoneDisabled.handle += _onElecZoneDisabled_handle;
    }

    private void _onElecZoneEnabled_handle(GameZone obj)
    {
        if (obj != ElectricityRoom) return;
        IsElecItemsEnabled = true;

        foreach(ElectricItem item in ElectricityItems)
        {
            item.HasElectricity = true;
            item.OnElecEnabled();
        }
    }

    private void _onElecZoneDisabled_handle(GameZone obj)
    {
        if (obj != ElectricityRoom) return;
        IsElecItemsEnabled = false;

        foreach (ElectricItem item in ElectricityItems)
        {
            item.HasElectricity = false;
            item.OnElecDisabled();
        }
    }

    protected virtual void OnDestroy()
    {
        _onElecZoneEnabled.handle -= _onElecZoneEnabled_handle;
        _onElecZoneDisabled.handle -= _onElecZoneDisabled_handle; ;
    }
}
