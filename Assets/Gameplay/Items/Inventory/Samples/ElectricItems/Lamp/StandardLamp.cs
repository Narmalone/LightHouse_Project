using LightHouse.Electricity;
using System;
using UnityEngine;

public class StandardLamp : MonoBehaviour, IElectricItem
{
    [SerializeField] private Light _light;

    public event Action<float> AddElectricityCostToManager;
    public event Action<float> RemoveElectricityCostToManager;

    [field: SerializeField] public bool IsElectricityOn { get; set; }
    [field: SerializeField] public ElectricityZones ItemZone { get; set; }
    [field: SerializeField] public float ElectricityCost { get; set; } = 10.0f;

    private void Start()
    {
        //Important to register on start to let the manager subscribe to the event
        ElectricItemRegistry.Register(this); 
    }

    private void OnDestroy()
    {
        ElectricItemRegistry.Unregister(this);
    }

    public void OnElectricityZoneDisabled()
    {
        _light.gameObject.SetActive(false);
        RemoveElectricityCostToManager?.Invoke(ElectricityCost);
    }

    public void OnElectricityZoneEnabled()
    {
        _light.gameObject.SetActive(true);
        AddElectricityCostToManager?.Invoke(ElectricityCost);
    }
}
