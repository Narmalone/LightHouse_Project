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

        /// <summary>
        /// When the zone of this item is enabled
        /// </summary>
        void OnElectricityZoneEnabled();

        /// <summary>
        /// When the zone of this item is disabled
        /// </summary>
        void OnElectricityZoneDisabled();

        /// <summary>
        /// You can put the logic when you want to the item be enabled
        /// You can call it where / when you want
        /// </summary>
        void UserTurnOn();

        /// <summary>
        /// You can put the logic when you want to the item be disabled
        /// You can call it where /when you want
        /// </summary>
        void UserTurnOff();
    }

}
