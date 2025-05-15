using System;

namespace LightHouse.Electricity
{
    public interface IElectricItem
    {
        public event Action<ElectricityZones, float> AddElectricityCostToManager;
        public event Action<ElectricityZones, float> RemoveElectricityCostToManager;
        public float ElectricityCost { get; set; }
        public bool IsElectricityOn { get; set; }
        public ElectricityZones ItemZone { get; set; }
        void OnElectricityZoneEnabled();
        void OnElectricityZoneDisabled();
    }

}
