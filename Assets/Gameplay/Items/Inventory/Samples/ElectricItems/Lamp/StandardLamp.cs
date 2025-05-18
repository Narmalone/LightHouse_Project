using System;
using UnityEngine;

namespace LightHouse.Electricity
{
    public class StandardLamp : MonoBehaviour, IElectricItem
    {
        [SerializeField] private Light _light;

        public event Action<ElectricityZones, float> AddElectricityCostToManager;
        public event Action<ElectricityZones, float> RemoveElectricityCostToManager;

        [field: SerializeField] public bool HasElectricity { get; set; }
        [field: SerializeField] public ElectricityZones ItemZone { get; set; }
        [field: SerializeField] public float ElectricityCost { get; set; } = 10.0f;

        private void Awake()
        {
            _light.gameObject.SetActive(false);
        }

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
            UserTurnOff();
        }
        public void OnElectricityZoneEnabled() { }

        public void UserTurnOn()
        {
            _light.gameObject.SetActive(true);
            AddElectricityCostToManager?.Invoke(ItemZone, ElectricityCost);
        }

        public void UserTurnOff()
        {
            _light.gameObject.SetActive(false);
            RemoveElectricityCostToManager?.Invoke(ItemZone, ElectricityCost);
        }
    }

}
