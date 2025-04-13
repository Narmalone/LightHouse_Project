using System;

namespace LightHouse.Electricity
{
    public interface IElectricItem
    {
        public event Action<float> AddElectricityCostToManager;
        public event Action<float> RemoveElectricityCostToManager;
        public float ElectricityCost { get; set; }
        public bool IsElectricityOn { get; set; }
        public ElectricityZones ItemZone { get; set; }
        void OnElectricityZoneEnabled();
        void OnElectricityZoneDisabled();
    }

}
