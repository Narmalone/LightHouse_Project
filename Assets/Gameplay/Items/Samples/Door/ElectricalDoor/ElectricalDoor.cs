using LightHouse.Interactions.Samples;
using LightHouse.Electricity;
using UnityEngine;
using System;

namespace LightHouse.Items.Samples
{
    public class ElectricalDoor : Door, IElectricItem
    {
        [field: SerializeField] public float ElectricityCost { get; set; }
        [field: SerializeField] public bool IsElectricityOn { get; set; }
        [field: SerializeField] public ElectricityZones ItemZone { get; set; }

        public event Action<float> AddElectricityCostToManager;
        public event Action<float> RemoveElectricityCostToManager;

        public override string GetInteractionName()
        {
            if (!CanBeInteracted) return $"Need electricity on {ItemZone}";
            return base.GetInteractionName();
        }

        private void Start()
        {
            CanBeInteracted = false;
            ElectricItemRegistry.Register(this);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            ElectricItemRegistry.Unregister(this);
        }

        public void OnElectricityZoneEnabled()
        {
            CanBeInteracted = true;
            AddElectricityCostToManager?.Invoke(ElectricityCost);
        }

        public void OnElectricityZoneDisabled()
        {
            CanBeInteracted = false;
            RemoveElectricityCostToManager?.Invoke(ElectricityCost);
        }
    }
}

