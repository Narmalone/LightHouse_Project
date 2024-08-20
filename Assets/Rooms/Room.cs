using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Room : MonoBehaviour
{
    [SerializeField] private ElectricityZones _electricityRoom;
    [SerializeField] private CustomEvent _onElectricityCameBack;
    [SerializeField] private CustomEvent _onElectricityShutDown;
    public List<IElectricityItem> StaticItems;
    public bool IsElecItemEnabled = false;

    protected virtual void Awake()
    {
        _onElectricityCameBack.handle += _onElectricityCameBack_handle;
        _onElectricityShutDown.handle += _onElectricityShutDown_handle;
    }

    protected virtual void OnDestroy()
    {
        _onElectricityCameBack.handle -= _onElectricityCameBack_handle;
        _onElectricityShutDown.handle -= _onElectricityShutDown_handle;
    }

    private void _onElectricityShutDown_handle()
    {
        Debug.Log("shutdown static items in this room");
        IsElecItemEnabled = false;
    }

    private void _onElectricityCameBack_handle()
    {
        Debug.Log("Enable static items in this room");
        IsElecItemEnabled = true;
    }
}
