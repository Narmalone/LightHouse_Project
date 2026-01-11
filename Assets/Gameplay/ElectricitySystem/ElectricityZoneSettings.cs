using UnityEngine;
using System;
using System.Collections.Generic;

namespace LightHouse.Features.Electricity
{
    [CreateAssetMenu(fileName = "ElectricityZoneSettings", menuName = "LightHouse/Electricity/Electricity Zone Settings")]
    public class ElectricityZoneSettings : ScriptableObject
    {
        [Serializable]
        public struct ZonePower
        {
            public ElectricityZones Zone;
            public float MaxPower;
        }

        [SerializeField]
        public List<ZonePower> zonePowers = new();

        private Dictionary<ElectricityZones, float> _lookup;

        public float GetMaxPower(ElectricityZones zone)
        {
            if (_lookup == null)
            {
                _lookup = new();
                foreach (var z in zonePowers)
                {
                    _lookup[z.Zone] = z.MaxPower;
                }
            }

            return _lookup.TryGetValue(zone, out var power) ? power : 0f;
        }
    }

}
