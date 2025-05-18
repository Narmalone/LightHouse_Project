using System;

namespace LightHouse.Electricity
{
    public interface IElectricItem
    {
        /// <summary>
        /// Call it when the item is using Electricity
        /// </summary>
        public event Action<ElectricityZones, float> AddElectricityCostToManager;

        /// <summary>
        /// Call it when the item stops to use the electricity
        /// </summary>
        public event Action<ElectricityZones, float> RemoveElectricityCostToManager;

        /// <summary>
        /// If the item receive electricity
        /// </summary>
        public bool HasElectricity { get; set; }

        /// <summary>
        /// The cost of the item to it's Zone
        /// </summary>
        public float ElectricityCost { get; set; }

        /// <summary>
        /// The zone where the item belongs to
        /// </summary>
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
