using System;
using LightHouse.Electricity;
using UnityEngine;

public class ElectricItemTest : MonoBehaviour, IElectricItem
{
    #region EVENTS
    public event Action<ElectricityZones, float> AddElectricityCostToManager;
    public event Action<ElectricityZones, float> RemoveElectricityCostToManager;
    #endregion

    #region SERIALIZED / PROPERTIES
    public bool HasElectricity { get; set; }
    [field: SerializeField] public float ElectricityCost { get; set; } = 10.0f;
    [field: SerializeField] public ElectricityZones ItemZone { get; set; } = ElectricityZones.None;
    #endregion

    #region UNITY'S LIFECYCLE
    private void Start() => ElectricItemRegistry.Register(this);
    private void OnDestroy() => ElectricItemRegistry.Unregister(this);
    #endregion

    #region IELECTRICITY
    public void OnElectricityZoneEnabled()
    {
        AddElectricityCostToManager?.Invoke(ItemZone, ElectricityCost);
    }

    public void OnElectricityZoneDisabled()
    {
        RemoveElectricityCostToManager?.Invoke(ItemZone, ElectricityCost);
    }

    public void UserTurnOff() { }
    public void UserTurnOn() { }
    #endregion
}
