using LightHouse.Features.Electricity;
using System;
using UnityEngine;

public class CeilingLight : MonoBehaviour, IElectricItem
{
    #region EVENTS

    public event Action<ElectricityZones, float> AddElectricityCostToManager;
    public event Action<ElectricityZones, float> RemoveElectricityCostToManager;

    #endregion


    #region SERIALIZED FIELDS

    [Header("Electricity")]
    [field: SerializeField] public float ElectricityCost { get; set; } = 20f;
    [field: SerializeField] public ElectricityZones ItemZone { get; set; } = ElectricityZones.GuardianHouse;

    [Header("Light")]
    [SerializeField] private Light _light;

    #endregion


    #region PROPERTIES

    public bool HasElectricity { get; set; }

    public bool IsTurnedOn { get; private set; }

    #endregion


    #region UNITY LIFECYCLE

    private void Start()
    {
        ElectricItemRegistry.Register(this);

        UpdateLightState();
    }

    private void OnDestroy()
    {
        ElectricItemRegistry.Unregister(this);
    }

    #endregion


    #region IELECTRICITEM

    public void OnElectricityZoneEnabled()
    {
        HasElectricity = true;

        UpdateLightState();

        if (IsTurnedOn)
        {
            AddElectricityCostToManager?.Invoke(ItemZone, ElectricityCost);
        }
    }

    public void OnElectricityZoneDisabled()
    {
        if (HasElectricity && IsTurnedOn)
        {
            RemoveElectricityCostToManager?.Invoke(ItemZone, ElectricityCost);
        }

        HasElectricity = false;

        UpdateLightState();
    }

    public void UserTurnOn()
    {
        if (IsTurnedOn)
            return;

        IsTurnedOn = true;

        if (HasElectricity)
        {
            AddElectricityCostToManager?.Invoke(ItemZone, ElectricityCost);
        }

        UpdateLightState();
    }

    public void UserTurnOff()
    {
        if (!IsTurnedOn)
            return;

        if (HasElectricity)
        {
            RemoveElectricityCostToManager?.Invoke(ItemZone, ElectricityCost);
        }

        IsTurnedOn = false;

        UpdateLightState();
    }

    #endregion


    #region LIGHT

    private void UpdateLightState()
    {
        if (_light == null)
            return;

        _light.enabled = HasElectricity && IsTurnedOn;
    }

    #endregion
}